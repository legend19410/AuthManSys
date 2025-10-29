using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using AuthManSys.Application.Common.Interfaces;
using AuthManSys.Infrastructure.Persistence;
using AuthManSys.Domain.Entities;
using AuthManSys.Infrastructure.Identity;

namespace AuthManSys.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Get database provider setting
        var databaseProvider = configuration["DatabaseProvider"] ?? "SQLite";
        
        // Add DbContext based on provider
        services.AddDbContext<AuthManSysDbContext>(options =>
        {
            if (databaseProvider.ToUpper() == "SQLSERVER")
            {
                options.UseSqlServer(
                    configuration.GetConnectionString("SqlServerConnection"),
                    b => b.MigrationsAssembly(typeof(AuthManSysDbContext).Assembly.FullName)
                );
            }
            else if (databaseProvider.ToUpper() == "MYSQL")
            {
                options.UseMySql(
                    configuration.GetConnectionString("MySqlConnection"),
                    ServerVersion.AutoDetect(configuration.GetConnectionString("MySqlConnection")),
                    b => b.MigrationsAssembly(typeof(AuthManSysDbContext).Assembly.FullName)
                );
            }
            else
            {
                options.UseSqlite(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(AuthManSysDbContext).Assembly.FullName)
                );
            }
        });

        // Add ASP.NET Identity
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            // Password settings
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;

            // User settings
            options.User.RequireUniqueEmail = true;

            // Sign-in settings
            options.SignIn.RequireConfirmedEmail = false;
            options.SignIn.RequireConfirmedPhoneNumber = false;
        })
        .AddEntityFrameworkStores<AuthManSysDbContext>()
        .AddDefaultTokenProviders();

        // Add repositories
        services.AddScoped<IAuthManSysDbContext, AuthManSysDbContext>();

        // Add Identity Extension
        services.AddScoped<IdentityExtension>();
        services.AddScoped<IIdentityExtension, IdentityExtension>();

        // Add other infrastructure services

        return services;
    }
}