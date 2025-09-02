using System;
using System.Threading;
using System.Threading.Tasks;
using Application.DTOs.Courses;
using Domain.Entities;
using Domain.Interfaces;
using AutoMapper;

namespace Application.UseCases
{
    public class CreateCourseHandler
    {
        private readonly ICourseRepository _courseRepo;
        private readonly IUserRepository _userRepo;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public CreateCourseHandler(ICourseRepository courseRepo, IUserRepository userRepo, IUnitOfWork uow, IMapper mapper)
        {
            _courseRepo = courseRepo;
            _userRepo = userRepo;
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<Guid> HandleAsync(CreateCourseDto dto, CancellationToken ct = default)
        {
            // Validaciones básicas
            var instructor = await _userRepo.GetByIdActiveAsync(dto.InstructorId, ct);
            if (instructor == null) throw new InvalidOperationException("Instructor no encontrado o inactivo.");

            // Map DTO -> entidad
            var course = _mapper.Map<Course>(dto);
            course.Id = Guid.NewGuid();
            course.CreatedAt = DateTime.UtcNow;

            await _courseRepo.AddAsync(course, ct);
            await _uow.SaveChangesAsync(ct);

            return course.Id;
        }
    }
}
