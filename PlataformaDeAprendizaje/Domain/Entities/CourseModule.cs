using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities;

public class CourseModule : BaseEntity
{
    public string Title { get; set; } = null!;
    public int OrderIndex { get; set; }
    public string Description { get; set; } = null!;

    // Relaciones
    public Guid CourseId { get; set; }
    public Course Course { get; set; } = null!;
    public ICollection<Resource> Resources { get; set; } = new List<Resource>();
}

