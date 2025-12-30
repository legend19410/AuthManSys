using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Google;
using AuthManSys.Application.Common.Interfaces;
using AuthManSys.Application.Common.Models;
using AuthManSys.Application.Common.Services;
using AuthManSys.Infrastructure.Database.DbContext;
using AuthManSys.Domain.Entities;
using AuthManSys.Infrastructure.Authorization;
using AuthManSys.Infrastructure.Database.Repositories;
using AuthManSys.Infrastructure.Email;
using AuthManSys.Infrastructure.Cache;
using AuthManSys.Infrastructure.GoogleApi.Configuration;
using AuthManSys.Infrastructure.GoogleApi.Authentication;
using AuthManSys.Infrastructure.GoogleApi.Services;

namespace AuthManSys.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Get database provider setting
        var databaseProvider = configuration["DatabaseProvider"] ?? "MySQL";

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
                var connectionString = configuration.GetConnectionString("MySqlConnection");
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException("MySqlConnection connection string is not configured or is empty. Please check your appsettings.json file.");
                }
                options.UseMySql(
                    connectionString,
                    ServerVersion.AutoDetect(connectionString),
                    b => b.MigrationsAssembly(typeof(AuthManSysDbContext).Assembly.FullName)
                );
            }
            else
            {
                throw new InvalidOperationException($"Unsupported database provider: {databaseProvider}. Supported providers are: MySQL, SqlServer");
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

        // Add Google OAuth authentication
        services.AddAuthentication()
            .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
            {
                options.ClientId = configuration["GoogleAuth:ClientId"] ?? "";
                options.ClientSecret = configuration["GoogleAuth:ClientSecret"] ?? "";
            });

        // DbContext is now encapsulated within repositories

        // Add Identity Provider (Infrastructure)
        // services.AddScoped<IIdentityProvider, IdentityProvider>(); // REMOVED - functionality moved to UserRepository, JwtService, and TokenRepository


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

        // Add Two-Factor Authentication Service (from Application layer)
        services.AddScoped<ITwoFactorService, TwoFactorService>();

        // Add Google Token Service
        services.AddScoped<IGoogleTokenService, GoogleTokenService>();

        // Add Activity Logging Repository
        services.AddScoped<IActivityLogRepository, ActivityLogRepository>();

        // Add Repository Services
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<ITokenRepository, TokenRepository>();

        // Add Google API services
        services.AddGoogleApiServices(configuration);

        return services;
    }

    public static IServiceCollection AddGoogleApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure Google API settings
        services.Configure<GoogleApiSettings>(options =>
        {
            configuration.GetSection(GoogleApiSettings.SectionName).Bind(options);
        });

        // Add Google API services
        services.AddScoped<IGoogleAuthService, GoogleAuthService>();
        services.AddScoped<IGoogleDriveService, GoogleDriveService>();
        services.AddScoped<IGoogleDocsService, GoogleDocsService>();

        return services;
    }
}