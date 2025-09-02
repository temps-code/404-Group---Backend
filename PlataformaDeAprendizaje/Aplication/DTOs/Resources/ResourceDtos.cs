namespace Application.DTOs.Resources
{
    using System;

    public class ResourceDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Url { get; set; } = null!;
        public DateTime UploadedAt { get; set; }
        public Guid UploadedById { get; set; }
        public Guid? CourseId { get; set; }
        public Guid? ModuleId { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateResourceDto
    {
        public string Type { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Url { get; set; } = null!;
        public Guid UploadedById { get; set; }
        public Guid? CourseId { get; set; }
        public Guid? ModuleId { get; set; }
    }

    public class UpdateResourceDto
    {
        public string? Name { get; set; }
        public string? Url { get; set; }
        public bool? IsActive { get; set; }
    }
}
