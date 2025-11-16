using AuthManSys.Application.Common.Interfaces;
using AuthManSys.Domain.Entities;
using AuthManSys.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace AuthManSys.Infrastructure.Services;

public class PermissionService : IPermissionService
{
    private readonly AuthManSysDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMemoryCache _cache;
    private readonly ILogger<PermissionService> _logger;
    private const int CacheExpirationMinutes = 30;

    public PermissionService(
        AuthManSysDbContext context,
        UserManager<ApplicationUser> userManager,
        IMemoryCache cache,
        ILogger<PermissionService> logger)
    {
        _context = context;
        _userManager = userManager;
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> UserHasPermissionAsync(string userId, string permissionName)
    {
        var cacheKey = $"user_permissions_{userId}";

        if (!_cache.TryGetValue(cacheKey, out HashSet<string>? userPermissions))
        {
            userPermissions = await GetUserPermissionsCachedAsync(userId);
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(CacheExpirationMinutes));

            _cache.Set(cacheKey, userPermissions, cacheEntryOptions);
        }

        return userPermissions?.Contains(permissionName) ?? false;
    }

    public async Task<bool> RoleHasPermissionAsync(string roleId, string permissionName)
    {
        var cacheKey = $"role_permissions_{roleId}";

        if (!_cache.TryGetValue(cacheKey, out HashSet<string>? rolePermissions))
        {
            rolePermissions = await GetRolePermissionsCachedAsync(roleId);
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(CacheExpirationMinutes));

            _cache.Set(cacheKey, rolePermissions, cacheEntryOptions);
        }

        return rolePermissions?.Contains(permissionName) ?? false;
    }

    public async Task<IEnumerable<string>> GetUserPermissionsAsync(string userId)
    {
        var userPermissions = await GetUserPermissionsCachedAsync(userId);
        return userPermissions.ToList();
    }

    public async Task<IEnumerable<string>> GetRolePermissionsAsync(string roleId)
    {
        var rolePermissions = await GetRolePermissionsCachedAsync(roleId);
        return rolePermissions.ToList();
    }

    public async Task GrantPermissionToRoleAsync(string roleId, string permissionName, string? grantedBy = null)
    {
        var permission = await _context.Permissions
            .FirstOrDefaultAsync(p => p.Name == permissionName && p.IsActive);

        if (permission == null)
        {
            throw new InvalidOperationException($"Permission '{permissionName}' not found or inactive");
        }

        var existingRolePermission = await _context.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permission.Id);

        if (existingRolePermission == null)
        {
            var rolePermission = new RolePermission
            {
                RoleId = roleId,
                PermissionId = permission.Id,
                GrantedAt = DateTime.UtcNow,
                GrantedBy = grantedBy
            };

            _context.RolePermissions.Add(rolePermission);
            await _context.SaveChangesAsync();

            // Clear cache for this role
            ClearRoleCache(roleId);

            _logger.LogInformation("Granted permission '{Permission}' to role '{RoleId}' by '{GrantedBy}'",
                permissionName, roleId, grantedBy ?? "System");
        }
    }

    public async Task RevokePermissionFromRoleAsync(string roleId, string permissionName)
    {
        var rolePermission = await _context.RolePermissions
            .Include(rp => rp.Permission)
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.Permission.Name == permissionName);

        if (rolePermission != null)
        {
            _context.RolePermissions.Remove(rolePermission);
            await _context.SaveChangesAsync();

            // Clear cache for this role
            ClearRoleCache(roleId);

            _logger.LogInformation("Revoked permission '{Permission}' from role '{RoleId}'",
                permissionName, roleId);
        }
    }

    public async Task<IEnumerable<string>> GetAllPermissionsAsync()
    {
        var cacheKey = "all_permissions";

        if (!_cache.TryGetValue(cacheKey, out List<string>? allPermissions))
        {
            allPermissions = await _context.Permissions
                .Where(p => p.IsActive)
                .Select(p => p.Name)
                .ToListAsync();

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(1)); // Cache longer since permissions change less frequently

            _cache.Set(cacheKey, allPermissions, cacheEntryOptions);
        }

        return allPermissions ?? new List<string>();
    }

    public async Task CreatePermissionAsync(string name, string? description = null, string? category = null)
    {
        var existingPermission = await _context.Permissions
            .FirstOrDefaultAsync(p => p.Name == name);

        if (existingPermission != null)
        {
            if (!existingPermission.IsActive)
            {
                existingPermission.IsActive = true;
                existingPermission.Description = description;
                existingPermission.Category = category;
                await _context.SaveChangesAsync();
            }
            return;
        }

        var permission = new Permission
        {
            Name = name,
            Description = description,
            Category = category,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Permissions.Add(permission);
        await _context.SaveChangesAsync();

        // Clear all permissions cache
        _cache.Remove("all_permissions");

        _logger.LogInformation("Created new permission '{Permission}' in category '{Category}'",
            name, category ?? "Default");
    }

    public async Task<Dictionary<string, List<string>>> GetRolePermissionMappingsAsync()
    {
        var mappings = await _context.RolePermissions
            .Include(rp => rp.Role)
            .Include(rp => rp.Permission)
            .Where(rp => rp.Permission.IsActive)
            .GroupBy(rp => rp.Role.Name!)
            .ToDictionaryAsync(
                group => group.Key,
                group => group.Select(rp => rp.Permission.Name).ToList()
            );

        return mappings;
    }

    private async Task<HashSet<string>> GetUserPermissionsCachedAsync(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found", userId);
                return new HashSet<string>();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            if (!userRoles.Any())
            {
                return new HashSet<string>();
            }

            var permissions = await _context.RolePermissions
                .Include(rp => rp.Role)
                .Include(rp => rp.Permission)
                .Where(rp => userRoles.Contains(rp.Role.Name!) && rp.Permission.IsActive)
                .Select(rp => rp.Permission.Name)
                .Distinct()
                .ToListAsync();

            return new HashSet<string>(permissions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting permissions for user {UserId}", userId);
            return new HashSet<string>();
        }
    }

    private async Task<HashSet<string>> GetRolePermissionsCachedAsync(string roleId)
    {
        try
        {
            var permissions = await _context.RolePermissions
                .Include(rp => rp.Permission)
                .Where(rp => rp.RoleId == roleId && rp.Permission.IsActive)
                .Select(rp => rp.Permission.Name)
                .ToListAsync();

            return new HashSet<string>(permissions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting permissions for role {RoleId}", roleId);
            return new HashSet<string>();
        }
    }

    private void ClearRoleCache(string roleId)
    {
        _cache.Remove($"role_permissions_{roleId}");

        // Also clear user caches for users with this role
        // Note: In a production system, you might want a more sophisticated cache invalidation strategy
    }
}