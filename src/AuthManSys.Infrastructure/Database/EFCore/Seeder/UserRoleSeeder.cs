using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using AuthManSys.Infrastructure.Database.EFCore.DbContext;
using AuthManSys.Infrastructure.Database.Entities;

namespace AuthManSys.Infrastructure.Database.EFCore.Seeder;

public static class UserRoleSeeder
{
    public static async Task SeedAsync(
        AuthManSysDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger? logger = null)
    {
        try
        {
            // Define user-role mappings
            var userRoles = new Dictionary<string, List<string>>
            {
                ["admin"] = new() { "Administrator" },
                ["jdoe"] = new() { "User" },
                ["jsmith"] = new() { "Manager" },
                ["bwilson"] = new() { "User" },
                ["adavis"] = new() { "ReadOnly" },
                ["mjohnson"] = new() { "User" }
            };

            foreach (var (username, roleNames) in userRoles)
            {
                var user = await userManager.FindByNameAsync(username);
                if (user == null)
                {
                    logger?.LogWarning("User {Username} not found, skipping role assignment", username);
                    continue;
                }

                foreach (var roleName in roleNames)
                {
                    if (!await userManager.IsInRoleAsync(user, roleName))
                    {
                        var result = await userManager.AddToRoleAsync(user, roleName);
                        if (result.Succeeded)
                        {
                            logger?.LogInformation("Assigned role {RoleName} to user {Username}", roleName, username);
                        }
                        else
                        {
                            logger?.LogError("Failed to assign role {RoleName} to user {Username}: {Errors}",
                                roleName, username, string.Join(", ", result.Errors.Select(e => e.Description)));
                        }
                    }
                    else
                    {
                        logger?.LogInformation("User {Username} already has role {RoleName}", username, roleName);
                    }
                }
            }

            await context.SaveChangesAsync();
            logger?.LogInformation("User role assignment completed");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error occurred while assigning roles to users");
            throw;
        }
    }
}