using Microsoft.Extensions.DependencyInjection;
using AuthManSys.Console.Commands;

namespace AuthManSys.Console.Commands;

public class InteractiveTests : IInteractiveTests
{
    private readonly IServiceProvider _serviceProvider;

    public InteractiveTests(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task RunAllTestsAsync()
    {
        System.Console.WriteLine("🧪 Running Interactive Tests");
        System.Console.WriteLine("─────────────────────────────────────────────────────────────────");

        try
        {
            await RunDatabaseTests();
            await RunUserTests();
            await RunAuthTests();

            System.Console.WriteLine("✅ All interactive tests completed!");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"❌ Error running tests: {ex.Message}");
        }
    }

    private async Task RunDatabaseTests()
    {
        System.Console.WriteLine("\n🗄️ Database Tests");
        System.Console.WriteLine("─────────────────────────────────────────────────────────────────");

        var dbCommands = _serviceProvider.GetRequiredService<IDatabaseCommands>();
        await dbCommands.CheckDatabaseStatusAsync();
    }

    private async Task RunUserTests()
    {
        System.Console.WriteLine("\n👥 User Management Tests");
        System.Console.WriteLine("─────────────────────────────────────────────────────────────────");

        var userCommands = _serviceProvider.GetRequiredService<IUserCommands>();
        await userCommands.ListUsersAsync();
    }

    private async Task RunAuthTests()
    {
        System.Console.WriteLine("\n🔐 Authentication Tests");
        System.Console.WriteLine("─────────────────────────────────────────────────────────────────");

        System.Console.WriteLine("Demo authentication flow:");
        System.Console.WriteLine("1. User registration");
        System.Console.WriteLine("2. User login");
        System.Console.WriteLine("3. Token validation");
        System.Console.WriteLine();
        System.Console.WriteLine("Use the individual menu options to test these features interactively.");
    }
}