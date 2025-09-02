namespace Application.DTOs.Courses
{
    using System;
    using System.Collections.Generic;
    using Application.DTOs.CourseModules;
    using Application.DTOs.Resources;

    public class CourseDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string Description { get; set; } = null!;
        public bool IsPublished { get; set; }
        public Guid InstructorId { get; set; }
        public string? InstructorName { get; set; }
        public List<CourseModuleDto> Modules { get; set; } = new();
        public List<ResourceDto> Resources { get; set; } = new();
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateCourseDto
    {
        public string Title { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string Description { get; set; } = null!;
        public Guid InstructorId { get; set; }
    }

    public class UpdateCourseDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public bool? IsPublished { get; set; }
        public bool? IsActive { get; set; }
    }
}
