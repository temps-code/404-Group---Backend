using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class NotificationRecipient : BaseEntity
    {
        public Guid NotificationId { get; set; }
        public Notification Notification { get; set; } = null!;

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public bool IsDelivered { get; set; } = false;
        public DateTime? DeliveredAt { get; set; }

        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; }
    }
}