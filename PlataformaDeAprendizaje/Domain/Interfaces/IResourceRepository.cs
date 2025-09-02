using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IResourceRepository : IRepository<Resource>
    {
        Task<IEnumerable<Resource>> GetByCourseAsync(Guid courseId, CancellationToken ct = default);
        Task<IEnumerable<Resource>> GetByModuleAsync(Guid moduleId, CancellationToken ct = default);
        Task<IEnumerable<Resource>> GetByUploaderAsync(Guid uploaderUserId, CancellationToken ct = default);
    }
}
