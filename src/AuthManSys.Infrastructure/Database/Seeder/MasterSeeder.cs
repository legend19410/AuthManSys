using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using AuthManSys.Domain.Entities;
using AuthManSys.Infrastructure.Database.DbContext;

namespace AuthManSys.Infrastructure.Database.Seeder;

public static class MasterSeeder
{
    public static async Task SeedAllAsync(
        AuthManSysDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger? logger = null)
    {
        try
        {
            logger?.LogInformation("Starting database seeding process...");

            // Step 1: Seed Roles (independent entity)
            logger?.LogInformation("Seeding roles...");
            await RoleSeeder.SeedAsync(context, roleManager, logger);

            // Step 2: Seed Users (independent entity)
            logger?.LogInformation("Seeding users...");
            await UserSeeder.SeedAsync(context, userManager, logger);

            // Step 3: Seed Permissions (independent entity)
            logger?.LogInformation("Seeding permissions...");
            await PermissionSeeder.SeedAsync(context, logger);

            // Step 4: Assign Roles to Users (depends on both users and roles)
            logger?.LogInformation("Assigning roles to users...");
            await UserRoleSeeder.SeedAsync(context, userManager, logger);

            // Step 5: Assign Permissions to Roles (depends on both roles and permissions)
            logger?.LogInformation("Assigning permissions to roles...");
            await RolePermissionSeeder.SeedAsync(context, roleManager, logger);

            logger?.LogInformation("Database seeding completed successfully!");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error occurred during database seeding");
            throw;
        }
    }

    public static async Task SeedUsersOnlyAsync(
        AuthManSysDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger? logger = null)
    {
        try
        {
            logger?.LogInformation("Seeding users only...");
            await UserSeeder.SeedAsync(context, userManager, logger);
            logger?.LogInformation("User seeding completed!");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error occurred while seeding users");
            throw;
        }
    }

    public static async Task SeedRolesOnlyAsync(
        AuthManSysDbContext context,
        RoleManager<IdentityRole> roleManager,
        ILogger? logger = null)
    {
        try
        {
            logger?.LogInformation("Seeding roles only...");
            await RoleSeeder.SeedAsync(context, roleManager, logger);
            logger?.LogInformation("Role seeding completed!");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error occurred while seeding roles");
            throw;
        }
    }

    public static async Task SeedPermissionsOnlyAsync(
        AuthManSysDbContext context,
        ILogger? logger = null)
    {
        try
        {
            logger?.LogInformation("Seeding permissions only...");
            await PermissionSeeder.SeedAsync(context, logger);
            logger?.LogInformation("Permission seeding completed!");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error occurred while seeding permissions");
            throw;
        }
    }

    public static async Task SeedUserRolesOnlyAsync(
        AuthManSysDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger? logger = null)
    {
        try
        {
            logger?.LogInformation("Assigning roles to users only...");
            await UserRoleSeeder.SeedAsync(context, userManager, logger);
            logger?.LogInformation("User role assignment completed!");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error occurred while assigning roles to users");
            throw;
        }
    }

    public static async Task SeedRolePermissionsOnlyAsync(
        AuthManSysDbContext context,
        RoleManager<IdentityRole> roleManager,
        ILogger? logger = null)
    {
        try
        {
            logger?.LogInformation("Assigning permissions to roles only...");
            await RolePermissionSeeder.SeedAsync(context, roleManager, logger);
            logger?.LogInformation("Role permission assignment completed!");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error occurred while assigning permissions to roles");
            throw;
        }
    }
}