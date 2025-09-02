using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface ICourseModuleRepository : IRepository<CourseModule>
    {
        Task<IEnumerable<CourseModule>> GetByCourseAsync(Guid courseId, CancellationToken ct = default);
        Task<IEnumerable<CourseModule>> GetByCourseOrderedAsync(Guid courseId, CancellationToken ct = default);
    }
}
