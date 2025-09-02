using Microsoft.AspNetCore.Identity;
using Domain.Interfaces;

namespace Domain.Entities
{
    public class User : IdentityUser<Guid>, IBaseEntity
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Role { get; set; } = "Student";

        // implementación de IBaseEntity
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Relaciones (mantén si las necesitas)
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public ICollection<Resource> ResourcesUploaded { get; set; } = new List<Resource>();
        public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
        public ICollection<Course> CoursesAsInstructor { get; set; } = new List<Course>();
    }
}