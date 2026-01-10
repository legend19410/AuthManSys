using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AuthManSys.Infrastructure.Database.EFCore.DbContext;
using AuthManSys.Infrastructure.Database.Entities;

namespace AuthManSys.Infrastructure.Database.EFCore.Seeder;

public static class RolePermissionSeeder
{
    public static async Task SeedAsync(
        AuthManSysDbContext context,
        RoleManager<IdentityRole> roleManager,
        ILogger? logger = null)
    {
        try
        {
            await SeedRolePermissionsAsync(context, roleManager, logger);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error occurred seeding role permissions");
            throw;
        }
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
        logger?.LogInformation("Role permission mapping completed");
    }
}