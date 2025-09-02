using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.DTOs.Resources;
using Application.UseCases.Resources;
using AutoMapper;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResourcesController : ControllerBase
    {
        private readonly CreateResourceHandler _createHandler;
        private readonly IResourceRepository _repo;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public ResourcesController(
            CreateResourceHandler createHandler,
            IResourceRepository repo,
            IUnitOfWork uow,
            IMapper mapper)
        {
            _createHandler = createHandler ?? throw new ArgumentNullException(nameof(createHandler));
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        // POST /api/resources
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateResourceDto dto, CancellationToken ct)
        {
            if (dto == null) return BadRequest(new { message = "Request body required." });

            try
            {
                var created = await _createHandler.HandleAsync(dto, ct);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (KeyNotFoundException knf) { return NotFound(new { message = knf.Message }); }
            catch (InvalidOperationException iop) { return BadRequest(new { message = iop.Message }); }
            catch (ArgumentException aex) { return BadRequest(new { message = aex.Message }); }
        }

        // GET /api/resources/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var entity = await _repo.GetByIdIncludeInactiveAsync(id, ct);
            if (entity == null) return NotFound();
            return Ok(_mapper.Map<ResourceDto>(entity));
        }

        // GET /api/resources -> todos
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var all = await _repo.GetAllAsync(ct);
            return Ok(all.Select(r => _mapper.Map<ResourceDto>(r)));
        }

        // GET /api/resources/active
        [HttpGet("active")]
        public async Task<IActionResult> GetActive(CancellationToken ct)
        {
            var list = await _repo.GetAllActiveAsync(ct);
            return Ok(list.Select(r => _mapper.Map<ResourceDto>(r)));
        }

        // GET /api/resources/inactive
        [HttpGet("inactive")]
        public async Task<IActionResult> GetInactive(CancellationToken ct)
        {
            var list = await _repo.GetAllInactiveAsync(ct);
            return Ok(list.Select(r => _mapper.Map<ResourceDto>(r)));
        }

        // GET /api/resources/course/{courseId}
        [HttpGet("course/{courseId:guid}")]
        public async Task<IActionResult> GetByCourse(Guid courseId, CancellationToken ct)
        {
            var list = await _repo.GetByCourseAsync(courseId, ct);
            return Ok(list.Select(r => _mapper.Map<ResourceDto>(r)));
        }

        // GET /api/resources/module/{moduleId}
        [HttpGet("module/{moduleId:guid}")]
        public async Task<IActionResult> GetByModule(Guid moduleId, CancellationToken ct)
        {
            var list = await _repo.GetByModuleAsync(moduleId, ct);
            return Ok(list.Select(r => _mapper.Map<ResourceDto>(r)));
        }

        // GET /api/resources/uploader/{uploaderId}
        [HttpGet("uploader/{uploaderId:guid}")]
        public async Task<IActionResult> GetByUploader(Guid uploaderId, CancellationToken ct)
        {
            var list = await _repo.GetByUploaderAsync(uploaderId, ct);
            return Ok(list.Select(r => _mapper.Map<ResourceDto>(r)));
        }

        // PUT /api/resources/{id} -> actualizar nombre/url/isActive
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateResourceDto dto, CancellationToken ct)
        {
            var existing = await _repo.GetByIdIncludeInactiveAsync(id, ct);
            if (existing == null) return NotFound();

            existing.Name = dto.Name ?? existing.Name;
            existing.Url = dto.Url ?? existing.Url;
            if (dto.IsActive.HasValue) existing.IsActive = dto.IsActive.Value;
            existing.UpdatedAt = DateTime.UtcNow;

            _repo.Update(existing);
            await _uow.SaveChangesAsync(ct);
            return NoContent();
        }

        // POST /api/resources/{id}/deactivate
        [HttpPost("{id:guid}/deactivate")]
        public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
        {
            var ok = await _repo.DeactivateAsync(id, ct);
            if (!ok) return NotFound(new { message = "Resource not found or already inactive." });
            await _uow.SaveChangesAsync(ct);
            return NoContent();
        }

        // POST /api/resources/{id}/reactivate
        [HttpPost("{id:guid}/reactivate")]
        public async Task<IActionResult> Reactivate(Guid id, CancellationToken ct)
        {
            var ok = await _repo.ReactivateAsync(id, ct);
            if (!ok) return NotFound(new { message = "Resource not found or already active." });
            await _uow.SaveChangesAsync(ct);
            return NoContent();
        }
    }
}
