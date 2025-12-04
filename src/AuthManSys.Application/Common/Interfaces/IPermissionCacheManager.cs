namespace AuthManSys.Application.Common.Interfaces;

public interface IPermissionCacheManager
{
    /// <summary>
    /// Clears all permission-related cache entries for a specific role
    /// </summary>
    Task ClearRoleCacheAsync(string roleId);

    /// <summary>
    /// Clears all permission-related cache entries for a specific user
    /// </summary>
    void ClearUserCache(string userId);

    /// <summary>
    /// Clears all permission-related cache entries for users who have the specified role
    /// </summary>
    Task ClearUserCachesByRoleAsync(string roleId);

    /// <summary>
    /// Clears all permission-related cache entries
    /// </summary>
    void ClearAllPermissionCaches();

    /// <summary>
    /// Registers a role-user relationship for cache tracking
    /// </summary>
    void RegisterRoleUserRelationship(string roleId, string userId);

    /// <summary>
    /// Removes a role-user relationship from cache tracking
    /// </summary>
    void UnregisterRoleUserRelationship(string roleId, string userId);

    /// <summary>
    /// Clears detailed role permission mappings cache
    /// </summary>
    void ClearDetailedRolePermissionMappingsCache();
}