// Application/UseCases/CourseModules/CreateCourseModuleHandler.cs
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.DTOs.CourseModules;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.UseCases.CourseModules
{
    public class CreateCourseModuleHandler
    {
        private readonly ICourseModuleRepository _moduleRepo;
        private readonly ICourseRepository _courseRepo;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public CreateCourseModuleHandler(
            ICourseModuleRepository moduleRepo,
            ICourseRepository courseRepo,
            IUnitOfWork uow,
            IMapper mapper)
        {
            _moduleRepo = moduleRepo;
            _courseRepo = courseRepo;
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<CourseModuleDto> HandleAsync(CreateCourseModuleDto dto, CancellationToken ct = default)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Title)) throw new ArgumentException("Title is required.");

            // validar existencia del curso
            var course = await _courseRepo.GetByIdIncludeInactiveAsync(dto.CourseId, ct);
            if (course == null) throw new KeyNotFoundException($"Course not found: {dto.CourseId}");
            if (!course.IsActive) throw new InvalidOperationException("Course is inactive.");

            // validar conflicto de OrderIndex usando el método existente GetByCourseAsync
            var existingModules = await _moduleRepo.GetByCourseAsync(dto.CourseId, ct);
            if (existingModules != null && existingModules.Any(m => m.OrderIndex == dto.OrderIndex && m.IsActive))
                throw new InvalidOperationException("There is already an active module with the same OrderIndex for this course.");

            var module = new CourseModule
            {
                Id = Guid.NewGuid(),
                CourseId = dto.CourseId,
                Title = dto.Title,
                OrderIndex = dto.OrderIndex,
                Description = dto.Description ?? string.Empty,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _moduleRepo.AddAsync(module, ct);
            await _uow.SaveChangesAsync(ct);

            return _mapper.Map<CourseModuleDto>(module);
        }
    }
}
