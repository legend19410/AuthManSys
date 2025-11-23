using AuthManSys.Infrastructure.DependencyInjection;
using AuthManSys.Application.DependencyInjection;
using AuthManSys.Api.DependencyInjection;


var builder = WebApplication.CreateBuilder(args);

// Add Infrastructure services (DbContext, repositories, etc.)
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add Application services (MediatR, handlers, etc.)
builder.Services.AddApplicationServices(builder.Configuration);


builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

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

app.UseCors("AuthManSysCorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();

// Make Program class accessible for testing
public partial class Program { }

