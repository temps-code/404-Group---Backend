using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities;

public class Evaluation : BaseEntity
{
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal TotalPoints { get; set; }
    public DateTime? OpenAt { get; set; }
    public DateTime? CloseAt { get; set; }
    public int? TimeLimitMinutes { get; set; }
    public bool IsPublished { get; set; } = false;

    // Relaciones
    public Guid CourseId { get; set; }
    public Course Course { get; set; } = null!;
    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}

