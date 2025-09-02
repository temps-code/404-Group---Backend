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
    public class SqlSubmissionRepository : BaseRepository<Submission>, ISubmissionRepository
    {
        public SqlSubmissionRepository(AppDbContext ctx) : base(ctx) { }

        public async Task<IEnumerable<Submission>> GetByEvaluationAsync(Guid evaluationId, CancellationToken ct = default)
            => await _ctx.Submissions.Where(s => s.EvaluationId == evaluationId && s.IsActive).ToListAsync(ct);

        public async Task<IEnumerable<Submission>> GetByStudentAsync(Guid studentId, CancellationToken ct = default)
            => await _ctx.Submissions.Where(s => s.StudentId == studentId && s.IsActive).ToListAsync(ct);

        public async Task<Submission?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
            => await _ctx.Submissions
                         .Include(s => s.Evaluation)
                         .Include(s => s.Student)
                         .FirstOrDefaultAsync(s => s.Id == id && s.IsActive, ct);
    }
}
