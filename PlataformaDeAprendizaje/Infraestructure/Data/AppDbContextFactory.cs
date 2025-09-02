// Infrastructure/Data/AppDbContextFactory.cs
using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var basePath = Directory.GetCurrentDirectory();

            // Intentamos localizar la carpeta Api (ajusta si tu solución tiene otra estructura)
            var apiFolder = Path.GetFullPath(Path.Combine(basePath, "..", "Api"));
            if (!Directory.Exists(apiFolder))
            {
                apiFolder = Path.GetFullPath(Path.Combine(basePath, "..", "..", "..", "Api"));
            }

            var configBuilder = new ConfigurationBuilder();

            if (Directory.Exists(apiFolder))
            {
                configBuilder.SetBasePath(apiFolder)
                             .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                             .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
                             .AddEnvironmentVariables();
            }
            else
            {
                configBuilder.SetBasePath(basePath)
                             .AddEnvironmentVariables();
            }

            var configuration = configBuilder.Build();

            var connectionString = Environment.GetEnvironmentVariable("DEFAULT_CONNECTION")
                                   ?? configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString = "Data Source=DESKTOP-TG2HJGA\\SQLEXPRESS;Initial Catalog=PlatformDB;Integrated Security=True;TrustServerCertificate=True";
            }

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            // Asegúrate de que el nombre pasado a MigrationsAssembly coincide con tu proyecto
            optionsBuilder.UseSqlServer(connectionString, sql => sql.MigrationsAssembly("Infraestructure"));

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
