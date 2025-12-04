using AuthManSys.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AuthManSys.Infrastructure.Database.DbContext;

namespace AuthManSys.Infrastructure.Database.Seeder;

public static class PermissionSeeder
{
    public static async Task SeedAsync(
        AuthManSysDbContext context,
        RoleManager<IdentityRole> roleManager,
        ILogger? logger = null)
    {
        try
        {
            // Seed Permissions
            await SeedPermissionsAsync(context, logger);

            // Seed Role-Permission mappings
            await SeedRolePermissionsAsync(context, roleManager, logger);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error occurred seeding permissions");
            throw;
        }
    }

    private static async Task SeedPermissionsAsync(AuthManSysDbContext context, ILogger? logger)
    {
        var permissions = new List<Permission>
        {
            // User Management Permissions
            new() { Name = "ManageUsers", Description = "Create, update, and delete users", Category = "User Management" },
            new() { Name = "ViewUsers", Description = "View user information and lists", Category = "User Management" },
            new() { Name = "CreateUsers", Description = "Create new users", Category = "User Management" },
            new() { Name = "EditUsers", Description = "Modify existing users", Category = "User Management" },
            new() { Name = "DeleteUsers", Description = "Delete users", Category = "User Management" },
            new() { Name = "ViewUserProfile", Description = "View user profile details", Category = "User Management" },
            new() { Name = "EditUserProfile", Description = "Edit user profile", Category = "User Management" },

            // Role Management Permissions
            new() { Name = "ManageRoles", Description = "Create, update, and delete roles", Category = "Role Management" },
            new() { Name = "ViewRoles", Description = "View roles and role assignments", Category = "Role Management" },
            new() { Name = "AssignRoles", Description = "Assign roles to users", Category = "Role Management" },

            // Permission Management Permissions
            new() { Name = "ManagePermissions", Description = "Manage permission assignments", Category = "Permission Management" },
            new() { Name = "ViewPermissions", Description = "View permission assignments", Category = "Permission Management" },
            new() { Name = "GrantPermissions", Description = "Grant permissions to roles", Category = "Permission Management" },
            new() { Name = "RevokePermissions", Description = "Revoke permissions from roles", Category = "Permission Management" },

            // System Administration
            new() { Name = "AccessAdminPanel", Description = "Access administrative interface", Category = "System Administration" },
            new() { Name = "ViewSystemLogs", Description = "View system logs and audit trails", Category = "System Administration" },
            new() { Name = "ManageSystemSettings", Description = "Modify system configuration", Category = "System Administration" },

            // API Access
            new() { Name = "AccessApiDocumentation", Description = "Access API documentation", Category = "API Access" },
            new() { Name = "UsePublicApi", Description = "Use public API endpoints", Category = "API Access" },
            new() { Name = "UsePrivateApi", Description = "Use private/admin API endpoints", Category = "API Access" },

            // Data Access
            new() { Name = "ViewReports", Description = "View reports and analytics", Category = "Data Access" },
            new() { Name = "ExportData", Description = "Export data to files", Category = "Data Access" },
            new() { Name = "ImportData", Description = "Import data from files", Category = "Data Access" },

            // Security
            new() { Name = "ViewAuditLogs", Description = "View audit logs and security events", Category = "Security" },
            new() { Name = "ManageAuthentication", Description = "Manage authentication settings", Category = "Security" },
            new() { Name = "ViewSessions", Description = "View active user sessions", Category = "Security" },
            new() { Name = "TerminateSessions", Description = "Terminate user sessions", Category = "Security" }
        };

        foreach (var permission in permissions)
        {
            var existingPermission = await context.Permissions
                .FirstOrDefaultAsync(p => p.Name == permission.Name);

            if (existingPermission == null)
            {
                context.Permissions.Add(permission);
                logger?.LogInformation("Added permission: {PermissionName}", permission.Name);
            }
            else if (!existingPermission.IsActive)
            {
                existingPermission.IsActive = true;
                existingPermission.Description = permission.Description;
                existingPermission.Category = permission.Category;
                logger?.LogInformation("Reactivated permission: {PermissionName}", permission.Name);
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedRolePermissionsAsync(
        AuthManSysDbContext context,
        RoleManager<IdentityRole> roleManager,
        ILogger? logger)
    {
        // Define role-permission mappings
        var rolePermissions = new Dictionary<string, List<string>>
        {
            ["Administrator"] = new()
            {
                "ManageUsers", "ViewUsers", "CreateUsers", "EditUsers", "DeleteUsers",
                "ManageRoles", "ViewRoles", "AssignRoles",
                "ManagePermissions", "ViewPermissions", "GrantPermissions", "RevokePermissions",
                "AccessAdminPanel", "ViewSystemLogs", "ManageSystemSettings",
                "AccessApiDocumentation", "UsePublicApi", "UsePrivateApi",
                "ViewReports", "ExportData", "ImportData",
                "ViewAuditLogs", "ManageAuthentication", "ViewSessions", "TerminateSessions"
            },
            ["Manager"] = new()
            {
                "ViewUsers", "CreateUsers", "EditUsers",
                "ViewRoles", "AssignRoles",
                "ViewPermissions",
                "AccessApiDocumentation", "UsePublicApi",
                "ViewReports", "ExportData",
                "ViewAuditLogs", "ViewSessions"
            },
            ["User"] = new()
            {
                "ViewUserProfile", "EditUserProfile",
                "UsePublicApi",
                "ViewReports"
            },
            ["ReadOnly"] = new()
            {
                "ViewUsers", "ViewRoles", "ViewPermissions",
                "UsePublicApi", "ViewReports"
            }
        };

        foreach (var (roleName, permissionNames) in rolePermissions)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                logger?.LogWarning("Role {RoleName} not found, skipping permission assignment", roleName);
                continue;
            }

            foreach (var permissionName in permissionNames)
            {
                var permission = await context.Permissions
                    .FirstOrDefaultAsync(p => p.Name == permissionName && p.IsActive);

                if (permission == null)
                {
                    logger?.LogWarning("Permission {PermissionName} not found, skipping", permissionName);
                    continue;
                }

                var existingRolePermission = await context.RolePermissions
                    .FirstOrDefaultAsync(rp => rp.RoleId == role.Id && rp.PermissionId == permission.Id);

                if (existingRolePermission == null)
                {
                    var rolePermission = new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = permission.Id,
                        GrantedAt = DateTime.UtcNow,
                        GrantedBy = "System"
                    };

                    context.RolePermissions.Add(rolePermission);
                    logger?.LogInformation("Granted permission {PermissionName} to role {RoleName}",
                        permissionName, roleName);
                }
            }
        }

        await context.SaveChangesAsync();
        logger?.LogInformation("Permission seeding completed");
    }
}