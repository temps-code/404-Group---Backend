using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.DTOs.Enrollments;
using Application.UseCases.Enrollments;
using AutoMapper;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnrollmentsController : ControllerBase
    {
        private readonly CreateEnrollmentHandler _createHandler;
        private readonly IEnrollmentRepository _repo;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public EnrollmentsController(
            CreateEnrollmentHandler createHandler,
            IEnrollmentRepository repo,
            IUnitOfWork uow,
            IMapper mapper)
        {
            _createHandler = createHandler ?? throw new ArgumentNullException(nameof(createHandler));
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Crear inscripción
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEnrollmentDto dto, CancellationToken ct)
        {
            if (dto == null) return BadRequest(new { message = "Request body is required." });

            try
            {
                var created = await _createHandler.HandleAsync(dto, ct);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (KeyNotFoundException knf)
            {
                return NotFound(new { message = knf.Message });
            }
            catch (InvalidOperationException iop)
            {
                return BadRequest(new { message = iop.Message });
            }
            catch (ArgumentException aex)
            {
                return BadRequest(new { message = aex.Message });
            }
        }

        /// <summary>
        /// Obtener inscripción por id (incluye inactivos)
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var entity = await _repo.GetByIdIncludeInactiveAsync(id, ct);
            if (entity == null) return NotFound();
            var dto = _mapper.Map<EnrollmentDto>(entity);
            return Ok(dto);
        }

        /// <summary>
        /// Listar todos (activos + inactivos)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var all = await _repo.GetAllAsync(ct);
            var dtos = all.Select(e => _mapper.Map<EnrollmentDto>(e));
            return Ok(dtos);
        }

        /// <summary>
        /// Listar activos
        /// </summary>
        [HttpGet("active")]
        public async Task<IActionResult> GetActive(CancellationToken ct)
        {
            var active = await _repo.GetAllActiveAsync(ct);
            var dtos = active.Select(e => _mapper.Map<EnrollmentDto>(e));
            return Ok(dtos);
        }

        /// <summary>
        /// Listar inactivos
        /// </summary>
        [HttpGet("inactive")]
        public async Task<IActionResult> GetInactive(CancellationToken ct)
        {
            var inactive = await _repo.GetAllInactiveAsync(ct);
            var dtos = inactive.Select(e => _mapper.Map<EnrollmentDto>(e));
            return Ok(dtos);
        }

        /// <summary>
        /// Desactivar (soft-delete)
        /// </summary>
        [HttpPost("{id:guid}/deactivate")]
        public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
        {
            var ok = await _repo.DeactivateAsync(id, ct);
            if (!ok) return NotFound(new { message = "Enrollment not found or already inactive." });

            await _uow.SaveChangesAsync(ct);
            return NoContent();
        }

        /// <summary>
        /// Reactivar
        /// </summary>
        [HttpPost("{id:guid}/reactivate")]
        public async Task<IActionResult> Reactivate(Guid id, CancellationToken ct)
        {
            var ok = await _repo.ReactivateAsync(id, ct);
            if (!ok) return NotFound(new { message = "Enrollment not found or already active." });

            await _uow.SaveChangesAsync(ct);
            return NoContent();
        }
    }
}
