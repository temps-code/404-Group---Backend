using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities;

public class Enrollment : BaseEntity
{
    public string Status { get; set; } = "pending";
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    public DateTime? UnenrolledAt { get; set; }

    // Relaciones
    public Guid CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}
