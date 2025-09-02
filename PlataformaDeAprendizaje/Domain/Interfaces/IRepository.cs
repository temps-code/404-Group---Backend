using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    /// <summary>
    /// Repositorio genérico para entidades del dominio que implementen IBaseEntity.
    /// </summary>
    public interface IRepository<T> where T : class, IBaseEntity
    {
        // Obtener por id (solo activo)
        Task<T?> GetByIdActiveAsync(Guid id, CancellationToken ct = default);

        // Obtener por id incluyendo inactivos (para reactivate/auditoría)
        Task<T?> GetByIdIncludeInactiveAsync(Guid id, CancellationToken ct = default);

        // Listados
        Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default);          // todos: activos + inactivos
        Task<IEnumerable<T>> GetAllActiveAsync(CancellationToken ct = default);    // solo activos
        Task<IEnumerable<T>> GetAllInactiveAsync(CancellationToken ct = default);  // solo inactivos

        // Mutaciones (no persisten: el UseCase debe llamar IUnitOfWork.SaveChangesAsync())
        Task AddAsync(T entity, CancellationToken ct = default);
        void Update(T entity);

        // Soft-delete / Reactivate (marcan IsActive, no guardan)
        Task<bool> DeactivateAsync(Guid id, CancellationToken ct = default);
        Task<bool> ReactivateAsync(Guid id, CancellationToken ct = default);
    }
}

