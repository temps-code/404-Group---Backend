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
    public class SqlNotificationRepository : BaseRepository<Notification>, INotificationRepository
    {
        public SqlNotificationRepository(AppDbContext ctx) : base(ctx) { }

        public async Task AddRecipientAsync(NotificationRecipient recipient, CancellationToken ct = default)
        {
            if (recipient == null) throw new ArgumentNullException(nameof(recipient));
            if (recipient.Id == Guid.Empty) recipient.Id = Guid.NewGuid();
            recipient.CreatedAt = DateTime.UtcNow;
            _ctx.NotificationRecipients.Add(recipient);
            await Task.CompletedTask;
        }

        public void UpdateRecipient(NotificationRecipient recipient)
        {
            if (recipient == null) throw new ArgumentNullException(nameof(recipient));
            recipient.UpdatedAt = DateTime.UtcNow;
            _ctx.NotificationRecipients.Update(recipient);
        }

        public async Task<IEnumerable<NotificationRecipient>> GetPendingRecipientsAsync(int maxItems = 100, CancellationToken ct = default)
            => await _ctx.NotificationRecipients
                         .Include(r => r.Notification)
                         .Where(r => r.IsActive && !r.IsDelivered)
                         .OrderBy(r => r.CreatedAt)
                         .Take(maxItems)
                         .ToListAsync(ct);

        public async Task<IEnumerable<NotificationRecipient>> GetRecipientsByUserAsync(Guid userId, bool onlyUnread = false, CancellationToken ct = default)
        {
            var q = _ctx.NotificationRecipients.Include(r => r.Notification).Where(r => r.UserId == userId && r.IsActive);
            if (onlyUnread) q = q.Where(r => !r.IsRead);
            return await q.OrderByDescending(r => r.CreatedAt).ToListAsync(ct);
        }

        public async Task<int> CountUnreadForUserAsync(Guid userId, CancellationToken ct = default)
            => await _ctx.NotificationRecipients.CountAsync(r => r.UserId == userId && r.IsActive && !r.IsRead, ct);

        // If needed, override GetByIdActive/IncludeInactive to include Recipients
        public override async Task<Notification?> GetByIdActiveAsync(Guid id, CancellationToken ct = default)
            => await _ctx.Notifications.Include(n => n.Recipients).FirstOrDefaultAsync(n => n.Id == id && n.IsActive, ct);

        public override async Task<Notification?> GetByIdIncludeInactiveAsync(Guid id, CancellationToken ct = default)
            => await _ctx.Notifications.IgnoreQueryFilters().Include(n => n.Recipients).FirstOrDefaultAsync(n => n.Id == id, ct);

        public async Task<NotificationRecipient?> GetRecipientByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _ctx.NotificationRecipients
                             .Include(r => r.Notification)
                             .Include(r => r.User)
                             .FirstOrDefaultAsync(r => r.Id == id && r.IsActive, ct);
        }
    }
}
