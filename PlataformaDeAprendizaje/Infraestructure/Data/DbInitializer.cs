// Infrastructure/Data/DbInitializer.cs
using System;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var provider = scope.ServiceProvider;

            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("DbInitializer");

            var ctx = provider.GetRequiredService<AppDbContext>();
            var userManager = provider.GetRequiredService<UserManager<User>>();
            var roleManager = provider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

            logger.LogInformation("DbInitializer: applying migrations...");
            await ctx.Database.MigrateAsync();

            // --- ROLES ---
            logger.LogInformation("DbInitializer: ensuring roles...");
            string[] roles = new[] { "Student", "Teacher", "Admin" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var r = await roleManager.CreateAsync(new IdentityRole<Guid>(role));
                    if (!r.Succeeded)
                        logger.LogWarning("Failed creating role {role}: {errors}", role, string.Join(", ", r.Errors.Select(e => e.Description)));
                    else
                        logger.LogInformation("Created role {role}", role);
                }
            }

            // --- USERS: 2 instructors, 2 students, 1 admin ---
            logger.LogInformation("DbInitializer: ensuring users...");

            var instructor1Email = "instructor1@demo.com";
            var instructor2Email = "instructor2@demo.com";
            var student1Email = "student1@demo.com";
            var student2Email = "student2@demo.com";
            var adminEmail = "admin@demo.com";

            var instructor1 = await EnsureUserAsync(userManager, logger, instructor1Email, "Instructor", "Uno", "Teacher");
            var instructor2 = await EnsureUserAsync(userManager, logger, instructor2Email, "Instructor", "Dos", "Teacher");
            var student1 = await EnsureUserAsync(userManager, logger, student1Email, "Alumno", "Uno", "Student");
            var student2 = await EnsureUserAsync(userManager, logger, student2Email, "Alumno", "Dos", "Student");
            var admin = await EnsureUserAsync(userManager, logger, adminEmail, "Admin", "Demo", "Admin");

            // Refresh context-tracked objects if needed
            // --- COURSES (2) ---
            logger.LogInformation("DbInitializer: ensuring courses...");
            var course1Code = "CUR-001";
            var course2Code = "CUR-002";

            var course1 = await ctx.Courses.FirstOrDefaultAsync(c => c.Code == course1Code);
            if (course1 == null)
            {
                course1 = new Course
                {
                    Id = Guid.NewGuid(),
                    Title = "Curso Demo - Fundamentos",
                    Code = course1Code,
                    Description = "Curso de fundamentos con contenido práctico.",
                    InstructorId = instructor1.Id,
                    IsPublished = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                await ctx.Courses.AddAsync(course1);
                logger.LogInformation("Created course {code}", course1Code);
            }
            else logger.LogInformation("Course exists {code}", course1Code);

            var course2 = await ctx.Courses.FirstOrDefaultAsync(c => c.Code == course2Code);
            if (course2 == null)
            {
                course2 = new Course
                {
                    Id = Guid.NewGuid(),
                    Title = "Curso Demo - Avanzado",
                    Code = course2Code,
                    Description = "Curso avanzado con ejercicios y evaluaciones.",
                    InstructorId = instructor2.Id,
                    IsPublished = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                await ctx.Courses.AddAsync(course2);
                logger.LogInformation("Created course {code}", course2Code);
            }
            else logger.LogInformation("Course exists {code}", course2Code);

            await ctx.SaveChangesAsync();

            // --- COURSE MODULES: 2 per course ---
            logger.LogInformation("DbInitializer: ensuring modules...");
            await EnsureModulesForCourse(ctx, course1, logger);
            await EnsureModulesForCourse(ctx, course2, logger);

            await ctx.SaveChangesAsync();

            // reload modules lists
            var course1Modules = await ctx.CourseModules.Where(m => m.CourseId == course1.Id).OrderBy(m => m.OrderIndex).ToListAsync();
            var course2Modules = await ctx.CourseModules.Where(m => m.CourseId == course2.Id).OrderBy(m => m.OrderIndex).ToListAsync();

            // --- RESOURCES: course-level + module-level (at least 2 per entity overall) ---
            logger.LogInformation("DbInitializer: ensuring resources...");
            // Course level resources
            await EnsureCourseResource(ctx, course1, instructor1, "Syllabus - Fundamentos", "https://example.com/syllabus-fundamentos.pdf", logger);
            await EnsureCourseResource(ctx, course2, instructor2, "Syllabus - Avanzado", "https://example.com/syllabus-avanzado.pdf", logger);

            // Module-level resources (one per module)
            foreach (var mod in course1Modules)
                await EnsureModuleResource(ctx, course1, mod, instructor1, $"Material - {mod.Title}", $"https://example.com/resources/{mod.Id}.zip", logger);

            foreach (var mod in course2Modules)
                await EnsureModuleResource(ctx, course2, mod, instructor2, $"Material - {mod.Title}", $"https://example.com/resources/{mod.Id}.zip", logger);

            await ctx.SaveChangesAsync();

            // --- EVALUATIONS: 2 per course (1 published, 1 draft) ---
            logger.LogInformation("DbInitializer: ensuring evaluations...");
            await EnsureEvaluationsForCourse(ctx, course1, logger);
            await EnsureEvaluationsForCourse(ctx, course2, logger);

            await ctx.SaveChangesAsync();

            // reload evaluations (published) for submissions
            var course1PublishedEval = await ctx.Evaluations.FirstOrDefaultAsync(ev => ev.CourseId == course1.Id && ev.IsPublished);
            var course2PublishedEval = await ctx.Evaluations.FirstOrDefaultAsync(ev => ev.CourseId == course2.Id && ev.IsPublished);

            // --- ENROLLMENTS: enroll both students in both courses (2 enrollments per course) ---
            logger.LogInformation("DbInitializer: ensuring enrollments...");
            await EnsureEnrollment(ctx, course1, student1, logger);
            await EnsureEnrollment(ctx, course1, student2, logger);
            await EnsureEnrollment(ctx, course2, student1, logger);
            await EnsureEnrollment(ctx, course2, student2, logger);

            await ctx.SaveChangesAsync();

            // --- SUBMISSIONS: create sample submissions for each published evaluation (one per student) ---
            logger.LogInformation("DbInitializer: ensuring sample submissions...");
            if (course1PublishedEval != null)
            {
                await EnsureSubmission(ctx, course1PublishedEval, student1, "Respuesta ejemplo estudiante1 - curso1", logger);
                await EnsureSubmission(ctx, course1PublishedEval, student2, "Respuesta ejemplo estudiante2 - curso1", logger);
            }

            if (course2PublishedEval != null)
            {
                await EnsureSubmission(ctx, course2PublishedEval, student1, "Respuesta ejemplo estudiante1 - curso2", logger);
                await EnsureSubmission(ctx, course2PublishedEval, student2, "Respuesta ejemplo estudiante2 - curso2", logger);
            }

            await ctx.SaveChangesAsync();

            // --- NOTIFICATIONS: create notifications for enrollments and for published evaluations ---
            logger.LogInformation("DbInitializer: ensuring notifications and recipients...");

            // Enrollment notifications (one per enrollment)
            var enrollments = await ctx.Enrollments.Where(e => e.IsActive).ToListAsync();
            foreach (var en in enrollments)
            {
                var nkey = $"enroll-{en.CourseId}-{en.UserId}";
                if (!await ctx.Notifications.AnyAsync(n => n.ContextType == "enrollment" && n.ContextId == nkey))
                {
                    var notif = new Notification
                    {
                        Id = Guid.NewGuid(),
                        Title = $"Inscripción al curso",
                        Message = $"Has sido inscrito al curso (Id: {en.CourseId})",
                        Channel = "ui",
                        SenderId = en.Course.InstructorId == Guid.Empty ? instructor1.Id : en.Course.InstructorId,
                        CourseId = en.CourseId,
                        ContextType = "enrollment",
                        ContextId = nkey,
                        SentAt = DateTime.UtcNow,
                        ExpiresAt = DateTime.UtcNow.AddDays(30),
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    await ctx.Notifications.AddAsync(notif);
                    await ctx.SaveChangesAsync();

                    var recipient = new NotificationRecipient
                    {
                        Id = Guid.NewGuid(),
                        NotificationId = notif.Id,
                        UserId = en.UserId,
                        IsDelivered = false,
                        DeliveredAt = null,
                        IsRead = false,
                        ReadAt = null,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    await ctx.NotificationRecipients.AddAsync(recipient);
                    await ctx.SaveChangesAsync();

                    logger.LogInformation("Created enrollment notification for enrollment {enId}", en.Id);
                }
            }

            // Notifications for published evaluations
            var publishedEvals = await ctx.Evaluations.Where(ev => ev.IsActive && ev.IsPublished).ToListAsync();
            foreach (var ev in publishedEvals)
            {
                var nkey = $"eval-published-{ev.Id}";
                if (!await ctx.Notifications.AnyAsync(n => n.ContextType == "evaluation_published" && n.ContextId == nkey))
                {
                    var notif = new Notification
                    {
                        Id = Guid.NewGuid(),
                        Title = $"Evaluación publicada: {ev.Title}",
                        Message = $"La evaluación '{ev.Title}' está disponible.",
                        Channel = "ui",
                        SenderId = ev.Course != null ? ev.Course.InstructorId : instructor1.Id,
                        CourseId = ev.CourseId,
                        ContextType = "evaluation_published",
                        ContextId = nkey,
                        SentAt = DateTime.UtcNow,
                        ExpiresAt = DateTime.UtcNow.AddDays(14),
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    await ctx.Notifications.AddAsync(notif);
                    await ctx.SaveChangesAsync();

                    // recipients: all students enrolled in the course
                    var recipients = await ctx.Enrollments.Where(en => en.CourseId == ev.CourseId && en.IsActive).Select(en => en.UserId).Distinct().ToListAsync();
                    foreach (var uid in recipients)
                    {
                        // avoid duplicate recipient
                        if (await ctx.NotificationRecipients.AnyAsync(r => r.NotificationId == notif.Id && r.UserId == uid)) continue;
                        var recipient = new NotificationRecipient
                        {
                            Id = Guid.NewGuid(),
                            NotificationId = notif.Id,
                            UserId = uid,
                            IsDelivered = false,
                            DeliveredAt = null,
                            IsRead = false,
                            ReadAt = null,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };
                        await ctx.NotificationRecipients.AddAsync(recipient);
                    }
                    await ctx.SaveChangesAsync();

                    logger.LogInformation("Created published-eval notification for eval {evalId}", ev.Id);
                }
            }

            logger.LogInformation("DbInitializer: finished.");
        }

        #region Helpers

        private static async Task<User> EnsureUserAsync(UserManager<User> userManager, ILogger logger, string email, string firstName, string lastName, string role)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new User
                {
                    Id = Guid.NewGuid(),
                    UserName = email,
                    NormalizedUserName = email.ToUpperInvariant(),
                    Email = email,
                    NormalizedEmail = email.ToUpperInvariant(),
                    FirstName = firstName,
                    LastName = lastName,
                    Role = role,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var res = await userManager.CreateAsync(user, "DemoPass123!");
                if (res.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                    logger.LogInformation("Created user {email} as {role}", email, role);
                }
                else
                {
                    logger.LogWarning("Failed creating user {email}: {errors}", email, string.Join(", ", res.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                logger.LogInformation("User exists {email}", email);
            }

            // re-fetch to ensure Id is populated as EF Identity may track different instance
            var u = await userManager.FindByEmailAsync(email);
            return u!;
        }

        private static async Task EnsureModulesForCourse(AppDbContext ctx, Course course, ILogger logger)
        {
            var exists = await ctx.CourseModules.AnyAsync(m => m.CourseId == course.Id);
            if (exists)
            {
                logger.LogInformation("Modules already exist for course {courseId}", course.Id);
                return;
            }

            var m1 = new CourseModule
            {
                Id = Guid.NewGuid(),
                CourseId = course.Id,
                Title = "Módulo 1 - Introducción",
                OrderIndex = 1,
                Description = "Contenido introductorio",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            var m2 = new CourseModule
            {
                Id = Guid.NewGuid(),
                CourseId = course.Id,
                Title = "Módulo 2 - Prácticas",
                OrderIndex = 2,
                Description = "Ejercicios y prácticas",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await ctx.CourseModules.AddRangeAsync(m1, m2);
            logger.LogInformation("Created modules for course {courseId}", course.Id);
        }

        private static async Task EnsureCourseResource(AppDbContext ctx, Course course, User uploader, string name, string url, ILogger logger)
        {
            var exists = await ctx.Resources.AnyAsync(r => r.CourseId == course.Id && r.Name == name);
            if (exists)
            {
                logger.LogInformation("Course resource already exists {name} for course {courseId}", name, course.Id);
                return;
            }

            var res = new Resource
            {
                Id = Guid.NewGuid(),
                CourseId = course.Id,
                ModuleId = null,
                Type = "document",
                Name = name,
                Url = url,
                UploadedById = uploader.Id,
                UploadedAt = DateTime.UtcNow,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await ctx.Resources.AddAsync(res);
            logger.LogInformation("Added course-level resource {name} for course {courseId}", name, course.Id);
        }

        private static async Task EnsureModuleResource(AppDbContext ctx, Course course, CourseModule module, User uploader, string name, string url, ILogger logger)
        {
            var exists = await ctx.Resources.AnyAsync(r => r.ModuleId == module.Id && r.Name == name);
            if (exists)
            {
                logger.LogInformation("Module resource already exists {name} for module {moduleId}", name, module.Id);
                return;
            }

            var res = new Resource
            {
                Id = Guid.NewGuid(),
                CourseId = course.Id,
                ModuleId = module.Id,
                Type = "file",
                Name = name,
                Url = url,
                UploadedById = uploader.Id,
                UploadedAt = DateTime.UtcNow,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await ctx.Resources.AddAsync(res);
            logger.LogInformation("Added module resource {name} for module {moduleId}", name, module.Id);
        }

        private static async Task EnsureEvaluationsForCourse(AppDbContext ctx, Course course, ILogger logger)
        {
            // published
            var publishedExists = await ctx.Evaluations.AnyAsync(ev => ev.CourseId == course.Id && ev.Title == "Examen Parcial 1");
            if (!publishedExists)
            {
                var pub = new Evaluation
                {
                    Id = Guid.NewGuid(),
                    CourseId = course.Id,
                    Title = "Examen Parcial 1",
                    Description = "Examen parcial - público",
                    TotalPoints = 100m,
                    OpenAt = DateTime.UtcNow.AddMinutes(-30),
                    CloseAt = DateTime.UtcNow.AddDays(7),
                    TimeLimitMinutes = 120,
                    IsPublished = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                await ctx.Evaluations.AddAsync(pub);
                logger.LogInformation("Created published evaluation for course {courseId}", course.Id);
            }
            else
            {
                logger.LogInformation("Published evaluation exists for course {courseId}", course.Id);
            }

            // draft
            var draftExists = await ctx.Evaluations.AnyAsync(ev => ev.CourseId == course.Id && ev.Title == "Tarea - Borrador");
            if (!draftExists)
            {
                var draft = new Evaluation
                {
                    Id = Guid.NewGuid(),
                    CourseId = course.Id,
                    Title = "Tarea - Borrador",
                    Description = "Tarea en borrador",
                    TotalPoints = 20m,
                    OpenAt = null,
                    CloseAt = null,
                    TimeLimitMinutes = null,
                    IsPublished = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                await ctx.Evaluations.AddAsync(draft);
                logger.LogInformation("Created draft evaluation for course {courseId}", course.Id);
            }
            else
            {
                logger.LogInformation("Draft evaluation exists for course {courseId}", course.Id);
            }
        }

        private static async Task EnsureEnrollment(AppDbContext ctx, Course course, User student, ILogger logger)
        {
            var exists = await ctx.Enrollments.AnyAsync(en => en.CourseId == course.Id && en.UserId == student.Id && en.IsActive);
            if (exists)
            {
                logger.LogInformation("Enrollment already exists for student {studentId} in course {courseId}", student.Id, course.Id);
                return;
            }

            var enrollment = new Enrollment
            {
                Id = Guid.NewGuid(),
                CourseId = course.Id,
                UserId = student.Id,
                Status = "active",
                EnrolledAt = DateTime.UtcNow,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await ctx.Enrollments.AddAsync(enrollment);
            logger.LogInformation("Enrolled student {studentEmail} into course {courseCode}", student.Email, course.Code);
        }

        private static async Task EnsureSubmission(AppDbContext ctx, Evaluation evaluation, User student, string content, ILogger logger)
        {
            var exists = await ctx.Submissions.AnyAsync(s => s.EvaluationId == evaluation.Id && s.StudentId == student.Id);
            if (exists)
            {
                logger.LogInformation("Submission already exists for student {studentEmail} on eval {evalId}", student.Email, evaluation.Id);
                return;
            }

            var submission = new Submission
            {
                Id = Guid.NewGuid(),
                EvaluationId = evaluation.Id,
                StudentId = student.Id,
                AttemptNumber = 1,
                Content = content,
                FileUrl = null,
                SubmittedAt = DateTime.UtcNow,
                IsLate = evaluation.CloseAt.HasValue && DateTime.UtcNow > evaluation.CloseAt.Value,
                Score = null,
                Feedback = null,
                Status = "submitted",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await ctx.Submissions.AddAsync(submission);
            logger.LogInformation("Created submission for student {studentEmail} on evaluation {evalId}", student.Email, evaluation.Id);
        }

        #endregion
    }
}
