using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface ICourseRepository : IRepository<Course>
    {
        // Trae el curso con módulos, recursos y evaluaciones
        Task<Course?> GetByIdWithContentAsync(Guid id, CancellationToken ct = default);

        Task<IEnumerable<Course>> GetPublishedAsync(CancellationToken ct = default);
        Task<IEnumerable<Course>> GetByInstructorAsync(Guid instructorId, CancellationToken ct = default);
        Task<IEnumerable<Course>> GetAllWithContentAsync(CancellationToken ct = default);
    }
}
