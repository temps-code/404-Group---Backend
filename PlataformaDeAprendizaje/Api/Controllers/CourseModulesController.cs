using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.DTOs.CourseModules;
using Application.UseCases.CourseModules;
using AutoMapper;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CourseModulesController : ControllerBase
    {
        private readonly CreateCourseModuleHandler _createHandler;
        private readonly ICourseModuleRepository _repo;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public CourseModulesController(
            CreateCourseModuleHandler createHandler,
            ICourseModuleRepository repo,
            IUnitOfWork uow,
            IMapper mapper)
        {
            _createHandler = createHandler ?? throw new ArgumentNullException(nameof(createHandler));
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        // POST /api/coursemodules
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCourseModuleDto dto, CancellationToken ct)
        {
            if (dto == null) return BadRequest(new { message = "Request body required." });

            try
            {
                var created = await _createHandler.HandleAsync(dto, ct);
                return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
            }
            catch (KeyNotFoundException knf) { return NotFound(new { message = knf.Message }); }
            catch (InvalidOperationException iop) { return BadRequest(new { message = iop.Message }); }
            catch (ArgumentException aex) { return BadRequest(new { message = aex.Message }); }
        }

        // GET /api/coursemodules/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id, CancellationToken ct)
        {
            var entity = await _repo.GetByIdIncludeInactiveAsync(id, ct);
            if (entity == null) return NotFound();
            var dto = _mapper.Map<CourseModuleDto>(entity);
            return Ok(dto);
        }

        // GET /api/coursemodules -> todos
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var all = await _repo.GetAllAsync(ct);
            var dtos = all.Select(m => _mapper.Map<CourseModuleDto>(m));
            return Ok(dtos);
        }

        // GET /api/coursemodules/active
        [HttpGet("active")]
        public async Task<IActionResult> GetActive(CancellationToken ct)
        {
            var list = await _repo.GetAllActiveAsync(ct);
            var dtos = list.Select(m => _mapper.Map<CourseModuleDto>(m));
            return Ok(dtos);
        }

        // GET /api/coursemodules/inactive
        [HttpGet("inactive")]
        public async Task<IActionResult> GetInactive(CancellationToken ct)
        {
            var list = await _repo.GetAllInactiveAsync(ct);
            var dtos = list.Select(m => _mapper.Map<CourseModuleDto>(m));
            return Ok(dtos);
        }

        // POST /api/coursemodules/{id}/deactivate
        [HttpPost("{id:guid}/deactivate")]
        public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
        {
            var ok = await _repo.DeactivateAsync(id, ct);
            if (!ok) return NotFound(new { message = "Module not found or already inactive." });
            await _uow.SaveChangesAsync(ct);
            return NoContent();
        }

        // POST /api/coursemodules/{id}/reactivate
        [HttpPost("{id:guid}/reactivate")]
        public async Task<IActionResult> Reactivate(Guid id, CancellationToken ct)
        {
            var ok = await _repo.ReactivateAsync(id, ct);
            if (!ok) return NotFound(new { message = "Module not found or already active." });
            await _uow.SaveChangesAsync(ct);
            return NoContent();
        }
    }
}
