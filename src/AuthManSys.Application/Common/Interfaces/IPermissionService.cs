using AuthManSys.Application.Common.Models;

namespace AuthManSys.Application.Common.Interfaces;

public interface IPermissionService
{
    /// <summary>
    /// Checks if a user has a specific permission through their roles
    /// </summary>
    Task<bool> UserHasPermissionAsync(string userId, string permissionName);

    /// <summary>
    /// Checks if a role has a specific permission
    /// </summary>
    Task<bool> RoleHasPermissionAsync(string roleId, string permissionName);

    /// <summary>
    /// Gets all permissions for a user
    /// </summary>
    Task<IEnumerable<string>> GetUserPermissionsAsync(string userId);

    /// <summary>
    /// Gets all permissions for a role
    /// </summary>
    Task<IEnumerable<string>> GetRolePermissionsAsync(string roleId);

    /// <summary>
    /// Grants a permission to a role
    /// </summary>
    Task GrantPermissionToRoleAsync(string roleId, string permissionName, string? grantedBy = null);

    /// <summary>
    /// Revokes a permission from a role
    /// </summary>
    Task RevokePermissionFromRoleAsync(string roleId, string permissionName);

    /// <summary>
    /// Grants a permission to a role by role name
    /// </summary>
    Task<bool> GrantPermissionToRoleByNameAsync(string roleName, string permissionName, string? grantedBy = null);

    /// <summary>
    /// Revokes a permission from a role by role name
    /// </summary>
    Task<bool> RevokePermissionFromRoleByNameAsync(string roleName, string permissionName);

    /// <summary>
    /// Gets all available permissions
    /// </summary>
    Task<IEnumerable<string>> GetAllPermissionsAsync();

    /// <summary>
    /// Gets all available permissions with detailed information
    /// </summary>
    Task<IEnumerable<PermissionDto>> GetAllPermissionsDetailedAsync();

    /// <summary>
    /// Creates a new permission
    /// </summary>
    Task CreatePermissionAsync(string name, string? description = null, string? category = null);

    /// <summary>
    /// Gets role-permission mappings for admin UI
    /// </summary>
    Task<Dictionary<string, List<string>>> GetRolePermissionMappingsAsync();

    /// <summary>
    /// Gets detailed role-permission mappings for admin UI including all roles
    /// </summary>
    Task<IEnumerable<RolePermissionMappingDto>> GetDetailedRolePermissionMappingsAsync();

    /// <summary>
    /// Grants permissions to roles in bulk using specific mappings
    /// </summary>
    Task<BulkOperationResult> BulkGrantPermissionsAsync(List<RolePermissionMappingRequest> permissions, string? grantedBy = null);

    /// <summary>
    /// Revokes permissions from roles in bulk using specific mappings
    /// </summary>
    Task<BulkOperationResult> BulkRevokePermissionsAsync(List<RolePermissionMappingRequest> permissions);
}