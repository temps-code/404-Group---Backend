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
    public class SqlCourseRepository : BaseRepository<Course>, ICourseRepository
    {
        public SqlCourseRepository(AppDbContext ctx) : base(ctx) { }

        public async Task<Course?> GetByIdWithContentAsync(Guid id, CancellationToken ct = default)
        {
            var course = await _ctx.Courses
                         .Include(c => c.Instructor)                          // incluir instructor
                         .Include(c => c.Modules)
                             .ThenInclude(m => m.Resources)
                         .Include(c => c.Resources)
                             .ThenInclude(r => r.UploadedBy)                 // opcional: subir info del uploader
                         .Include(c => c.Evaluations)
                         .FirstOrDefaultAsync(c => c.Id == id && c.IsActive, ct);

            if (course != null && course.Modules != null)
            {
                // ordenar los módulos por OrderIndex (en memoria)
                course.Modules = course.Modules.OrderBy(m => m.OrderIndex).ToList();
            }

            return course;
        }

        public async Task<IEnumerable<Course>> GetPublishedAsync(CancellationToken ct = default)
            => await _ctx.Courses.Where(c => c.IsPublished && c.IsActive).ToListAsync(ct);

        public async Task<IEnumerable<Course>> GetByInstructorAsync(Guid instructorId, CancellationToken ct = default)
            => await _ctx.Courses.Where(c => c.InstructorId == instructorId && c.IsActive).ToListAsync(ct);

        // Nuevo: trae todos los cursos activos con contenido relevante (instructor, módulos, recursos)
        public async Task<IEnumerable<Course>> GetAllWithContentAsync(CancellationToken ct = default)
        {
            var courses = await _ctx.Courses
                .Where(c => c.IsActive)
                .Include(c => c.Instructor)
                .Include(c => c.Modules)
                    .ThenInclude(m => m.Resources)
                .Include(c => c.Resources)
                    .ThenInclude(r => r.UploadedBy)
                .Include(c => c.Evaluations)
                .ToListAsync(ct);

            // Ordenar módulos por OrderIndex en cada curso
            foreach (var c in courses)
            {
                if (c.Modules != null)
                    c.Modules = c.Modules.OrderBy(m => m.OrderIndex).ToList();
            }

            return courses;
        }
    }
}
