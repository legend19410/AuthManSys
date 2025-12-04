using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using AuthManSys.Application.Common.Interfaces;
using AuthManSys.Application.Common.Models;
using AuthManSys.Infrastructure.Database.DbContext;
using AuthManSys.Domain.Entities;
using AuthManSys.Infrastructure.Services;
using AuthManSys.Infrastructure.Authorization;

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

        // Add Identity Service
        services.AddScoped<IdentityService>();
        services.AddScoped<IIdentityService, IdentityService>();

        // Add Permission Services
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IPermissionCacheManager, PermissionCacheManager>();

        // Add Authorization components
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

        // Add Memory Cache for permission caching
        services.AddMemoryCache();

        // Configure EmailSettings
        services.Configure<EmailSettings>(options =>
        {
            configuration.GetSection("EmailSettings").Bind(options);
        });

        // Add Email Service
        services.AddScoped<IEmailService, EmailService>();

        // Add Two-Factor Authentication Service
        services.AddScoped<ITwoFactorService, TwoFactorService>();

        // Add Activity Logging Service
        services.AddScoped<IActivityLogService, ActivityLogService>();

        return services;
    }
}