namespace Application.DTOs.Enrollments
{
    using System;

    // DTO para creación de inscripción
    public class CreateEnrollmentDto
    {
        public Guid CourseId { get; set; }
        public Guid UserId { get; set; }
    }

    // DTO para lectura/transferencia de una inscripción
    public class EnrollmentDto
    {
        public Guid Id { get; set; }
        public Guid CourseId { get; set; }
        public Guid UserId { get; set; }
        public string Status { get; set; } = null!;
        public DateTime EnrolledAt { get; set; }
        public DateTime? UnenrolledAt { get; set; }
    }
}
