using System;
using System.Threading;
using System.Threading.Tasks;
using Application.DTOs.Resources;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.UseCases.Resources
{
    public class CreateResourceHandler
    {
        private readonly IResourceRepository _resourceRepo;
        private readonly ICourseRepository _courseRepo;
        private readonly ICourseModuleRepository _moduleRepo;
        private readonly IUserRepository _userRepo;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public CreateResourceHandler(
            IResourceRepository resourceRepo,
            ICourseRepository courseRepo,
            ICourseModuleRepository moduleRepo,
            IUserRepository userRepo,
            IUnitOfWork uow,
            IMapper mapper)
        {
            _resourceRepo = resourceRepo;
            _courseRepo = courseRepo;
            _moduleRepo = moduleRepo;
            _userRepo = userRepo;
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<ResourceDto> HandleAsync(CreateResourceDto dto, CancellationToken ct = default)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Type)) throw new ArgumentException("Type is required.");
            if (string.IsNullOrWhiteSpace(dto.Name)) throw new ArgumentException("Name is required.");
            if (string.IsNullOrWhiteSpace(dto.Url)) throw new ArgumentException("Url is required.");

            // Validar uploader (user)
            var uploader = await _userRepo.GetByIdIncludeInactiveAsync(dto.UploadedById, ct);
            if (uploader == null) throw new KeyNotFoundException($"Uploader not found: {dto.UploadedById}");
            if (!uploader.IsActive) throw new InvalidOperationException("Uploader is inactive.");

            // Validar curso y/o módulo si se proporcionan
            if (dto.CourseId.HasValue)
            {
                var course = await _courseRepo.GetByIdIncludeInactiveAsync(dto.CourseId.Value, ct);
                if (course == null) throw new KeyNotFoundException($"Course not found: {dto.CourseId}");
                if (!course.IsActive) throw new InvalidOperationException("Course is inactive.");
            }

            if (dto.ModuleId.HasValue)
            {
                var module = await _moduleRepo.GetByIdIncludeInactiveAsync(dto.ModuleId.Value, ct);
                if (module == null) throw new KeyNotFoundException($"Module not found: {dto.ModuleId}");
                if (!module.IsActive) throw new InvalidOperationException("Module is inactive.");
                // Opcional: verificar que module.CourseId == dto.CourseId si quieres coherencia
                if (dto.CourseId.HasValue && module.CourseId != dto.CourseId.Value)
                    throw new InvalidOperationException("Module does not belong to the provided CourseId.");
            }

            var resource = new Resource
            {
                Id = Guid.NewGuid(),
                Type = dto.Type,
                Name = dto.Name,
                Url = dto.Url,
                UploadedById = dto.UploadedById,
                CourseId = dto.CourseId,
                ModuleId = dto.ModuleId,
                UploadedAt = DateTime.UtcNow,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _resourceRepo.AddAsync(resource, ct);
            await _uow.SaveChangesAsync(ct);

            return _mapper.Map<ResourceDto>(resource);
        }
    }
}
