using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.DTOs.Evaluations;
using Application.UseCases.Evaluations;
using AutoMapper;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EvaluationsController : ControllerBase
    {
        private readonly CreateEvaluationHandler _createHandler;
        private readonly IEvaluationRepository _repo;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public EvaluationsController(
            CreateEvaluationHandler createHandler,
            IEvaluationRepository repo,
            IUnitOfWork uow,
            IMapper mapper)
        {
            _createHandler = createHandler ?? throw new ArgumentNullException(nameof(createHandler));
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        // POST /api/evaluations
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEvaluationDto dto, CancellationToken ct)
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

        // GET /api/evaluations/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var entity = await _repo.GetByIdIncludeInactiveAsync(id, ct);
            if (entity == null) return NotFound();
            return Ok(_mapper.Map<EvaluationDto>(entity));
        }

        // GET /api/evaluations -> todos
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var all = await _repo.GetAllAsync(ct);
            return Ok(all.Select(ev => _mapper.Map<EvaluationDto>(ev)));
        }

        // GET /api/evaluations/active
        [HttpGet("active")]
        public async Task<IActionResult> GetActive(CancellationToken ct)
        {
            var list = await _repo.GetAllActiveAsync(ct);
            return Ok(list.Select(ev => _mapper.Map<EvaluationDto>(ev)));
        }

        // GET /api/evaluations/inactive
        [HttpGet("inactive")]
        public async Task<IActionResult> GetInactive(CancellationToken ct)
        {
            var list = await _repo.GetAllInactiveAsync(ct);
            return Ok(list.Select(ev => _mapper.Map<EvaluationDto>(ev)));
        }

        // GET /api/evaluations/course/{courseId}
        [HttpGet("course/{courseId:guid}")]
        public async Task<IActionResult> GetByCourse(Guid courseId, CancellationToken ct)
        {
            var list = await _repo.GetByCourseAsync(courseId, ct);
            return Ok(list.Select(ev => _mapper.Map<EvaluationDto>(ev)));
        }

        // GET /api/evaluations/open
        [HttpGet("open")]
        public async Task<IActionResult> GetOpen(CancellationToken ct)
        {
            var list = await _repo.GetOpenEvaluationsAsync(ct);
            return Ok(list.Select(ev => _mapper.Map<EvaluationDto>(ev)));
        }

        // PUT /api/evaluations/{id} -> update (partial)
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEvaluationDto dto, CancellationToken ct)
        {
            var existing = await _repo.GetByIdIncludeInactiveAsync(id, ct);
            if (existing == null) return NotFound();

            existing.Title = dto.Title ?? existing.Title;
            existing.Description = dto.Description ?? existing.Description;
            existing.TotalPoints = dto.TotalPoints ?? existing.TotalPoints;
            existing.OpenAt = dto.OpenAt ?? existing.OpenAt;
            existing.CloseAt = dto.CloseAt ?? existing.CloseAt;
            existing.TimeLimitMinutes = dto.TimeLimitMinutes ?? existing.TimeLimitMinutes;
            if (dto.IsPublished.HasValue) existing.IsPublished = dto.IsPublished.Value;
            if (dto.IsActive.HasValue) existing.IsActive = dto.IsActive.Value;
            existing.UpdatedAt = DateTime.UtcNow;

            _repo.Update(existing);
            await _uow.SaveChangesAsync(ct);
            return NoContent();
        }

        // POST /api/evaluations/{id}/deactivate
        [HttpPost("{id:guid}/deactivate")]
        public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
        {
            var ok = await _repo.DeactivateAsync(id, ct);
            if (!ok) return NotFound(new { message = "Evaluation not found or already inactive." });
            await _uow.SaveChangesAsync(ct);
            return NoContent();
        }

        // POST /api/evaluations/{id}/reactivate}
        [HttpPost("{id:guid}/reactivate")]
        public async Task<IActionResult> Reactivate(Guid id, CancellationToken ct)
        {
            var ok = await _repo.ReactivateAsync(id, ct);
            if (!ok) return NotFound(new { message = "Evaluation not found or already active." });
            await _uow.SaveChangesAsync(ct);
            return NoContent();
        }

        // POST /api/evaluations/{id}/publish
        [HttpPost("{id:guid}/publish")]
        public async Task<IActionResult> Publish(Guid id, [FromBody] PublishEvaluationDto? dto, CancellationToken ct)
        {
            var existing = await _repo.GetByIdIncludeInactiveAsync(id, ct);
            if (existing == null) return NotFound(new { message = "Evaluation not found." });

            // Opcional: validar que el curso exista o que el user sea instructor
            if (dto != null)
            {
                if (dto.OpenAt.HasValue) existing.OpenAt = dto.OpenAt;
                if (dto.CloseAt.HasValue) existing.CloseAt = dto.CloseAt;
            }

            existing.IsPublished = true;
            existing.UpdatedAt = DateTime.UtcNow;

            _repo.Update(existing);
            await _uow.SaveChangesAsync(ct);
            return NoContent();
        }

        // POST /api/evaluations/{id}/unpublish
        [HttpPost("{id:guid}/unpublish")]
        public async Task<IActionResult> Unpublish(Guid id, CancellationToken ct)
        {
            var existing = await _repo.GetByIdIncludeInactiveAsync(id, ct);
            if (existing == null) return NotFound(new { message = "Evaluation not found." });

            existing.IsPublished = false;
            existing.UpdatedAt = DateTime.UtcNow;

            _repo.Update(existing);
            await _uow.SaveChangesAsync(ct);
            return NoContent();
        }
    }
}
