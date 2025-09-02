using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface INotificationRepository : IRepository<Notification>
    {
        // Recipients: manejo separado (añadir/actualizar marcas, etc.)
        Task AddRecipientAsync(NotificationRecipient recipient, CancellationToken ct = default);
        void UpdateRecipient(NotificationRecipient recipient);

        Task<IEnumerable<NotificationRecipient>> GetPendingRecipientsAsync(int maxItems = 100, CancellationToken ct = default);
        Task<IEnumerable<NotificationRecipient>> GetRecipientsByUserAsync(Guid userId, bool onlyUnread = false, CancellationToken ct = default);

        // Métricas rápidas (opcional)
        Task<int> CountUnreadForUserAsync(Guid userId, CancellationToken ct = default);

        Task<NotificationRecipient?> GetRecipientByIdAsync(Guid id, CancellationToken ct = default);
    }
}
