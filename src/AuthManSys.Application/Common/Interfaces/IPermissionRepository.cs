using AuthManSys.Domain.Entities;
using AuthManSys.Application.Common.Models;

namespace AuthManSys.Application.Common.Interfaces;

public interface IPermissionRepository
{
    // Basic CRUD operations
    Task<Permission?> GetByIdAsync(int permissionId);
    Task<Permission?> GetByNameAsync(string permissionName);
    Task<IEnumerable<Permission>> GetAllAsync();
    Task<IEnumerable<Permission>> GetActiveAsync();
    Task<IEnumerable<Permission>> GetByCategoryAsync(string category);
    Task<Permission> CreateAsync(Permission permission);
    Task<Permission> UpdateAsync(Permission permission);
    Task DeleteAsync(int permissionId);
    Task<bool> ExistsAsync(string permissionName);

    // Permission-Role relationships
    Task<IEnumerable<Permission>> GetPermissionsForRoleAsync(string roleId);
    Task<IEnumerable<Permission>> GetPermissionsForRolesAsync(IEnumerable<string> roleIds);
    Task<IEnumerable<string>> GetRolesWithPermissionAsync(string permissionName);
    Task AssignPermissionToRoleAsync(string roleId, string permissionName);
    Task RemovePermissionFromRoleAsync(string roleId, string permissionName);
    Task<bool> RoleHasPermissionAsync(string roleId, string permissionName);

    // Bulk operations
    Task<BulkOperationResult> BulkAssignPermissionsToRolesAsync(IEnumerable<RolePermissionMapping> mappings);
    Task<BulkOperationResult> BulkRemovePermissionsFromRolesAsync(IEnumerable<RolePermissionMapping> mappings);

    // User permissions (through roles)
    Task<IEnumerable<Permission>> GetUserPermissionsAsync(string userId);
    Task<IEnumerable<string>> GetUserPermissionNamesAsync(string userId);
    Task<bool> UserHasPermissionAsync(string userId, string permissionName);
    Task<Dictionary<string, List<string>>> GetDetailedRolePermissionMappingsAsync();

    // Advanced queries
    Task<IEnumerable<Permission>> SearchPermissionsAsync(string searchTerm);
    Task<IEnumerable<Permission>> GetPaginatedAsync(int pageNumber, int pageSize);
    Task<int> GetTotalCountAsync();
    Task<IEnumerable<string>> GetCategoriesAsync();
    Task<Dictionary<string, int>> GetPermissionCountByCategoryAsync();

    // Cache management
    Task ClearPermissionCacheAsync();
    Task ClearUserPermissionCacheAsync(string userId);
    Task ClearRolePermissionCacheAsync(string roleId);
}