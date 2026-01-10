using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AuthManSys.Application.Common.Helpers;
using AuthManSys.Infrastructure.Database.EFCore.DbContext;
using AuthManSys.Infrastructure.Database.Entities;

namespace AuthManSys.Infrastructure.Database.EFCore.Seeder;

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

        // Seed ASP.NET Identity Roles with descriptions
        var identityRoles = new Dictionary<string, string>
        {
            { "Administrator", "Full system access with all administrative privileges including user management, system configuration, and security settings" },
            { "Manager", "Management-level access with permissions to oversee operations, manage users, and access reports" },
            { "User", "Standard user access with basic functionality for daily operations and personal account management" },
            { "ReadOnly", "Read-only access with view permissions but no ability to modify data or system settings" }
        };

        foreach (var roleInfo in identityRoles)
        {
            if (!await roleManager.RoleExistsAsync(roleInfo.Key))
            {
                await roleManager.CreateAsync(new IdentityRole(roleInfo.Key));
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
                EmailConfirmationToken = null,
                PasswordResetToken = null,
                RequestVerificationToken = null,
                TermsConditionsAccepted = true,
                LastPasswordChangedDate = DateTime.UtcNow
                // UserId will be auto-assigned by database
            };

            var result = await userManager.CreateAsync(user, userData.Password);

            if (result.Succeeded)
            {
                // Add user to roles
                foreach (var roleName in userData.Roles)
                {
                    await userManager.AddToRoleAsync(user, roleName);
                }
            }
        }

        // Save changes to the database
        await context.SaveChangesAsync();

        // Seed permissions and role-permission mappings
        await PermissionSeeder.SeedAsync(context);
    }

}