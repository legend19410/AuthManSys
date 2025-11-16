
using System.Text;
using AuthManSys.Application.Common.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace AuthManSys.Api.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        // JWT service is now registered in ApplicationServices
        // Add services to the container.
        
        services.AddControllers();
        services.AddEndpointsApiExplorer();

        // Configure JWT Authentication
        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();
        var key = Encoding.ASCII.GetBytes(jwtSettings!.SecretKey);

        // Register Application JwtSettings for SecurityService
        var appJwtSettings = new JwtSettings
        {
            SecretKey = jwtSettings.SecretKey,
            Issuer = jwtSettings.Issuer,
            Audience = jwtSettings.Audience,
            ExpirationInMinutes = jwtSettings.ExpirationInMinutes
        };

        services.Configure<JwtSettings>(options =>
        {
            options.SecretKey = jwtSettings.SecretKey;
            options.Issuer = jwtSettings.Issuer;
            options.Audience = jwtSettings.Audience;
            options.ExpirationInMinutes = jwtSettings.ExpirationInMinutes;
        });

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        services.AddSingleton(tokenValidationParameters);


        // Register Security Options

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = tokenValidationParameters;
        });

        services.AddAuthorization();

        // Configure CORS
        var corsSettings = configuration.GetSection("CorsSettings").Get<CorsSettings>();
        services.AddCors(options =>
        {
            options.AddPolicy(corsSettings?.PolicyName ?? "DefaultCorsPolicy", policy =>
            {
                if (corsSettings?.AllowAnyOrigin == true)
                {
                    policy.AllowAnyOrigin();
                }
                else if (corsSettings?.AllowedOrigins?.Length > 0)
                {
                    policy.WithOrigins(corsSettings.AllowedOrigins);
                }

                if (corsSettings?.AllowAnyMethod == true)
                {
                    policy.AllowAnyMethod();
                }
                else if (corsSettings?.AllowedMethods?.Length > 0)
                {
                    policy.WithMethods(corsSettings.AllowedMethods);
                }

                if (corsSettings?.AllowAnyHeader == true)
                {
                    policy.AllowAnyHeader();
                }
                else if (corsSettings?.AllowedHeaders?.Length > 0)
                {
                    policy.WithHeaders(corsSettings.AllowedHeaders);
                }

                if (corsSettings?.ExposedHeaders?.Length > 0)
                {
                    policy.WithExposedHeaders(corsSettings.ExposedHeaders);
                }

                if (corsSettings?.AllowCredentials == true)
                {
                    policy.AllowCredentials();
                }

                if (corsSettings?.PreflightMaxAge > 0)
                {
                    policy.SetPreflightMaxAge(TimeSpan.FromSeconds(corsSettings.PreflightMaxAge));
                }
            });
        });

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo 
            { 
                Title = "AuthManSys API", 
                Version = "v1",
                Description = "Authentication Management System API"
            });

            // Add JWT authentication to Swagger
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme (Example: 'Bearer 12345abcdef')",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });


        return services;
    }
}