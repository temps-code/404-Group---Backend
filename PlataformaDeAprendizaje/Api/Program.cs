using Application.Mapping;
using Application.UseCases;
using Application.UseCases.CourseModules;
using Application.UseCases.Enrollments;
using Application.UseCases.Evaluations;
using Application.UseCases.Notifications;
using Application.UseCases.Resources;
using Application.UseCases.Submissions;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- CORS: política permisiva para desarrollo ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()   // permite cualquier origen (dev only)
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString)
);

// Identity
builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
{
    // Ajustes de contraseña para el MVP (ajusta según necesites)
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// AutoMapper
builder.Services.AddAutoMapper(typeof(AutoMapperProfile).Assembly);

// Unit of Work
builder.Services.AddScoped<IUnitOfWork, EfUnitOfWork>();

// Repositories
builder.Services.AddScoped<IUserRepository, SqlUserRepository>();
builder.Services.AddScoped<ICourseRepository, SqlCourseRepository>();
builder.Services.AddScoped<ICourseModuleRepository, SqlCourseModuleRepository>();
builder.Services.AddScoped<IResourceRepository, SqlResourceRepository>();
builder.Services.AddScoped<IEnrollmentRepository, SqlEnrollmentRepository>();
builder.Services.AddScoped<IEvaluationRepository, SqlEvaluationRepository>();
builder.Services.AddScoped<ISubmissionRepository, SqlSubmissionRepository>();
builder.Services.AddScoped<INotificationRepository, SqlNotificationRepository>();

// UseCases / Handler
builder.Services.AddScoped<CreateUserHandler>();
builder.Services.AddScoped<CreateCourseHandler>();
builder.Services.AddScoped<CreateEnrollmentHandler>();
builder.Services.AddScoped<CreateCourseModuleHandler>();
builder.Services.AddScoped<CreateResourceHandler>();
builder.Services.AddScoped<CreateEvaluationHandler>();
builder.Services.AddScoped<CreateSubmissionHandler>();
builder.Services.AddScoped<CreateNotificationHandler>();

// Build app
var app = builder.Build();

// Swagger in dev
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Seed: usamos el servicio provider para que DbInitializer pueda usar UserManager/RoleManager
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // DbInitializer.SeedAsync ahora recibe IServiceProvider y maneja migraciones internamente
        await DbInitializer.SeedAsync(services);

        // Asegurar roles básicos
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        string[] roles = new[] { "Student", "Teacher", "Admin" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error while migrating/seeding the database.");
    }
}

// Middleware
app.UseHttpsRedirection();

// CORS
app.UseCors("AllowAll");

// Authentication/Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
