using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class BaseRepository<T> : IRepository<T> where T : class, IBaseEntity
    {
        protected readonly AppDbContext _ctx;
        protected readonly DbSet<T> _dbSet;

        public BaseRepository(AppDbContext ctx)
        {
            _ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));
            _dbSet = _ctx.Set<T>();
        }

        public virtual async Task<T?> GetByIdActiveAsync(Guid id, CancellationToken ct = default)
        {
            return await _dbSet.FirstOrDefaultAsync(e => e.Id == id && e.IsActive, ct);
        }

        public virtual async Task<T?> GetByIdIncludeInactiveAsync(Guid id, CancellationToken ct = default)
        {
            return await _dbSet.FirstOrDefaultAsync(e => e.Id == id, ct);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default)
        {
            return await _dbSet.AsNoTracking().ToListAsync(ct);
        }

        public virtual async Task<IEnumerable<T>> GetAllActiveAsync(CancellationToken ct = default)
        {
            return await _dbSet.AsNoTracking().Where(e => e.IsActive).ToListAsync(ct);
        }

        public virtual async Task<IEnumerable<T>> GetAllInactiveAsync(CancellationToken ct = default)
        {
            return await _dbSet.AsNoTracking().Where(e => !e.IsActive).ToListAsync(ct);
        }

        public virtual async Task AddAsync(T entity, CancellationToken ct = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            await _dbSet.AddAsync(entity, ct);
        }

        public virtual void Update(T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            _dbSet.Update(entity);
        }

        /// <summary>
        /// Marca IsActive = false y actualiza el contexto pero NO llama SaveChanges.
        /// El UseCase debe invocar IUnitOfWork.SaveChangesAsync().
        /// </summary>
        public virtual async Task<bool> DeactivateAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _dbSet.FirstOrDefaultAsync(e => e.Id == id, ct);
            if (entity == null) return false;
            if (!entity.IsActive) return false;

            entity.IsActive = false;
            entity.UpdatedAt = DateTime.UtcNow;
            _dbSet.Update(entity);
            return true;
        }

        /// <summary>
        /// Marca IsActive = true y actualiza el contexto pero NO llama SaveChanges.
        /// </summary>
        public virtual async Task<bool> ReactivateAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _dbSet.FirstOrDefaultAsync(e => e.Id == id, ct);
            if (entity == null) return false;
            if (entity.IsActive) return false;

            entity.IsActive = true;
            entity.UpdatedAt = DateTime.UtcNow;
            _dbSet.Update(entity);
            return true;
        }
    }
}
