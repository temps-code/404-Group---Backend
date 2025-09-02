using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Mantén el DbSet de otras entidades
        public DbSet<Course> Courses { get; set; } = null!;
        public DbSet<CourseModule> CourseModules { get; set; } = null!;
        public DbSet<Resource> Resources { get; set; } = null!;
        public DbSet<Enrollment> Enrollments { get; set; } = null!;
        public DbSet<Evaluation> Evaluations { get; set; } = null!;
        public DbSet<Submission> Submissions { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;
        public DbSet<NotificationRecipient> NotificationRecipients { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // USER
            builder.Entity<User>(b =>
            {
                b.HasKey(u => u.Id);
                b.HasIndex(u => u.Email).IsUnique();
                b.Property(u => u.FirstName).IsRequired();
                b.Property(u => u.LastName).IsRequired();
                b.Property(u => u.Email).IsRequired();
            });

            // COURSE
            builder.Entity<Course>(b =>
            {
                b.HasKey(c => c.Id);
                b.Property(c => c.Title).IsRequired();
                b.Property(c => c.Code).IsRequired();
                b.HasIndex(c => c.Code).IsUnique(false); // opcional: unique si lo deseas
                b.HasOne(c => c.Instructor)
                 .WithMany(u => u.CoursesAsInstructor)
                 .HasForeignKey(c => c.InstructorId)
                 .OnDelete(DeleteBehavior.NoAction);
            });

            // COURSE MODULE (CourseModules)
            builder.Entity<CourseModule>(b =>
            {
                b.HasKey(m => m.Id);
                b.Property(m => m.Title).IsRequired();
                b.Property(m => m.OrderIndex).IsRequired();
                b.Property(m => m.Description).IsRequired(false);

                b.HasOne(m => m.Course)
                 .WithMany(c => c.Modules)         // sigue siendo .Modules en la entidad Course
                 .HasForeignKey(m => m.CourseId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasIndex(m => new { m.CourseId, m.OrderIndex });
            });

            // RESOURCE
            builder.Entity<Resource>(b =>
            {
                b.HasKey(r => r.Id);
                b.Property(r => r.Name).IsRequired();
                b.Property(r => r.Url).IsRequired();
                b.Property(r => r.Type).IsRequired();

                b.HasOne(r => r.UploadedBy)
                 .WithMany(u => u.ResourcesUploaded)
                 .HasForeignKey(r => r.UploadedById)
                 .OnDelete(DeleteBehavior.NoAction);

                b.HasOne(r => r.Course)
                 .WithMany(c => c.Resources)
                 .HasForeignKey(r => r.CourseId)
                 .OnDelete(DeleteBehavior.NoAction);

                b.HasOne(r => r.Module)
                 .WithMany(m => m.Resources)
                 .HasForeignKey(r => r.ModuleId)
                 .OnDelete(DeleteBehavior.NoAction);
            });

            // ENROLLMENT
            builder.Entity<Enrollment>(b =>
            {
                b.HasKey(e => e.Id);
                b.HasOne(e => e.Course).WithMany(c => c.Enrollments).HasForeignKey(e => e.CourseId).OnDelete(DeleteBehavior.NoAction);
                b.HasOne(e => e.User).WithMany(u => u.Enrollments).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.NoAction);
                // si quieres unicidad activa por Course+User gestionarla desde lógica o índice según reglas
            });

            // EVALUATION
            builder.Entity<Evaluation>(b =>
            {
                b.HasKey(ev => ev.Id);
                b.Property(ev => ev.Title).IsRequired();
                b.HasOne(ev => ev.Course).WithMany(c => c.Evaluations).HasForeignKey(ev => ev.CourseId).OnDelete(DeleteBehavior.NoAction);
            });

            // SUBMISSION
            builder.Entity<Submission>(b =>
            {
                b.HasKey(s => s.Id);
                b.Property(s => s.Content).IsRequired();
                b.HasOne(s => s.Evaluation).WithMany(ev => ev.Submissions).HasForeignKey(s => s.EvaluationId).OnDelete(DeleteBehavior.Cascade);
                b.HasOne(s => s.Student).WithMany(u => u.Submissions).HasForeignKey(s => s.StudentId).OnDelete(DeleteBehavior.NoAction);
            });

            // NOTIFICATION
            builder.Entity<Notification>(b =>
            {
                b.HasKey(n => n.Id);
                b.Property(n => n.Title).IsRequired();
                b.Property(n => n.Message).IsRequired();
                b.Property(n => n.Channel).IsRequired();
                b.HasOne(n => n.Sender).WithMany().HasForeignKey(n => n.SenderId).OnDelete(DeleteBehavior.NoAction);
                b.HasOne(n => n.Course).WithMany().HasForeignKey(n => n.CourseId).OnDelete(DeleteBehavior.NoAction);
            });

            // NOTIFICATION RECIPIENT
            builder.Entity<NotificationRecipient>(b =>
            {
                b.HasKey(r => r.Id);
                b.HasOne(r => r.Notification).WithMany(n => n.Recipients).HasForeignKey(r => r.NotificationId).OnDelete(DeleteBehavior.Cascade);
                b.HasOne(r => r.User).WithMany().HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.NoAction);
                b.HasIndex(r => new { r.UserId, r.IsRead });
            });
        }
    }
}
