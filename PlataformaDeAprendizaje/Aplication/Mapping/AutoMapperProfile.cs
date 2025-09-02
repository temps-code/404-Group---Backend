using AutoMapper;
using Domain.Entities;
using Application.DTOs.Users;
using Application.DTOs.Courses;
using Application.DTOs.CourseModules;
using Application.DTOs.Resources;
using Application.DTOs.Enrollments;
using Application.DTOs.Evaluations;
using Application.DTOs.Submissions;
using Application.DTOs.Notifications;

namespace Application.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // USERS
            CreateMap<User, UserDto>();
            CreateMap<CreateUserDto, User>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.PasswordHash, opt => opt.Ignore())
                .ForMember(d => d.CreatedAt, opt => opt.Ignore());
            CreateMap<UpdateUserDto, User>().ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // COURSES
            CreateMap<Course, CourseDto>()
                .ForMember(d => d.InstructorName,
                           opt => opt.MapFrom(s => s.Instructor != null ? (s.Instructor.FirstName + " " + s.Instructor.LastName) : null));
            CreateMap<CreateCourseDto, Course>().ForMember(d => d.Id, opt => opt.Ignore()).ForMember(d => d.CreatedAt, opt => opt.Ignore());
            CreateMap<UpdateCourseDto, Course>().ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // COURSE MODULES
            CreateMap<CourseModule, CourseModuleDto>();
            CreateMap<CreateCourseModuleDto, CourseModule>().ForMember(d => d.Id, opt => opt.Ignore()).ForMember(d => d.CreatedAt, opt => opt.Ignore());
            CreateMap<UpdateCourseModuleDto, CourseModule>().ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // RESOURCES
            CreateMap<Resource, ResourceDto>();
            CreateMap<CreateResourceDto, Resource>().ForMember(d => d.Id, opt => opt.Ignore()).ForMember(d => d.CreatedAt, opt => opt.Ignore());
            CreateMap<UpdateResourceDto, Resource>().ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // ENROLLMENTS
            CreateMap<Enrollment, EnrollmentDto>();
            CreateMap<CreateEnrollmentDto, Enrollment>().ForMember(d => d.Id, opt => opt.Ignore()).ForMember(d => d.CreatedAt, opt => opt.Ignore());

            // EVALUATIONS
            CreateMap<Evaluation, EvaluationDto>();
            CreateMap<CreateEvaluationDto, Evaluation>().ForMember(d => d.Id, opt => opt.Ignore()).ForMember(d => d.CreatedAt, opt => opt.Ignore());
            CreateMap<UpdateEvaluationDto, Evaluation>().ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // SUBMISSIONS
            CreateMap<Submission, Application.DTOs.Submissions.SubmissionDto>().ReverseMap();
            CreateMap<Application.DTOs.Submissions.CreateSubmissionDto, Submission>().ForMember(d => d.Id, opt => opt.Ignore()).ForMember(d => d.CreatedAt, opt => opt.Ignore()).ForMember(d => d.SubmittedAt, opt => opt.Ignore());
            CreateMap<Application.DTOs.Submissions.UpdateSubmissionDto, Submission>().ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            
            // NOTIFICATIONS
            CreateMap<Notification, Application.DTOs.Notifications.NotificationDto>().ReverseMap();
            CreateMap<Application.DTOs.Notifications.CreateNotificationDto, Notification>().ForMember(d => d.Id, opt => opt.Ignore()).ForMember(d => d.CreatedAt, opt => opt.Ignore()).ForMember(d => d.SentAt, opt => opt.Ignore());
            CreateMap<NotificationRecipient, Application.DTOs.Notifications.NotificationRecipientDto>().ReverseMap();
        }
    }
}
