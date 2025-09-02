using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface ISubmissionRepository : IRepository<Submission>
    {
        Task<IEnumerable<Submission>> GetByEvaluationAsync(Guid evaluationId, CancellationToken ct = default);
        Task<IEnumerable<Submission>> GetByStudentAsync(Guid studentId, CancellationToken ct = default);
        Task<Submission?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);
    }
}
