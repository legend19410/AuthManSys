using Microsoft.AspNetCore.Identity;
using AuthManSys.Domain.Entities;
using AuthManSys.Infrastructure.Identity;

namespace AuthManSys.Api.ConsoleTest;

public static class InteractiveIdentityTests
{
    public static async Task RunInteractiveTest(IServiceScope scope)
    {
        var identityExtension = scope.ServiceProvider.GetRequiredService<IdentityExtension>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        System.Console.WriteLine("\n=== Interactive identityExtension Test Console ===");

        while (true)
        {
            System.Console.WriteLine("\nChoose an option:");
            System.Console.WriteLine("1. Find user by username");
            System.Console.WriteLine("2. Verify user password");
            System.Console.WriteLine("3. Check if email is confirmed");
            System.Console.WriteLine("4. Generate email confirmation token");
            System.Console.WriteLine("5. Generate password reset token");
            System.Console.WriteLine("6. List all users");
            System.Console.WriteLine("0. Exit and start API");
            System.Console.Write("\nEnter your choice (0-6): ");

            var choice = System.Console.ReadLine();

            try
            {
                switch (choice)
                {
                    case "1":
                        await TestFindUser(identityExtension);
                        break;
                    case "2":
                        await TestVerifyPassword(userManager);
                        break;
                    case "3":
                        await TestEmailConfirmed(identityExtension);
                        break;
                    case "4":
                        await TestGenerateEmailToken(identityExtension);
                        break;
                    case "5":
                        await TestGeneratePasswordResetToken(identityExtension);
                        break;
                    case "6":
                        await ListAllUsers(userManager);
                        break;
                    case "0":
                        System.Console.WriteLine("Starting API server...");
                        return;
                    default:
                        System.Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    private static async Task TestFindUser(IdentityExtension identityExtension)
    {
        System.Console.Write("Enter username to find: ");
        var username = System.Console.ReadLine();

        if (string.IsNullOrEmpty(username)) return;

        var user = await identityExtension.FindByUserNameAsync(username);
        if (user != null)
        {
            System.Console.WriteLine($"✓ User found: {user.UserName} ({user.Email})");
            System.Console.WriteLine($"  Name: {user.FirstName} {user.LastName}");
            System.Console.WriteLine($"  Email Confirmed: {user.EmailConfirmed}");
        }
        else
        {
            System.Console.WriteLine("✗ User not found");
        }
    }

    private static async Task TestVerifyPassword(UserManager<ApplicationUser> userManager)
    {
        System.Console.Write("Enter username: ");
        var username = System.Console.ReadLine();
        System.Console.Write("Enter password: ");
        var password = System.Console.ReadLine();

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) return;

        var user = await userManager.FindByNameAsync(username);
        if (user == null)
        {
            System.Console.WriteLine("✗ User not found");
            return;
        }

        var isValid = await userManager.CheckPasswordAsync(user, password);
        System.Console.WriteLine(isValid ? "✓ Password is correct" : "✗ Password is incorrect");
    }


   

    private static async Task TestEmailConfirmed(IdentityExtension identityExtension)
    {
        System.Console.Write("Enter username: ");
        var username = System.Console.ReadLine();

        if (string.IsNullOrEmpty(username)) return;

        var isConfirmed = await identityExtension.IsEmailConfirmedAsync(username);
        System.Console.WriteLine(isConfirmed ? "✓ Email is confirmed" : "✗ Email is not confirmed");
    }

    private static async Task TestGenerateEmailToken(IdentityExtension identityExtension)
    {
        System.Console.Write("Enter username: ");
        var username = System.Console.ReadLine();

        if (string.IsNullOrEmpty(username)) return;

        var token = await identityExtension.GenerateEmailConfirmationTokenAsync(username);
        System.Console.WriteLine($"✓ Email confirmation token: {token}");
    }

    private static async Task TestGeneratePasswordResetToken(IdentityExtension identityExtension)
    {
        System.Console.Write("Enter username: ");
        var username = System.Console.ReadLine();

        if (string.IsNullOrEmpty(username)) return;

        var token = await identityExtension.GeneratePasswordResetTokenAsync(username);
        System.Console.WriteLine($"✓ Password reset token: {token}");
    }

    private static async Task ListAllUsers(UserManager<ApplicationUser> userManager)
    {
        var users = userManager.Users.ToList();
        System.Console.WriteLine($"\n✓ Found {users.Count} users:");
        foreach (var user in users)
        {
            System.Console.WriteLine($"  - {user.UserName} ({user.Email}) - {user.FirstName} {user.LastName}");
        }
    }
}