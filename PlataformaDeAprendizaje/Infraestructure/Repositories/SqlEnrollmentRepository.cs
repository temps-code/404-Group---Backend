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
    public class SqlEnrollmentRepository : BaseRepository<Enrollment>, IEnrollmentRepository
    {
        public SqlEnrollmentRepository(AppDbContext ctx) : base(ctx) { }

        public async Task<Enrollment?> GetAsync(Guid courseId, Guid userId, CancellationToken ct = default)
            => await _ctx.Enrollments.FirstOrDefaultAsync(e => e.CourseId == courseId && e.UserId == userId && e.IsActive, ct);

        public async Task<IEnumerable<Enrollment>> GetByCourseAsync(Guid courseId, CancellationToken ct = default)
            => await _ctx.Enrollments.Where(e => e.CourseId == courseId && e.IsActive).ToListAsync(ct);

        public async Task<IEnumerable<Enrollment>> GetByUserAsync(Guid userId, CancellationToken ct = default)
            => await _ctx.Enrollments.Where(e => e.UserId == userId && e.IsActive).ToListAsync(ct);
    }
}
