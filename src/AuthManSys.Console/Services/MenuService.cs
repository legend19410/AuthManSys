using Microsoft.Extensions.DependencyInjection;
using AuthManSys.Console.Commands;
using Console = System.Console;

namespace AuthManSys.Console.Services;

public class MenuService : IMenuService
{
    private readonly IServiceProvider _serviceProvider;

    public MenuService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task ShowMenuAsync()
    {
        while (true)
        {
            System.Console.Clear();
            ShowHeader();
            ShowMenuOptions();

            var choice = System.Console.ReadLine();

            switch (choice?.ToLower())
            {
                case "1":
                case "user":
                    await ShowUserMenu();
                    break;
                case "2":
                case "auth":
                    await ShowAuthMenu();
                    break;
                case "3":
                case "db":
                    await ShowDatabaseMenu();
                    break;
                case "4":
                case "test":
                    await RunInteractiveTests();
                    break;
                case "q":
                case "quit":
                case "exit":
                    System.Console.WriteLine("Goodbye!");
                    return;
                default:
                    System.Console.WriteLine("Invalid option. Press any key to continue...");
                    System.Console.ReadKey();
                    break;
            }
        }
    }

    private void ShowHeader()
    {
        System.Console.WriteLine("╔══════════════════════════════════════════════════════════════════╗");
        System.Console.WriteLine("║                      AuthManSys Console Tool                       ║");
        System.Console.WriteLine("║                   Custom dotnet tool (cdotnet)                     ║");
        System.Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝");
        System.Console.WriteLine();
    }

    private void ShowMenuOptions()
    {
        System.Console.WriteLine("Main Menu:");
        System.Console.WriteLine("─────────────────────────────────────────────────────────────────");
        System.Console.WriteLine("1. User Management        (user)");
        System.Console.WriteLine("2. Authentication Tests   (auth)");
        System.Console.WriteLine("3. Database Operations    (db)");
        System.Console.WriteLine("4. Interactive Tests      (test)");
        System.Console.WriteLine("Q. Quit                   (quit/exit)");
        System.Console.WriteLine();
        System.Console.Write("Select an option: ");
    }

    private async Task ShowUserMenu()
    {
        var userCommands = _serviceProvider.GetRequiredService<IUserCommands>();

        while (true)
        {
            System.Console.Clear();
            System.Console.WriteLine("User Management");
            System.Console.WriteLine("─────────────────────────────────────────────────────────────────");
            System.Console.WriteLine("1. List all users");
            System.Console.WriteLine("2. Create new user");
            System.Console.WriteLine("3. Delete user");
            System.Console.WriteLine("4. Update user");
            System.Console.WriteLine("5. Find user");
            System.Console.WriteLine("B. Back to main menu");
            System.Console.WriteLine();
            System.Console.Write("Select an option: ");

            var choice = System.Console.ReadLine();

            switch (choice?.ToLower())
            {
                case "1":
                    await userCommands.ListUsersAsync();
                    break;
                case "2":
                    await userCommands.CreateUserAsync();
                    break;
                case "3":
                    await userCommands.DeleteUserAsync();
                    break;
                case "4":
                    await userCommands.UpdateUserAsync();
                    break;
                case "5":
                    await userCommands.FindUserAsync();
                    break;
                case "b":
                case "back":
                    return;
                default:
                    System.Console.WriteLine("Invalid option. Press any key to continue...");
                    System.Console.ReadKey();
                    break;
            }

            if (choice != "b" && choice != "back")
            {
                System.Console.WriteLine("\nPress any key to continue...");
                System.Console.ReadKey();
            }
        }
    }

    private async Task ShowAuthMenu()
    {
        var authCommands = _serviceProvider.GetRequiredService<IAuthCommands>();

        while (true)
        {
            System.Console.Clear();
            System.Console.WriteLine("Authentication Tests");
            System.Console.WriteLine("─────────────────────────────────────────────────────────────────");
            System.Console.WriteLine("1. Test user login");
            System.Console.WriteLine("2. Test user registration");
            System.Console.WriteLine("3. Test token validation");
            System.Console.WriteLine("4. Test password reset");
            System.Console.WriteLine("5. Test email confirmation");
            System.Console.WriteLine("B. Back to main menu");
            System.Console.WriteLine();
            System.Console.Write("Select an option: ");

            var choice = System.Console.ReadLine();

            switch (choice?.ToLower())
            {
                case "1":
                    await authCommands.TestLoginAsync();
                    break;
                case "2":
                    await authCommands.TestRegistrationAsync();
                    break;
                case "3":
                    await authCommands.TestTokenValidationAsync();
                    break;
                case "4":
                    await authCommands.TestPasswordResetAsync();
                    break;
                case "5":
                    await authCommands.TestEmailConfirmationAsync();
                    break;
                case "b":
                case "back":
                    return;
                default:
                    System.Console.WriteLine("Invalid option. Press any key to continue...");
                    System.Console.ReadKey();
                    break;
            }

            if (choice != "b" && choice != "back")
            {
                System.Console.WriteLine("\nPress any key to continue...");
                System.Console.ReadKey();
            }
        }
    }

    private async Task ShowDatabaseMenu()
    {
        var dbCommands = _serviceProvider.GetRequiredService<IDatabaseCommands>();

        while (true)
        {
            System.Console.Clear();
            System.Console.WriteLine("Database Operations");
            System.Console.WriteLine("─────────────────────────────────────────────────────────────────");
            System.Console.WriteLine("1. Seed database");
            System.Console.WriteLine("2. Run migrations");
            System.Console.WriteLine("3. Reset database");
            System.Console.WriteLine("4. Check database status");
            System.Console.WriteLine("B. Back to main menu");
            System.Console.WriteLine();
            System.Console.Write("Select an option: ");

            var choice = System.Console.ReadLine();

            switch (choice?.ToLower())
            {
                case "1":
                    await dbCommands.SeedDatabaseAsync();
                    break;
                case "2":
                    await dbCommands.RunMigrationsAsync();
                    break;
                case "3":
                    await dbCommands.ResetDatabaseAsync();
                    break;
                case "4":
                    await dbCommands.CheckDatabaseStatusAsync();
                    break;
                case "b":
                case "back":
                    return;
                default:
                    System.Console.WriteLine("Invalid option. Press any key to continue...");
                    System.Console.ReadKey();
                    break;
            }

            if (choice != "b" && choice != "back")
            {
                System.Console.WriteLine("\nPress any key to continue...");
                System.Console.ReadKey();
            }
        }
    }

    private async Task RunInteractiveTests()
    {
        var interactiveTests = _serviceProvider.GetRequiredService<IInteractiveTests>();
        await interactiveTests.RunAllTestsAsync();

        System.Console.WriteLine("\nPress any key to continue...");
        System.Console.ReadKey();
    }
}