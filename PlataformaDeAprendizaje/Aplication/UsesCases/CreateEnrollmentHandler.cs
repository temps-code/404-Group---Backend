using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.DTOs.Enrollments;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.UseCases.Enrollments
{
    public class CreateEnrollmentHandler
    {
        private readonly IEnrollmentRepository _enrollmentRepo;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepo;
        private readonly ICourseRepository _courseRepo;

        public CreateEnrollmentHandler(
            IEnrollmentRepository enrollmentRepo,
            IUnitOfWork uow,
            IMapper mapper,
            IUserRepository userRepo,
            ICourseRepository courseRepo)
        {
            _enrollmentRepo = enrollmentRepo;
            _uow = uow;
            _mapper = mapper;
            _userRepo = userRepo;
            _courseRepo = courseRepo;
        }

        public async Task<EnrollmentDto> HandleAsync(CreateEnrollmentDto dto, CancellationToken ct = default)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            // 1) Validar existencia de usuario
            var user = await _userRepo.GetByIdIncludeInactiveAsync(dto.UserId, ct);
            if (user == null) throw new KeyNotFoundException($"User not found: {dto.UserId}");
            if (!user.IsActive) throw new InvalidOperationException("User is inactive.");

            // 2) Validar existencia de curso
            var course = await _courseRepo.GetByIdIncludeInactiveAsync(dto.CourseId, ct);
            if (course == null) throw new KeyNotFoundException($"Course not found: {dto.CourseId}");
            if (!course.IsActive) throw new InvalidOperationException("Course is inactive.");

            // 3) Evitar duplicados (inscripciones activas)
            var actives = await _enrollmentRepo.GetAllActiveAsync(ct);
            if (actives.Any(e => e.CourseId == dto.CourseId && e.UserId == dto.UserId))
                throw new InvalidOperationException("User already enrolled in this course.");

            // 4) Crear inscripción
            var enrollment = new Enrollment
            {
                Id = Guid.NewGuid(),
                CourseId = dto.CourseId,
                UserId = dto.UserId,
                Status = "active",
                EnrolledAt = DateTime.UtcNow,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _enrollmentRepo.AddAsync(enrollment, ct);
            await _uow.SaveChangesAsync(ct);

            return _mapper.Map<EnrollmentDto>(enrollment);
        }
    }
}
