// Infrastructure/Repositories/SqlCourseModuleRepository.cs
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
    public class SqlCourseModuleRepository : BaseRepository<CourseModule>, ICourseModuleRepository
    {
        public SqlCourseModuleRepository(AppDbContext ctx) : base(ctx) { }

        public async Task<IEnumerable<CourseModule>> GetByCourseAsync(Guid courseId, CancellationToken ct = default)
        {
            return await _ctx.CourseModules
                             .Where(m => m.CourseId == courseId)
                             .ToListAsync(ct);
        }

        public async Task<IEnumerable<CourseModule>> GetByCourseOrderedAsync(Guid courseId, CancellationToken ct = default)
        {
            return await _ctx.CourseModules
                             .Where(m => m.CourseId == courseId)
                             .OrderBy(m => m.OrderIndex)
                             .ToListAsync(ct);
        }
    }
}
