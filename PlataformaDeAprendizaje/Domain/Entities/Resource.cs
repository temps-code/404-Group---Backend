using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities;

public class Resource : BaseEntity
{
    public string Type { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Url { get; set; } = null!;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // Relaciones
    public Guid UploadedById { get; set; }
    public User UploadedBy { get; set; } = null!;

    public Guid? CourseId { get; set; }
    public Course? Course { get; set; }

    public Guid? ModuleId { get; set; }
    public CourseModule? Module { get; set; }
}

