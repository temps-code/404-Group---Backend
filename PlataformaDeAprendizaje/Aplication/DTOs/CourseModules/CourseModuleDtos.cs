namespace Application.DTOs.CourseModules
{
    using System;

    public class CreateCourseModuleDto
    {
        public Guid CourseId { get; set; }
        public string Title { get; set; } = null!;
        public int OrderIndex { get; set; }
        public string? Description { get; set; }
    }

    public class UpdateCourseModuleDto
    {
        public string? Title { get; set; }
        public int? OrderIndex { get; set; }
        public string? Description { get; set; }
    }

    public class CourseModuleDto
    {
        public Guid Id { get; set; }
        public Guid CourseId { get; set; }
        public string Title { get; set; } = null!;
        public int OrderIndex { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
