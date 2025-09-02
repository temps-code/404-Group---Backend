namespace Application.DTOs.Notifications
{
    using System;
    using System.Collections.Generic;

    public class NotificationDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string? Payload { get; set; }
        public string Channel { get; set; } = "ui";
        public Guid? SenderId { get; set; }
        public Guid? CourseId { get; set; }
        public string? ContextType { get; set; }
        public string? ContextId { get; set; }
        public DateTime? SentAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateNotificationDto
    {
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string? Payload { get; set; }
        public string Channel { get; set; } = "ui";
        public Guid? SenderId { get; set; }
        public Guid? CourseId { get; set; }
        public string? ContextType { get; set; }
        public string? ContextId { get; set; }
        public List<Guid>? RecipientIds { get; set; } = new();
        public bool TargetAll { get; set; } = false;
    }

    public class NotificationRecipientDto
    {
        public Guid Id { get; set; }
        public Guid NotificationId { get; set; }
        public Guid UserId { get; set; }
        public bool IsDelivered { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public bool IsActive { get; set; }
    }
}
