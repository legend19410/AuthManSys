using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using AuthManSys.Api.Models;
using AuthManSys.Infrastructure.DependencyInjection;
using AuthManSys.Infrastructure.Persistence;
using AuthManSys.Domain.Entities;
using AuthManSys.Application.DependencyInjection;
using AuthManSys.Api.ConsoleTest;

var builder = WebApplication.CreateBuilder(args);

// Configure JWT settings
//builder.Services.Configure<AuthManSys.Api.Models.JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<DefaultCredentials>(builder.Configuration.GetSection("DefaultCredentials"));

// Add Infrastructure services (DbContext, repositories, etc.)
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add Application services (MediatR, handlers, etc.)
builder.Services.AddApplicationServices(builder.Configuration);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// JWT service is now registered in ApplicationServices

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
var key = Encoding.ASCII.GetBytes(jwtSettings!.SecretKey);

// Register Application JwtSettings for SecurityService
var appJwtSettings = new AuthManSys.Application.Common.Models.JwtSettings
{
    SecretKey = jwtSettings.SecretKey,
    Issuer = jwtSettings.Issuer,
    Audience = jwtSettings.Audience,
    ExpirationInMinutes = jwtSettings.ExpiryMinutes
};
builder.Services.Configure<AuthManSys.Application.Common.Models.JwtSettings>(options =>
{
    options.SecretKey = jwtSettings.SecretKey;
    options.Issuer = jwtSettings.Issuer;
    options.Audience = jwtSettings.Audience;
    options.ExpirationInMinutes = jwtSettings.ExpiryMinutes;
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

builder.Services.AddSingleton(tokenValidationParameters);


// Register Security Options

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = tokenValidationParameters;
});

builder.Services.AddAuthorization();

builder.Services.AddSwaggerGen(c =>
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

var app = builder.Build();

// Seed database in development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AuthManSysDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await IdentitySeeder.SeedAsync(context, userManager, roleManager);

    // Interactive IdentityManager testing
    await InteractiveIdentityTests.RunInteractiveTest(scope);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AuthManSys API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();

// Make Program class accessible for testing
public partial class Program { }

