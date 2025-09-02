using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class SqlUserRepository : BaseRepository<User>, IUserRepository
    {
        public SqlUserRepository(AppDbContext ctx) : base(ctx) { }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            return await _ctx.Users
                             .AsNoTracking()
                             .FirstOrDefaultAsync(u => u.Email == email, ct);
        }

        // Implementación solicitada: GetByIdAsync
        // Esta puede incluir relaciones importantes si lo deseas (Enrollments, Submissions, etc.)
        public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _ctx.Users
                             .Include(u => u.Enrollments)   // opcional: incluye relaciones necesarias
                             .Include(u => u.ResourcesUploaded)
                             .AsNoTracking()
                             .FirstOrDefaultAsync(u => u.Id == id, ct);
        }

        // Implementación solicitada: GetByCourseAsync
        // Devuelve los usuarios que están inscritos (activos) en un curso determinado.
        public async Task<IEnumerable<User>> GetByCourseAsync(Guid courseId, CancellationToken ct = default)
        {
            // Unimos enrollments con users; filtramos por enrollments activos si quieres
            var users = await _ctx.Enrollments
                                  .AsNoTracking()
                                  .Where(e => e.CourseId == courseId && e.IsActive)
                                  .Select(e => e.User)
                                  .Where(u => u != null && u.IsActive)
                                  .Distinct()
                                  .ToListAsync(ct);

            return users!;
        }

        // Nota: GetByIdActiveAsync / GetByIdIncludeInactiveAsync ya vienen del BaseRepository.
        // Si quieres que GetByIdAsync respete "active only", cambia la consulta arriba para u.IsActive.
    }
}
