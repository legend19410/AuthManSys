using MediatR;
using Microsoft.AspNetCore.Identity;
using AuthManSys.Infrastructure.Database.Entities;

namespace AuthManSys.Console.Commands;

public class UserCommands : IUserCommands
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMediator _mediator;

    public UserCommands(UserManager<ApplicationUser> userManager, IMediator mediator)
    {
        _userManager = userManager;
        _mediator = mediator;
    }

    public async Task ListUsersAsync()
    {
        System.Console.WriteLine("📋 Listing all users...");
        System.Console.WriteLine();

        var users = _userManager.Users.ToList();

        if (!users.Any())
        {
            System.Console.WriteLine("No users found.");
            return;
        }

        System.Console.WriteLine($"{"ID",-38} {"Username",-20} {"Email",-30} {"Confirmed",-10}");
        System.Console.WriteLine(new string('─', 100));

        foreach (var user in users)
        {
            System.Console.WriteLine($"{user.Id,-38} {user.UserName,-20} {user.Email,-30} {user.EmailConfirmed,-10}");
        }

        System.Console.WriteLine();
        System.Console.WriteLine($"Total users: {users.Count}");
    }

    public async Task CreateUserAsync()
    {
        System.Console.WriteLine("👤 Create New User");
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
                System.Console.WriteLine($"\n✅ User '{username}' created successfully!");
            }
            else
            {
                System.Console.WriteLine($"\n❌ Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"\n❌ Error creating user: {ex.Message}");
        }
    }

    public async Task DeleteUserAsync()
    {
        System.Console.WriteLine("🗑️ Delete User");
        System.Console.WriteLine("─────────────────────────────────────────────────────────────────");

        System.Console.Write("Enter username or email: ");
        var identifier = System.Console.ReadLine();

        if (string.IsNullOrWhiteSpace(identifier))
        {
            System.Console.WriteLine("❌ Username or email is required.");
            return;
        }

        var user = await _userManager.FindByNameAsync(identifier) ??
                   await _userManager.FindByEmailAsync(identifier);

        if (user == null)
        {
            System.Console.WriteLine($"❌ User '{identifier}' not found.");
            return;
        }

        System.Console.WriteLine($"Found user: {user.UserName} ({user.Email})");
        System.Console.Write("Are you sure you want to delete this user? (y/N): ");
        var confirmation = System.Console.ReadLine();

        if (confirmation?.ToLower() != "y")
        {
            System.Console.WriteLine("❌ Deletion cancelled.");
            return;
        }

        try
        {
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                System.Console.WriteLine($"✅ User '{user.UserName}' deleted successfully!");
            }
            else
            {
                System.Console.WriteLine($"❌ Failed to delete user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"❌ Error deleting user: {ex.Message}");
        }
    }

    public async Task UpdateUserAsync()
    {
        System.Console.WriteLine("✏️ Update User");
        System.Console.WriteLine("─────────────────────────────────────────────────────────────────");

        System.Console.Write("Enter username or email: ");
        var identifier = System.Console.ReadLine();

        if (string.IsNullOrWhiteSpace(identifier))
        {
            System.Console.WriteLine("❌ Username or email is required.");
            return;
        }

        var user = await _userManager.FindByNameAsync(identifier) ??
                   await _userManager.FindByEmailAsync(identifier);

        if (user == null)
        {
            System.Console.WriteLine($"❌ User '{identifier}' not found.");
            return;
        }

        System.Console.WriteLine($"Current user details:");
        System.Console.WriteLine($"Username: {user.UserName}");
        System.Console.WriteLine($"Email: {user.Email}");
        System.Console.WriteLine($"First Name: {user.FirstName}");
        System.Console.WriteLine($"Last Name: {user.LastName}");
        System.Console.WriteLine();

        System.Console.Write($"Enter new first name (current: {user.FirstName}): ");
        var firstName = System.Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(firstName))
            user.FirstName = firstName;

        System.Console.Write($"Enter new last name (current: {user.LastName}): ");
        var lastName = System.Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(lastName))
            user.LastName = lastName;

        try
        {
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                System.Console.WriteLine($"✅ User '{user.UserName}' updated successfully!");
            }
            else
            {
                System.Console.WriteLine($"❌ Failed to update user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"❌ Error updating user: {ex.Message}");
        }
    }

    public async Task FindUserAsync()
    {
        System.Console.WriteLine("🔍 Find User");
        System.Console.WriteLine("─────────────────────────────────────────────────────────────────");

        System.Console.Write("Enter username or email: ");
        var identifier = System.Console.ReadLine();

        if (string.IsNullOrWhiteSpace(identifier))
        {
            System.Console.WriteLine("❌ Username or email is required.");
            return;
        }

        var user = await _userManager.FindByNameAsync(identifier) ??
                   await _userManager.FindByEmailAsync(identifier);

        if (user == null)
        {
            System.Console.WriteLine($"❌ User '{identifier}' not found.");
            return;
        }

        System.Console.WriteLine($"✅ User found:");
        System.Console.WriteLine($"ID: {user.Id}");
        System.Console.WriteLine($"Username: {user.UserName}");
        System.Console.WriteLine($"Email: {user.Email}");
        System.Console.WriteLine($"First Name: {user.FirstName}");
        System.Console.WriteLine($"Last Name: {user.LastName}");
        System.Console.WriteLine($"Email Confirmed: {user.EmailConfirmed}");
        System.Console.WriteLine($"Created: {DateTime.Now}"); // CreatedAt not available on ApplicationUser

        var roles = await _userManager.GetRolesAsync(user);
        System.Console.WriteLine($"Roles: {(roles.Any() ? string.Join(", ", roles) : "None")}");
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