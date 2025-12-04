using AuthManSys.Application.Common.Interfaces;
using AuthManSys.Infrastructure.Database.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace AuthManSys.Infrastructure.Services;

public class PermissionCacheManager : IPermissionCacheManager
{
    private readonly IMemoryCache _cache;
    private readonly AuthManSysDbContext _context;
    private readonly ILogger<PermissionCacheManager> _logger;

    // Thread-safe collections to track cache relationships
    private static readonly ConcurrentDictionary<string, HashSet<string>> _roleToUsersMap = new();
    private static readonly ConcurrentDictionary<string, HashSet<string>> _userToRolesMap = new();
    private static readonly object _lockObject = new object();

    // Cache key patterns for easy management
    private const string UserPermissionsCacheKeyPattern = "user_permissions_";
    private const string RolePermissionsCacheKeyPattern = "role_permissions_";
    private const string AllPermissionsCacheKey = "all_permissions";
    private const string AllPermissionsDetailedCacheKey = "all_permissions_detailed";
    private const string DetailedRolePermissionMappingsCacheKey = "detailed_role_permission_mappings";

    public PermissionCacheManager(
        IMemoryCache cache,
        AuthManSysDbContext context,
        ILogger<PermissionCacheManager> logger)
    {
        _cache = cache;
        _context = context;
        _logger = logger;
    }

    public async Task ClearRoleCacheAsync(string roleId)
    {
        try
        {
            // Clear the role's permission cache
            var rolePermissionsCacheKey = $"{RolePermissionsCacheKeyPattern}{roleId}";
            _cache.Remove(rolePermissionsCacheKey);

            // Clear all users who have this role
            await ClearUserCachesByRoleAsync(roleId);

            // Clear global permission caches that might be affected
            ClearDetailedRolePermissionMappingsCache();

            _logger.LogDebug("Cleared cache for role {RoleId} and all associated users", roleId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cache for role {RoleId}", roleId);
        }
    }

    public void ClearUserCache(string userId)
    {
        try
        {
            var userPermissionsCacheKey = $"{UserPermissionsCacheKeyPattern}{userId}";
            _cache.Remove(userPermissionsCacheKey);

            _logger.LogDebug("Cleared cache for user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cache for user {UserId}", userId);
        }
    }

    public async Task ClearUserCachesByRoleAsync(string roleId)
    {
        try
        {
            // Get all users who have this role from database
            var usersWithRole = await _context.UserRoles
                .Where(ur => ur.RoleId == roleId)
                .Select(ur => ur.UserId)
                .ToListAsync();

            // Clear cache for each user
            foreach (var userId in usersWithRole)
            {
                ClearUserCache(userId);
            }

            // Also check our in-memory tracking (fallback in case DB query fails)
            lock (_lockObject)
            {
                if (_roleToUsersMap.TryGetValue(roleId, out var trackedUsers))
                {
                    foreach (var userId in trackedUsers)
                    {
                        ClearUserCache(userId);
                    }
                }
            }

            _logger.LogDebug("Cleared user caches for {UserCount} users with role {RoleId}", usersWithRole.Count, roleId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing user caches for role {RoleId}", roleId);
        }
    }

    public void ClearAllPermissionCaches()
    {
        try
        {
            // Get all cache keys that start with our known patterns
            var cacheKeysToRemove = new List<string>();

            // Unfortunately, IMemoryCache doesn't provide a way to enumerate keys
            // So we'll use a more aggressive approach and clear known cache entries

            // Clear global permission caches
            _cache.Remove(AllPermissionsCacheKey);
            _cache.Remove(AllPermissionsDetailedCacheKey);
            _cache.Remove(DetailedRolePermissionMappingsCacheKey);

            // Clear tracked user and role caches
            lock (_lockObject)
            {
                foreach (var userRole in _userToRolesMap.Keys)
                {
                    ClearUserCache(userRole);
                }

                foreach (var roleId in _roleToUsersMap.Keys)
                {
                    _cache.Remove($"{RolePermissionsCacheKeyPattern}{roleId}");
                }

                // Clear the tracking maps
                _roleToUsersMap.Clear();
                _userToRolesMap.Clear();
            }

            _logger.LogInformation("Cleared all permission-related caches");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing all permission caches");
        }
    }

    public void RegisterRoleUserRelationship(string roleId, string userId)
    {
        try
        {
            lock (_lockObject)
            {
                // Track role -> users mapping
                if (!_roleToUsersMap.TryGetValue(roleId, out var users))
                {
                    users = new HashSet<string>();
                    _roleToUsersMap[roleId] = users;
                }
                users.Add(userId);

                // Track user -> roles mapping
                if (!_userToRolesMap.TryGetValue(userId, out var roles))
                {
                    roles = new HashSet<string>();
                    _userToRolesMap[userId] = roles;
                }
                roles.Add(roleId);
            }

            _logger.LogTrace("Registered relationship: Role {RoleId} -> User {UserId}", roleId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering role-user relationship: {RoleId} -> {UserId}", roleId, userId);
        }
    }

    public void UnregisterRoleUserRelationship(string roleId, string userId)
    {
        try
        {
            lock (_lockObject)
            {
                // Remove from role -> users mapping
                if (_roleToUsersMap.TryGetValue(roleId, out var users))
                {
                    users.Remove(userId);
                    if (users.Count == 0)
                    {
                        _roleToUsersMap.TryRemove(roleId, out _);
                    }
                }

                // Remove from user -> roles mapping
                if (_userToRolesMap.TryGetValue(userId, out var roles))
                {
                    roles.Remove(roleId);
                    if (roles.Count == 0)
                    {
                        _userToRolesMap.TryRemove(userId, out _);
                    }
                }
            }

            // Clear the user's cache since their roles changed
            ClearUserCache(userId);

            _logger.LogTrace("Unregistered relationship: Role {RoleId} -> User {UserId}", roleId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unregistering role-user relationship: {RoleId} -> {UserId}", roleId, userId);
        }
    }

    public void ClearDetailedRolePermissionMappingsCache()
    {
        try
        {
            _cache.Remove(DetailedRolePermissionMappingsCacheKey);
            _logger.LogDebug("Cleared detailed role permission mappings cache");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing detailed role permission mappings cache");
        }
    }
}