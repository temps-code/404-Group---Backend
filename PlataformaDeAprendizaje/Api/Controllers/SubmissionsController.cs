using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.DTOs.Submissions;
using Application.UseCases.Submissions;
using AutoMapper;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubmissionsController : ControllerBase
    {
        private readonly CreateSubmissionHandler _createHandler;
        private readonly ISubmissionRepository _repo;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public SubmissionsController(
            CreateSubmissionHandler createHandler,
            ISubmissionRepository repo,
            IUnitOfWork uow,
            IMapper mapper)
        {
            _createHandler = createHandler ?? throw new ArgumentNullException(nameof(createHandler));
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        // POST /api/submissions
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSubmissionDto dto, CancellationToken ct)
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

        // GET /api/submissions/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var entity = await _repo.GetByIdWithDetailsAsync(id, ct);
            if (entity == null) return NotFound();
            return Ok(_mapper.Map<SubmissionDto>(entity));
        }

        // GET /api/submissions -> todos
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var all = await _repo.GetAllAsync(ct);
            return Ok(all.Select(s => _mapper.Map<SubmissionDto>(s)));
        }

        // GET /api/submissions/active
        [HttpGet("active")]
        public async Task<IActionResult> GetActive(CancellationToken ct)
        {
            var list = await _repo.GetAllActiveAsync(ct);
            return Ok(list.Select(s => _mapper.Map<SubmissionDto>(s)));
        }

        // GET /api/submissions/inactive
        [HttpGet("inactive")]
        public async Task<IActionResult> GetInactive(CancellationToken ct)
        {
            var list = await _repo.GetAllInactiveAsync(ct);
            return Ok(list.Select(s => _mapper.Map<SubmissionDto>(s)));
        }

        // GET /api/submissions/evaluation/{evaluationId}
        [HttpGet("evaluation/{evaluationId:guid}")]
        public async Task<IActionResult> GetByEvaluation(Guid evaluationId, CancellationToken ct)
        {
            var list = await _repo.GetByEvaluationAsync(evaluationId, ct);
            return Ok(list.Select(s => _mapper.Map<SubmissionDto>(s)));
        }

        // GET /api/submissions/student/{studentId}
        [HttpGet("student/{studentId:guid}")]
        public async Task<IActionResult> GetByStudent(Guid studentId, CancellationToken ct)
        {
            var list = await _repo.GetByStudentAsync(studentId, ct);
            return Ok(list.Select(s => _mapper.Map<SubmissionDto>(s)));
        }

        // PUT /api/submissions/{id}/grade -> para que un profesor ponga Score y Feedback
        [HttpPut("{id:guid}/grade")]
        public async Task<IActionResult> Grade(Guid id, [FromBody] GradeSubmissionDto dto, CancellationToken ct)
        {
            var existing = await _repo.GetByIdIncludeInactiveAsync(id, ct);
            if (existing == null) return NotFound();

            if (dto.Score.HasValue)
            {
                existing.Score = dto.Score.Value;
            }
            existing.Feedback = dto.Feedback ?? existing.Feedback;
            existing.Status = dto.Status ?? existing.Status;
            existing.UpdatedAt = DateTime.UtcNow;

            _repo.Update(existing);
            await _uow.SaveChangesAsync(ct);
            return NoContent();
        }

        // PUT /api/submissions/{id} -> actualizar contenido/file (student puede usar)
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSubmissionDto dto, CancellationToken ct)
        {
            var existing = await _repo.GetByIdIncludeInactiveAsync(id, ct);
            if (existing == null) return NotFound();

            existing.Content = dto.Content ?? existing.Content;
            existing.FileUrl = dto.FileUrl ?? existing.FileUrl;
            existing.UpdatedAt = DateTime.UtcNow;

            _repo.Update(existing);
            await _uow.SaveChangesAsync(ct);
            return NoContent();
        }

        // POST /api/submissions/{id}/deactivate
        [HttpPost("{id:guid}/deactivate")]
        public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
        {
            var ok = await _repo.DeactivateAsync(id, ct);
            if (!ok) return NotFound(new { message = "Submission not found or already inactive." });
            await _uow.SaveChangesAsync(ct);
            return NoContent();
        }

        // POST /api/submissions/{id}/reactivate
        [HttpPost("{id:guid}/reactivate")]
        public async Task<IActionResult> Reactivate(Guid id, CancellationToken ct)
        {
            var ok = await _repo.ReactivateAsync(id, ct);
            if (!ok) return NotFound(new { message = "Submission not found or already active." });
            await _uow.SaveChangesAsync(ct);
            return NoContent();
        }
    }
}
