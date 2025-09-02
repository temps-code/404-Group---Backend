using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.DTOs.Users;
using Application.UseCases;
using Domain.Entities;
using Domain.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly CreateUserHandler _createHandler;
        private readonly IUserRepository _userRepo;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public UsersController(
            CreateUserHandler createHandler,
            IUserRepository userRepo,
            IUnitOfWork uow,
            IMapper mapper)
        {
            _createHandler = createHandler;
            _userRepo = userRepo;
            _uow = uow;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserDto dto, CancellationToken ct)
        {
            try
            {
                var created = await _createHandler.HandleAsync(dto, ct);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var user = await _userRepo.GetByIdAsync(id, ct);
            if (user == null || !user.IsActive) return NotFound();
            var dto = _mapper.Map<UserDto>(user);
            return Ok(dto);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var list = await _userRepo.GetAllActiveAsync(ct);
            var dtos = list.Select(u => _mapper.Map<UserDto>(u));
            return Ok(dtos);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto dto, CancellationToken ct)
        {
            var existing = await _userRepo.GetByIdIncludeInactiveAsync(id, ct);
            if (existing == null) return NotFound();

            // Mapear campos que se permiten actualizar (puedes ajustar)
            existing.FirstName = dto.FirstName ?? existing.FirstName;
            existing.LastName = dto.LastName ?? existing.LastName;
            existing.Email = dto.Email ?? existing.Email;
            existing.UpdatedAt = DateTime.UtcNow;

            _userRepo.Update(existing);
            await _uow.SaveChangesAsync(ct);

            return NoContent();
        }

        [HttpPost("{id:guid}/deactivate")]
        public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
        {
            var ok = await _userRepo.DeactivateAsync(id, ct);
            if (!ok) return NotFound();
            await _uow.SaveChangesAsync(ct);
            return NoContent();
        }

        [HttpPost("{id:guid}/reactivate")]
        public async Task<IActionResult> Reactivate(Guid id, CancellationToken ct)
        {
            var ok = await _userRepo.ReactivateAsync(id, ct);
            if (!ok) return NotFound();
            await _uow.SaveChangesAsync(ct);
            return NoContent();
        }
    }
}
