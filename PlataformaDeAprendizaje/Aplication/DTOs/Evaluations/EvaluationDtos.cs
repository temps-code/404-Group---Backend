namespace Application.DTOs.Evaluations
{
    using System;

    public class EvaluationDto
    {
        public Guid Id { get; set; }
        public Guid CourseId { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal TotalPoints { get; set; }
        public DateTime? OpenAt { get; set; }
        public DateTime? CloseAt { get; set; }
        public int? TimeLimitMinutes { get; set; }
        public bool IsPublished { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateEvaluationDto
    {
        public Guid CourseId { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal TotalPoints { get; set; }
        public DateTime? OpenAt { get; set; }
        public DateTime? CloseAt { get; set; }
        public int? TimeLimitMinutes { get; set; }
    }

    public class UpdateEvaluationDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public decimal? TotalPoints { get; set; }
        public DateTime? OpenAt { get; set; }
        public DateTime? CloseAt { get; set; }
        public int? TimeLimitMinutes { get; set; }
        public bool? IsPublished { get; set; }
        public bool? IsActive { get; set; }
    }
    public class PublishEvaluationDto
    {
        public DateTime? OpenAt { get; set; }
        public DateTime? CloseAt { get; set; }
    }
}
