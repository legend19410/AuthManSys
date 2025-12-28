using Microsoft.Extensions.DependencyInjection;
using AuthManSys.Console.Services;
using AuthManSys.Console.Commands;

namespace AuthManSys.Console.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConsoleServices(this IServiceCollection services)
    {
        // Register menu service
        services.AddScoped<IMenuService, MenuService>();

        // Register command services
        services.AddScoped<IUserCommands, UserCommands>();
        services.AddScoped<IAuthCommands, AuthCommands>();
        services.AddScoped<IDatabaseCommands, DatabaseCommands>();
        services.AddScoped<IInteractiveTests, InteractiveTests>();
        services.AddScoped<IGoogleDocsCommands, GoogleDocsCommands>();

        return services;
    }
}