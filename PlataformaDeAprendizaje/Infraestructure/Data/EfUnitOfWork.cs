using System.Threading;
using System.Threading.Tasks;
using Domain.Interfaces;
using Infrastructure.Data;

namespace Infrastructure.Data
{
    public class EfUnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _ctx;

        public EfUnitOfWork(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            return _ctx.SaveChangesAsync(ct);
        }
    }
}
