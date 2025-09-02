using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.DTOs.Courses;
using Application.UseCases;
using Domain.Entities;
using Domain.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoursesController : ControllerBase
    {
        private readonly CreateCourseHandler _createHandler;
        private readonly ICourseRepository _courseRepo;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public CoursesController(CreateCourseHandler createHandler, ICourseRepository courseRepo, IUnitOfWork uow, IMapper mapper)
        {
            _createHandler = createHandler;
            _courseRepo = courseRepo;
            _uow = uow;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCourseDto dto, CancellationToken ct)
        {
            try
            {
                var id = await _createHandler.HandleAsync(dto, ct);
                return CreatedAtAction(nameof(Get), new { id }, null);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id, CancellationToken ct)
        {
            var course = await _courseRepo.GetByIdWithContentAsync(id, ct);
            if (course == null || !course.IsActive) return NotFound();
            var dto = _mapper.Map<CourseDto>(course);
            return Ok(dto);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var list = await _courseRepo.GetAllWithContentAsync(ct); // ahora incluye instructor, módulos, resources
            var dtos = list.Select(c => _mapper.Map<CourseDto>(c));
            return Ok(dtos);
        }


        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCourseDto dto, CancellationToken ct)
        {
            var course = await _courseRepo.GetByIdIncludeInactiveAsync(id, ct);
            if (course == null) return NotFound();

            course.Title = dto.Title ?? course.Title;
            course.Description = dto.Description ?? course.Description;
            course.IsPublished = dto.IsPublished ?? course.IsPublished;
            course.UpdatedAt = DateTime.UtcNow;

            _courseRepo.Update(course);
            await _uow.SaveChangesAsync(ct);
            return NoContent();
        }

        [HttpPost("{id:guid}/deactivate")]
        public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
        {
            var ok = await _courseRepo.DeactivateAsync(id, ct);
            if (!ok) return NotFound();
            await _uow.SaveChangesAsync(ct);
            return NoContent();
        }

        [HttpPost("{id:guid}/reactivate")]
        public async Task<IActionResult> Reactivate(Guid id, CancellationToken ct)
        {
            var ok = await _courseRepo.ReactivateAsync(id, ct);
            if (!ok) return NotFound();
            await _uow.SaveChangesAsync(ct);
            return NoContent();
        }
    }
}
