namespace Application.DTOs.Submissions
{
    using System;

    public class SubmissionDto
    {
        public Guid Id { get; set; }
        public Guid EvaluationId { get; set; }
        public Guid StudentId { get; set; }
        public int AttemptNumber { get; set; }
        public string Content { get; set; } = null!;
        public string? FileUrl { get; set; }
        public DateTime SubmittedAt { get; set; }
        public bool IsLate { get; set; }
        public decimal? Score { get; set; }
        public string? Feedback { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateSubmissionDto
    {
        public Guid EvaluationId { get; set; }
        public Guid StudentId { get; set; }
        public int AttemptNumber { get; set; } = 1;
        public string Content { get; set; } = null!;
        public string? FileUrl { get; set; }
    }
    public class GradeSubmissionDto
    {
        public decimal? Score { get; set; }
        public string? Feedback { get; set; }
        public string? Status { get; set; } // p.ej. "graded", "needs_review"
    }

    public class UpdateSubmissionDto
    {
        public string? Content { get; set; }
        public string? FileUrl { get; set; }
    }
}
