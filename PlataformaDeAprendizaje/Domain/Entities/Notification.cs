using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Notification : BaseEntity
    {
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string? Payload { get; set; }
        public string Channel { get; set; } = "ui";
        public Guid? SenderId { get; set; }
        public Guid? CourseId { get; set; }
        public string? ContextType { get; set; }
        public string? ContextId { get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime? ExpiresAt { get; set; }

        // Navigation
        public User? Sender { get; set; }
        public Course? Course { get; set; }
        public ICollection<NotificationRecipient> Recipients { get; set; } = new List<NotificationRecipient>();
    }
}