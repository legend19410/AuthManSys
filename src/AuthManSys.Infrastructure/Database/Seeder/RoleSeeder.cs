using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using AuthManSys.Infrastructure.Database.DbContext;

namespace AuthManSys.Infrastructure.Database.Seeder;

public static class RoleSeeder
{
    public static async Task SeedAsync(
        AuthManSysDbContext context,
        RoleManager<IdentityRole> roleManager,
        ILogger? logger = null)
    {
        try
        {
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
                    var role = new IdentityRole(roleInfo.Key);
                    var result = await roleManager.CreateAsync(role);

                    if (result.Succeeded)
                    {
                        logger?.LogInformation("Created role: {RoleName}", roleInfo.Key);
                    }
                    else
                    {
                        logger?.LogError("Failed to create role {RoleName}: {Errors}",
                            roleInfo.Key, string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
                else
                {
                    logger?.LogInformation("Role {RoleName} already exists", roleInfo.Key);
                }
            }

            await context.SaveChangesAsync();
            logger?.LogInformation("Role seeding completed");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error occurred while seeding roles");
            throw;
        }
    }
}