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
    public class SqlResourceRepository : BaseRepository<Resource>, IResourceRepository
    {
        public SqlResourceRepository(AppDbContext ctx) : base(ctx) { }

        public async Task<IEnumerable<Resource>> GetByCourseAsync(Guid courseId, CancellationToken ct = default)
            => await _ctx.Resources.Where(r => r.CourseId == courseId && r.IsActive).ToListAsync(ct);

        public async Task<IEnumerable<Resource>> GetByModuleAsync(Guid moduleId, CancellationToken ct = default)
            => await _ctx.Resources.Where(r => r.ModuleId == moduleId && r.IsActive).ToListAsync(ct);

        public async Task<IEnumerable<Resource>> GetByUploaderAsync(Guid uploaderUserId, CancellationToken ct = default)
            => await _ctx.Resources.Where(r => r.UploadedById == uploaderUserId && r.IsActive).ToListAsync(ct);
    }
}
