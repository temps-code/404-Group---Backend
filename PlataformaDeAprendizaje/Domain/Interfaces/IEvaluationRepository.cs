using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IEvaluationRepository : IRepository<Evaluation>
    {
        Task<IEnumerable<Evaluation>> GetByCourseAsync(Guid courseId, CancellationToken ct = default);
        Task<IEnumerable<Evaluation>> GetOpenEvaluationsAsync(CancellationToken ct = default);
    }
}
