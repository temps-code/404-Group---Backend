using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.DTOs.Submissions;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.UseCases.Submissions
{
    public class CreateSubmissionHandler
    {
        private readonly ISubmissionRepository _submissionRepo;
        private readonly IEvaluationRepository _evaluationRepo;
        private readonly IUserRepository _userRepo;
        private readonly IEnrollmentRepository _enrollmentRepo;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public CreateSubmissionHandler(
            ISubmissionRepository submissionRepo,
            IEvaluationRepository evaluationRepo,
            IUserRepository userRepo,
            IEnrollmentRepository enrollmentRepo,
            IUnitOfWork uow,
            IMapper mapper)
        {
            _submissionRepo = submissionRepo;
            _evaluationRepo = evaluationRepo;
            _userRepo = userRepo;
            _enrollmentRepo = enrollmentRepo;
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<SubmissionDto> HandleAsync(CreateSubmissionDto dto, CancellationToken ct = default)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Content)) throw new ArgumentException("Content is required.");

            // 1) validar evaluation
            var evaluation = await _evaluationRepo.GetByIdIncludeInactiveAsync(dto.EvaluationId, ct);
            if (evaluation == null) throw new KeyNotFoundException($"Evaluation not found: {dto.EvaluationId}");
            if (!evaluation.IsActive) throw new InvalidOperationException("Evaluation is inactive.");
            // opcional: exigir que esté publicado para aceptar envíos
            if (!evaluation.IsPublished) throw new InvalidOperationException("Evaluation is not published.");

            // 2) validar student
            var student = await _userRepo.GetByIdIncludeInactiveAsync(dto.StudentId, ct);
            if (student == null) throw new KeyNotFoundException($"Student not found: {dto.StudentId}");
            if (!student.IsActive) throw new InvalidOperationException("Student is inactive.");

            // 3) opcional: validar que el estudiante esté inscrito en el curso de la evaluación
            var enrollments = await _enrollmentRepo.GetAllActiveAsync(ct);
            var isEnrolled = enrollments.Any(e => e.CourseId == evaluation.CourseId && e.UserId == dto.StudentId);
            if (!isEnrolled) throw new InvalidOperationException("Student is not enrolled in the evaluation's course.");

            // 4) calcular AttemptNumber si no se proveyó (o validar coherencia)
            int attemptNumber = dto.AttemptNumber > 0 ? dto.AttemptNumber
                                                      : (await _submissionRepo.GetByEvaluationAsync(dto.EvaluationId, ct))
                                                            .Where(s => s.StudentId == dto.StudentId)
                                                            .Max(s => (int?)s.AttemptNumber) != null
                                                            ? (await _submissionRepo.GetByEvaluationAsync(dto.EvaluationId, ct))
                                                                .Where(s => s.StudentId == dto.StudentId).Max(s => s.AttemptNumber) + 1
                                                            : 1;

            // 5) marcar SubmittedAt y IsLate según CloseAt (si CloseAt existe)
            var now = DateTime.UtcNow;
            bool isLate = false;
            if (evaluation.CloseAt.HasValue && now > evaluation.CloseAt.Value)
            {
                isLate = true;
            }

            var submission = new Submission
            {
                Id = Guid.NewGuid(),
                EvaluationId = dto.EvaluationId,
                StudentId = dto.StudentId,
                AttemptNumber = attemptNumber,
                Content = dto.Content,
                FileUrl = dto.FileUrl,
                SubmittedAt = now,
                IsLate = isLate,
                Status = "submitted",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _submissionRepo.AddAsync(submission, ct);
            await _uow.SaveChangesAsync(ct);

            return _mapper.Map<SubmissionDto>(submission);
        }
    }
}
