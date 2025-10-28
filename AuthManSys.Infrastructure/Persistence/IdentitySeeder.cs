using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AuthManSys.Domain.Entities;

namespace AuthManSys.Infrastructure.Persistence;

public static class IdentitySeeder
{
    public static async Task SeedAsync(
        AuthManSysDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        // Check if ASP.NET Identity users already exist
        if (await userManager.Users.AnyAsync())
        {
            return; // Already seeded
        }

        // Seed ASP.NET Identity Roles
        var identityRoles = new[]
        {
            "Administrator",
            "Manager",
            "User",
            "ReadOnly"
        };

        foreach (var roleName in identityRoles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // Create ASP.NET Identity Users
        var users = new[]
        {
            new { UserName = "admin", Email = "admin@authmansys.com", Password = "Admin123!", FirstName = "System", LastName = "Administrator", Roles = new[] { "Administrator" } },
            new { UserName = "jdoe", Email = "john.doe@authmansys.com", Password = "User123!", FirstName = "John", LastName = "Doe", Roles = new[] { "User" } },
            new { UserName = "jsmith", Email = "jane.smith@authmansys.com", Password = "Manager123!", FirstName = "Jane", LastName = "Smith", Roles = new[] { "Manager" } },
            new { UserName = "bwilson", Email = "bob.wilson@authmansys.com", Password = "User123!", FirstName = "Bob", LastName = "Wilson", Roles = new[] { "User" } },
            new { UserName = "adavis", Email = "alice.davis@authmansys.com", Password = "ReadOnly123!", FirstName = "Alice", LastName = "Davis", Roles = new[] { "ReadOnly" } },
            new { UserName = "mjohnson", Email = "mike.johnson@authmansys.com", Password = "User123!", FirstName = "Mike", LastName = "Johnson", Roles = new[] { "User" } }
        };

        foreach (var userData in users)
        {
            var user = new ApplicationUser
            {
                UserName = userData.UserName,
                Email = userData.Email,
                EmailConfirmed = true,
                FirstName = userData.FirstName,
                LastName = userData.LastName,
                EmailConfirmationToken = string.Empty,
                PasswordResetToken = string.Empty,
                RequestVerificationToken = string.Empty,
                TermsConditionsAccepted = true,
                LastPasswordChangedDate = DateTime.UtcNow,
                UserId = 0 // Will be set after custom user creation
            };

            var result = await userManager.CreateAsync(user, userData.Password);

            if (result.Succeeded)
            {
                // Add user to roles
                foreach (var role in userData.Roles)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }
        }

        // Save changes to the database
        await context.SaveChangesAsync();}

}