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
    public class SqlEvaluationRepository : BaseRepository<Evaluation>, IEvaluationRepository
    {
        public SqlEvaluationRepository(AppDbContext ctx) : base(ctx) { }

        public async Task<IEnumerable<Evaluation>> GetByCourseAsync(Guid courseId, CancellationToken ct = default)
            => await _ctx.Evaluations.Where(ev => ev.CourseId == courseId && ev.IsActive).ToListAsync(ct);

        public async Task<IEnumerable<Evaluation>> GetOpenEvaluationsAsync(CancellationToken ct = default)
            => await _ctx.Evaluations.Where(ev => ev.IsActive && (ev.OpenAt == null || ev.OpenAt <= DateTime.UtcNow) && (ev.CloseAt == null || ev.CloseAt >= DateTime.UtcNow)).ToListAsync(ct);
    }
}
