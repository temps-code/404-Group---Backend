using System;
using System.Threading;
using System.Threading.Tasks;
using Application.DTOs.Evaluations;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.UseCases.Evaluations
{
    public class CreateEvaluationHandler
    {
        private readonly IEvaluationRepository _evaluationRepo;
        private readonly ICourseRepository _courseRepo;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public CreateEvaluationHandler(
            IEvaluationRepository evaluationRepo,
            ICourseRepository courseRepo,
            IUnitOfWork uow,
            IMapper mapper)
        {
            _evaluationRepo = evaluationRepo;
            _courseRepo = courseRepo;
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<EvaluationDto> HandleAsync(CreateEvaluationDto dto, CancellationToken ct = default)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Title)) throw new ArgumentException("Title is required.");
            if (dto.TotalPoints <= 0) throw new ArgumentException("TotalPoints must be greater than 0.");

            // validar existencia del curso
            var course = await _courseRepo.GetByIdIncludeInactiveAsync(dto.CourseId, ct);
            if (course == null) throw new KeyNotFoundException($"Course not found: {dto.CourseId}");
            if (!course.IsActive) throw new InvalidOperationException("Course is inactive.");

            var evaluation = new Evaluation
            {
                Id = Guid.NewGuid(),
                CourseId = dto.CourseId,
                Title = dto.Title,
                Description = dto.Description ?? string.Empty,
                TotalPoints = dto.TotalPoints,
                OpenAt = dto.OpenAt,
                CloseAt = dto.CloseAt,
                TimeLimitMinutes = dto.TimeLimitMinutes,
                IsPublished = false, // por defecto no publicado
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _evaluationRepo.AddAsync(evaluation, ct);
            await _uow.SaveChangesAsync(ct);

            return _mapper.Map<EvaluationDto>(evaluation);
        }
    }
}
