using MediatR;
using AuthManSys.Application.Login.Commands;
using Microsoft.AspNetCore.Identity;
using AuthManSys.Domain.Entities;
using Console = System.Console;

namespace AuthManSys.Console.Commands;

public class AuthCommands : IAuthCommands
{
    private readonly IMediator _mediator;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthCommands(IMediator mediator, UserManager<ApplicationUser> userManager)
    {
        _mediator = mediator;
        _userManager = userManager;
    }

    public async Task TestLoginAsync()
    {
        System.Console.WriteLine("🔐 Test User Login");
        System.Console.WriteLine("─────────────────────────────────────────────────────────────────");

        System.Console.Write("Enter username or email: ");
        var username = System.Console.ReadLine();

        System.Console.Write("Enter password: ");
        var password = ReadPassword();

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            System.Console.WriteLine("\n❌ Username and password are required.");
            return;
        }

        try
        {
            var loginCommand = new LoginCommand(username, password, false);

            var result = await _mediator.Send(loginCommand);

            if (!string.IsNullOrEmpty(result.Token))
            {
                System.Console.WriteLine("\n✅ Login successful!");
                System.Console.WriteLine($"Token: {result.Token}");
                System.Console.WriteLine($"Username: {result.Username}");
                System.Console.WriteLine($"Email: {result.Email}");
            }
            else
            {
                System.Console.WriteLine($"\n❌ Login failed: Invalid credentials");
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"\n❌ Error during login: {ex.Message}");
        }
    }

    public async Task TestRegistrationAsync()
    {
        System.Console.WriteLine("📝 Test User Registration");
        System.Console.WriteLine("─────────────────────────────────────────────────────────────────");

        System.Console.Write("Enter username: ");
        var username = System.Console.ReadLine();

        System.Console.Write("Enter email: ");
        var email = System.Console.ReadLine();

        System.Console.Write("Enter first name: ");
        var firstName = System.Console.ReadLine();

        System.Console.Write("Enter last name: ");
        var lastName = System.Console.ReadLine();

        System.Console.Write("Enter password: ");
        var password = ReadPassword();

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) ||
            string.IsNullOrWhiteSpace(password))
        {
            System.Console.WriteLine("\n❌ All fields are required.");
            return;
        }

        try
        {
            var user = new ApplicationUser
            {
                UserName = username,
                Email = email,
                FirstName = firstName,
                LastName = lastName
            };

            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                System.Console.WriteLine("\n✅ Registration successful!");
                System.Console.WriteLine($"User ID: {user.Id}");
            }
            else
            {
                System.Console.WriteLine($"\n❌ Registration failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"\n❌ Error during registration: {ex.Message}");
        }
    }

    public async Task TestTokenValidationAsync()
    {
        System.Console.WriteLine("🔍 Test Token Validation");
        System.Console.WriteLine("─────────────────────────────────────────────────────────────────");

        System.Console.Write("Enter JWT token: ");
        var token = System.Console.ReadLine();

        if (string.IsNullOrWhiteSpace(token))
        {
            System.Console.WriteLine("❌ Token is required.");
            return;
        }

        try
        {
            // For now, just show basic token information
            // In a real implementation, you would validate the token using your security service
            var tokenParts = token.Split('.');

            if (tokenParts.Length != 3)
            {
                System.Console.WriteLine("❌ Invalid JWT token format.");
                return;
            }

            System.Console.WriteLine("✅ Token format is valid (3 parts)");
            System.Console.WriteLine($"Header: {tokenParts[0]}");
            System.Console.WriteLine($"Payload: {tokenParts[1]}");
            System.Console.WriteLine($"Signature: {tokenParts[2]}");

            // TODO: Add actual token validation logic using SecurityService
            System.Console.WriteLine("\n⚠️ Note: Full token validation not implemented yet.");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"❌ Error validating token: {ex.Message}");
        }
    }

    public async Task TestPasswordResetAsync()
    {
        System.Console.WriteLine("🔄 Test Password Reset");
        System.Console.WriteLine("─────────────────────────────────────────────────────────────────");

        System.Console.Write("Enter username or email: ");
        var identifier = System.Console.ReadLine();

        if (string.IsNullOrWhiteSpace(identifier))
        {
            System.Console.WriteLine("❌ Username or email is required.");
            return;
        }

        try
        {
            // TODO: Implement password reset using Application layer commands
            System.Console.WriteLine($"✅ Password reset request would be sent for: {identifier}");
            System.Console.WriteLine("⚠️ Note: Password reset functionality not implemented yet.");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"❌ Error during password reset: {ex.Message}");
        }

        await Task.CompletedTask;
    }

    public async Task TestEmailConfirmationAsync()
    {
        System.Console.WriteLine("📧 Test Email Confirmation");
        System.Console.WriteLine("─────────────────────────────────────────────────────────────────");

        System.Console.Write("Enter username or email: ");
        var identifier = System.Console.ReadLine();

        System.Console.Write("Enter confirmation token: ");
        var token = System.Console.ReadLine();

        if (string.IsNullOrWhiteSpace(identifier) || string.IsNullOrWhiteSpace(token))
        {
            System.Console.WriteLine("❌ Username/email and token are required.");
            return;
        }

        try
        {
            // TODO: Implement email confirmation using Application layer commands
            System.Console.WriteLine($"✅ Email confirmation would be processed for: {identifier}");
            System.Console.WriteLine($"Token: {token}");
            System.Console.WriteLine("⚠️ Note: Email confirmation functionality not implemented yet.");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"❌ Error during email confirmation: {ex.Message}");
        }

        await Task.CompletedTask;
    }

    private static string ReadPassword()
    {
        var password = "";
        ConsoleKeyInfo key;

        do
        {
            key = System.Console.ReadKey(true);

            if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
            {
                password += key.KeyChar;
                System.Console.Write("*");
            }
            else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
            {
                password = password[..^1];
                System.Console.Write("\b \b");
            }
        }
        while (key.Key != ConsoleKey.Enter);

        System.Console.WriteLine();
        return password;
    }
}