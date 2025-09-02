using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities;

public class Course : BaseEntity
{
    public string Title { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool IsPublished { get; set; } = false;

    // Relaciones
    public Guid InstructorId { get; set; }
    public User Instructor { get; set; } = null!;
    public ICollection<CourseModule> Modules { get; set; } = new List<CourseModule>();
    public ICollection<Resource> Resources { get; set; } = new List<Resource>();
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<Evaluation> Evaluations { get; set; } = new List<Evaluation>();
}