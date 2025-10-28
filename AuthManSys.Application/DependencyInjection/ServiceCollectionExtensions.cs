using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using AuthManSys.Application.Security.Services;
using AuthManSys.Application.Common.Interfaces;

namespace AuthManSys.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });
    
        // Register Security Services
        services.AddScoped<ISecurityService, SecurityService>();

        return services;
    }
}