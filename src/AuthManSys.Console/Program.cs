using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using AuthManSys.Application.DependencyInjection;
using AuthManSys.Infrastructure.DependencyInjection;
using AuthManSys.Console.Services;
using AuthManSys.Console.DependencyInjection;
using AuthManSys.Console.Commands;

namespace AuthManSys.Console;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var host = CreateHost();

        var rootCommand = new RootCommand("AuthManSys Console - Custom dotnet tool for AuthManSys operations");

        // Add commands
        var menuCommand = new Command("menu", "Start interactive menu");
        var userCommand = CreateUserCommand(host);
        var authCommand = CreateAuthCommand(host);
        var dbCommand = CreateDatabaseCommand(host);

        rootCommand.AddCommand(menuCommand);
        rootCommand.AddCommand(userCommand);
        rootCommand.AddCommand(authCommand);
        rootCommand.AddCommand(dbCommand);

        // Set handlers
        menuCommand.SetHandler(async () =>
        {
            var menuService = host.Services.GetRequiredService<IMenuService>();
            await menuService.ShowMenuAsync();
        });

        // If no arguments provided, show interactive menu
        if (args.Length == 0)
        {
            var menuService = host.Services.GetRequiredService<IMenuService>();
            await menuService.ShowMenuAsync();
            return 0;
        }

        return await rootCommand.InvokeAsync(args);
    }

    static IHost CreateHost()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        return Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton<IConfiguration>(configuration);
                services.AddInfrastructureServices(configuration);
                services.AddApplicationServices(configuration);
                services.AddConsoleServices();
            })
            .Build();
    }

    static Command CreateUserCommand(IHost host)
    {
        var userCommand = new Command("user", "User management operations");

        var listUsersCommand = new Command("list", "List all users");
        var createUserCommand = new Command("create", "Create a new user");
        var deleteUserCommand = new Command("delete", "Delete a user");

        listUsersCommand.SetHandler(async () =>
        {
            var userCommands = host.Services.GetRequiredService<IUserCommands>();
            await userCommands.ListUsersAsync();
        });

        userCommand.AddCommand(listUsersCommand);
        userCommand.AddCommand(createUserCommand);
        userCommand.AddCommand(deleteUserCommand);

        return userCommand;
    }

    static Command CreateAuthCommand(IHost host)
    {
        var authCommand = new Command("auth", "Authentication operations");

        var loginCommand = new Command("login", "Test user login");
        var registerCommand = new Command("register", "Test user registration");

        authCommand.AddCommand(loginCommand);
        authCommand.AddCommand(registerCommand);

        return authCommand;
    }

    static Command CreateDatabaseCommand(IHost host)
    {
        var dbCommand = new Command("db", "Database operations");

        var seedCommand = new Command("seed", "Seed database with initial data");
        var migrateCommand = new Command("migrate", "Run database migrations");
        var statusCommand = new Command("status", "Check database status");
        var resetCommand = new Command("reset", "Reset database (delete all data and reseed)");

        // Add handlers for db commands
        seedCommand.SetHandler(async () =>
        {
            var dbCommands = host.Services.GetRequiredService<IDatabaseCommands>();
            await dbCommands.SeedDatabaseAsync();
        });

        migrateCommand.SetHandler(async () =>
        {
            var dbCommands = host.Services.GetRequiredService<IDatabaseCommands>();
            await dbCommands.RunMigrationsAsync();
        });

        statusCommand.SetHandler(async () =>
        {
            var dbCommands = host.Services.GetRequiredService<IDatabaseCommands>();
            await dbCommands.CheckDatabaseStatusAsync();
        });

        resetCommand.SetHandler(async () =>
        {
            var dbCommands = host.Services.GetRequiredService<IDatabaseCommands>();
            await dbCommands.ResetDatabaseAsync();
        });

        dbCommand.AddCommand(seedCommand);
        dbCommand.AddCommand(migrateCommand);
        dbCommand.AddCommand(statusCommand);
        dbCommand.AddCommand(resetCommand);

        return dbCommand;
    }
}
