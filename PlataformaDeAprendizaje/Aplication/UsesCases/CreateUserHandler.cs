using System;
using System.Threading;
using System.Threading.Tasks;
using Application.DTOs.Users;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Application.UseCases
{
    public class CreateUserHandler
    {
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;

        public CreateUserHandler(UserManager<User> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<UserDto> HandleAsync(CreateUserDto dto, CancellationToken ct = default)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Email)) throw new ArgumentException("Email requerido");
            if (string.IsNullOrWhiteSpace(dto.Password)) throw new ArgumentException("Password requerido");

            var existing = await _userManager.FindByEmailAsync(dto.Email);
            if (existing != null) throw new InvalidOperationException("Ya existe un usuario con ese email.");

            var user = new User
            {
                Id = Guid.NewGuid(),
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Role = dto.Role ?? "Student",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors);
                throw new InvalidOperationException($"No se pudo crear el usuario: {errors}");
            }

            // opcional: asignar rol
            if (!string.IsNullOrWhiteSpace(user.Role))
            {
                await _userManager.AddToRoleAsync(user, user.Role);
            }

            return _mapper.Map<UserDto>(user);
        }
    }
}
