using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AuthManSys.Domain.Entities;
using AuthManSys.Infrastructure.Database.DbContext;

namespace AuthManSys.Infrastructure.Database.Seeder;

public static class UserSeeder
{
    public static async Task SeedAsync(
        AuthManSysDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger? logger = null)
    {
        try
        {
            // Check if users already exist
            if (await userManager.Users.AnyAsync())
            {
                logger?.LogInformation("Users already exist, skipping user seeding");
                return;
            }

            // Create ASP.NET Identity Users
            var users = new[]
            {
                new { UserName = "admin", Email = "admin@authmansys.com", Password = "Admin123!", FirstName = "System", LastName = "Administrator" },
                new { UserName = "jdoe", Email = "john.doe@authmansys.com", Password = "User123!", FirstName = "John", LastName = "Doe" },
                new { UserName = "jsmith", Email = "jane.smith@authmansys.com", Password = "Manager123!", FirstName = "Jane", LastName = "Smith" },
                new { UserName = "bwilson", Email = "bob.wilson@authmansys.com", Password = "User123!", FirstName = "Bob", LastName = "Wilson" },
                new { UserName = "adavis", Email = "alice.davis@authmansys.com", Password = "ReadOnly123!", FirstName = "Alice", LastName = "Davis" },
                new { UserName = "mjohnson", Email = "mike.johnson@authmansys.com", Password = "User123!", FirstName = "Mike", LastName = "Johnson" }
            };

            // Get next available UserId
            var maxUserId = await context.Users.AnyAsync()
                ? await context.Users.MaxAsync(u => (int?)u.UserId) ?? 0
                : 0;

            var currentUserId = maxUserId;

            foreach (var userData in users)
            {
                currentUserId++; // Increment for each new user

                var user = new ApplicationUser
                {
                    UserName = userData.UserName,
                    Email = userData.Email,
                    EmailConfirmed = true, // Ensure all users have verified emails
                    FirstName = userData.FirstName,
                    LastName = userData.LastName,
                    UserId = currentUserId, // Manually assign UserId
                    EmailConfirmationToken = string.Empty,
                    PasswordResetToken = string.Empty,
                    RequestVerificationToken = string.Empty,
                    TermsConditionsAccepted = true,
                    LastPasswordChangedDate = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(user, userData.Password);

                if (result.Succeeded)
                {
                    logger?.LogInformation("Created user: {UserName} ({Email})", user.UserName, user.Email);
                }
                else
                {
                    logger?.LogError("Failed to create user {UserName}: {Errors}",
                        userData.UserName, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }

            await context.SaveChangesAsync();
            logger?.LogInformation("User seeding completed");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error occurred while seeding users");
            throw;
        }
    }
}