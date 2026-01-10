using AuthManSys.Infrastructure.DependencyInjection;
using AuthManSys.Application.DependencyInjection;
using AuthManSys.Api.DependencyInjection;
using AuthManSys.Infrastructure.Database.EFCore.DbContext;
using AuthManSys.Api.Middleware;
using Microsoft.EntityFrameworkCore;
using Hangfire;
using Hangfire.Dashboard;
using AuthManSys.Application.Common.Interfaces;


var builder = WebApplication.CreateBuilder(args);

// Add Infrastructure services (DbContext, repositories, etc.)
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add Application services (MediatR, handlers, etc.)
builder.Services.AddApplicationServices(builder.Configuration);


builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AuthManSysDbContext>();
    context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
// Enable Swagger for all environments
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "AuthManSys API v1");
    c.RoutePrefix = string.Empty;
});

// Only use HTTPS redirection when not in Docker container
if (!app.Environment.IsEnvironment("Docker") && !Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER")?.Equals("true", StringComparison.OrdinalIgnoreCase) == true)
{
    app.UseHttpsRedirection();
}

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseCors("AuthManSysCorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

// Add Hangfire Dashboard (only in development for security)
if (app.Environment.IsDevelopment())
{
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = Array.Empty<IDashboardAuthorizationFilter>()
    });
}

app.MapControllers();

// Configure recurring jobs
using (var scope = app.Services.CreateScope())
{
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

    // Schedule the activity report job to run every minute
    recurringJobManager.AddOrUpdate<IActivityLogReportJob>(
        "user-activity-report",
        job => job.GenerateUserActivityReportAsync(),
        Cron.Minutely); // This will run every minute
}

app.Run();

// Make Program class accessible for testing
public partial class Program { }

