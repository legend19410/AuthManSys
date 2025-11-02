using Microsoft.AspNetCore.Identity;
using AuthManSys.Api.Models;
using AuthManSys.Infrastructure.DependencyInjection;
using AuthManSys.Infrastructure.Persistence;
using AuthManSys.Domain.Entities;
using AuthManSys.Application.DependencyInjection;
using AuthManSys.Api.DependencyInjection;

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

builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

// Seed database in development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AuthManSysDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await IdentitySeeder.SeedAsync(context, userManager, roleManager);

    // Interactive IdentityManager testing (only when not in container)
    if (!Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER")?.Equals("true", StringComparison.OrdinalIgnoreCase) == true)
    {
        await InteractiveIdentityTests.RunInteractiveTest(scope);
    }
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

// Only use HTTPS redirection when not in Docker container
if (!app.Environment.IsEnvironment("Docker") && !Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER")?.Equals("true", StringComparison.OrdinalIgnoreCase) == true)
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();

// Make Program class accessible for testing
public partial class Program { }

