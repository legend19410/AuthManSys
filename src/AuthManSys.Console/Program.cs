using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using AuthManSys.Application.DependencyInjection;
using AuthManSys.Infrastructure.DependencyInjection;
using AuthManSys.Console.Services;
using AuthManSys.Console.DependencyInjection;
using AuthManSys.Console.Commands;
using AuthManSys.Console.Utilities;

namespace AuthManSys.Console;

class Program
{
    static async Task<int> Main(string[] args)
    {
        // Configure the services 
        // set appsettings.json as the base configuration file
        var host = CreateHost();

        var rootCommand = new RootCommand("AuthManSys Console - Custom dotnet tool for AuthManSys operations");

        // Add commands
        var menuCommand = new Command("menu", "Start interactive menu");
        var userCommand = CreateUserCommand(host);
        var authCommand = CreateAuthCommand(host);
        var dbCommand = CreateDatabaseCommand(host);
        var googleCommand = CreateGoogleDocsCommand(host);

        rootCommand.AddCommand(menuCommand);
        rootCommand.AddCommand(userCommand);
        rootCommand.AddCommand(authCommand);
        rootCommand.AddCommand(dbCommand);
        rootCommand.AddCommand(googleCommand);

        // Set handlers
        menuCommand.SetHandler(async () =>
        {
            var menuService = host.Services.GetRequiredService<IMenuService>();
            await menuService.ShowMenuAsync();
        });

        // If no arguments provided, show interactive menu (but only if input is available)
        if (args.Length == 0)
        {
            if (SafeConsole.IsInputRedirected)
            {
                // Input is redirected (like in VSCode debugger), show help instead
                SafeConsole.WriteLine("AuthManSys Console - Custom dotnet tool for AuthManSys operations");
                SafeConsole.WriteLine();
                SafeConsole.WriteLine("Console input is redirected. Available commands:");
                SafeConsole.WriteLine("  db status    - Check database status");
                SafeConsole.WriteLine("  db migrate   - Run database migrations");
                SafeConsole.WriteLine("  db seed      - Seed database with initial data");
                SafeConsole.WriteLine("  db reset     - Reset database (delete all data and reseed)");
                SafeConsole.WriteLine("  user list    - List all users");
                SafeConsole.WriteLine("  google create  - Create a new Google Document");
                SafeConsole.WriteLine("  google write   - Write content to a Google Document");
                SafeConsole.WriteLine("  google list    - List Google Documents");
                SafeConsole.WriteLine("  menu         - Start interactive menu (requires console input)");
                SafeConsole.WriteLine();
                SafeConsole.WriteLine("Example: dotnet run -- db status");
                return 0;
            }
            else
            {
                var menuService = host.Services.GetRequiredService<IMenuService>();
                await menuService.ShowMenuAsync();
                return 0;
            }
        }

        return await rootCommand.InvokeAsync(args);
    }

    static IHost CreateHost()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("src/AuthManSys.Console/appsettings.json", optional: true)
            .AddJsonFile("src/AuthManSys.Console/appsettings.Development.json", optional: true)
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

        // What happens when "user list" command is called       
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

    static Command CreateGoogleDocsCommand(IHost host)
    {
        var googleCommand = new Command("google", "Google Docs operations");

        var createCommand = new Command("create", "Create a new Google Document");
        var writeCommand = new Command("write", "Write content to a Google Document");
        var createWithContentCommand = new Command("create-with-content", "Create a new Google Document with initial content");
        var listCommand = new Command("list", "List Google Documents");
        var infoCommand = new Command("info", "Get information about a Google Document");
        var shareCommand = new Command("share", "Share a Google Document");
        var exportCommand = new Command("export", "Export a Google Document");

        // Add arguments for commands
        var titleArgument = new Argument<string>("title", "The title of the document");
        var documentIdArgument = new Argument<string>("document-id", "The ID of the document");
        var contentArgument = new Argument<string>("content", "The content to write");
        var emailArgument = new Argument<string>("email", "Email address to share with");
        var roleOption = new Option<string>("--role", () => "reader", "Role for sharing (reader, writer, editor)");
        var formatOption = new Option<string>("--format", () => "text", "Export format (text, pdf)");

        createCommand.AddArgument(titleArgument);
        writeCommand.AddArgument(documentIdArgument);
        writeCommand.AddArgument(contentArgument);
        createWithContentCommand.AddArgument(titleArgument);
        createWithContentCommand.AddArgument(contentArgument);
        infoCommand.AddArgument(documentIdArgument);
        shareCommand.AddArgument(documentIdArgument);
        shareCommand.AddArgument(emailArgument);
        shareCommand.AddOption(roleOption);
        exportCommand.AddArgument(documentIdArgument);
        exportCommand.AddOption(formatOption);

        // Add handlers for google commands
        createCommand.SetHandler(async (title) =>
        {
            var googleCommands = host.Services.GetRequiredService<IGoogleDocsCommands>();
            await googleCommands.CreateDocumentAsync(title);
        }, titleArgument);

        writeCommand.SetHandler(async (documentId, content) =>
        {
            var googleCommands = host.Services.GetRequiredService<IGoogleDocsCommands>();
            await googleCommands.WriteToDocumentAsync(documentId, content);
        }, documentIdArgument, contentArgument);

        createWithContentCommand.SetHandler(async (title, content) =>
        {
            var googleCommands = host.Services.GetRequiredService<IGoogleDocsCommands>();
            await googleCommands.CreateAndWriteAsync(title, content);
        }, titleArgument, contentArgument);

        listCommand.SetHandler(async () =>
        {
            var googleCommands = host.Services.GetRequiredService<IGoogleDocsCommands>();
            await googleCommands.ListDocumentsAsync();
        });

        infoCommand.SetHandler(async (documentId) =>
        {
            var googleCommands = host.Services.GetRequiredService<IGoogleDocsCommands>();
            await googleCommands.GetDocumentInfoAsync(documentId);
        }, documentIdArgument);

        shareCommand.SetHandler(async (documentId, email, role) =>
        {
            var googleCommands = host.Services.GetRequiredService<IGoogleDocsCommands>();
            await googleCommands.ShareDocumentAsync(documentId, email, role);
        }, documentIdArgument, emailArgument, roleOption);

        exportCommand.SetHandler(async (documentId, format) =>
        {
            var googleCommands = host.Services.GetRequiredService<IGoogleDocsCommands>();
            await googleCommands.ExportDocumentAsync(documentId, format);
        }, documentIdArgument, formatOption);

        googleCommand.AddCommand(createCommand);
        googleCommand.AddCommand(writeCommand);
        googleCommand.AddCommand(createWithContentCommand);
        googleCommand.AddCommand(listCommand);
        googleCommand.AddCommand(infoCommand);
        googleCommand.AddCommand(shareCommand);
        googleCommand.AddCommand(exportCommand);

        return googleCommand;
    }
}
