using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IEnrollmentRepository : IRepository<Enrollment>
    {
        Task<Enrollment?> GetAsync(Guid courseId, Guid userId, CancellationToken ct = default);
        Task<IEnumerable<Enrollment>> GetByCourseAsync(Guid courseId, CancellationToken ct = default);
        Task<IEnumerable<Enrollment>> GetByUserAsync(Guid userId, CancellationToken ct = default);
    }
}
