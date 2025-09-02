using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities;

public class Submission : BaseEntity
{
    public int AttemptNumber { get; set; } = 1;
    public string Content { get; set; } = null!;
    public string? FileUrl { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public bool IsLate { get; set; }
    public decimal? Score { get; set; }
    public string? Feedback { get; set; }
    public string Status { get; set; } = "submitted";

    // Relaciones
    public Guid EvaluationId { get; set; }
    public Evaluation Evaluation { get; set; } = null!;

    public Guid StudentId { get; set; }
    public User Student { get; set; } = null!;
}
