using AuthManSys.Application.Common.Interfaces;
using AuthManSys.Application.Common.Models;
using AuthManSys.Domain.Entities;
using AuthManSys.Infrastructure.Database.DbContext;
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
    private readonly IPermissionCacheManager _cacheManager;
    private readonly ILogger<PermissionService> _logger;
    private const int CacheExpirationMinutes = 30;

    public PermissionService(
        AuthManSysDbContext context,
        UserManager<ApplicationUser> userManager,
        IMemoryCache cache,
        IPermissionCacheManager cacheManager,
        ILogger<PermissionService> logger)
    {
        _context = context;
        _userManager = userManager;
        _cache = cache;
        _cacheManager = cacheManager;
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
            await _cacheManager.ClearRoleCacheAsync(roleId);

            _logger.LogInformation("Granted permission '{Permission}' to role '{RoleId}' by '{GrantedBy}'",
                permissionName, roleId, grantedBy ?? "System");
        }
        else
        {
            _logger.LogWarning("Permission '{Permission}' already granted to role '{RoleId}' - no action taken",
                permissionName, roleId);
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
            await _cacheManager.ClearRoleCacheAsync(roleId);

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
        _cacheManager.ClearDetailedRolePermissionMappingsCache();

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

    // Removed old ClearRoleCache method - now using sophisticated IPermissionCacheManager

    public async Task<IEnumerable<PermissionDto>> GetAllPermissionsDetailedAsync()
    {
        var cacheKey = "all_permissions_detailed";

        if (!_cache.TryGetValue(cacheKey, out List<PermissionDto>? allPermissions))
        {
            allPermissions = await _context.Permissions
                .Where(p => p.IsActive)
                .OrderBy(p => p.Category)
                .ThenBy(p => p.Name)
                .Select(p => new PermissionDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Category = p.Category,
                    IsActive = p.IsActive,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(1)); // Cache longer since permissions change less frequently

            _cache.Set(cacheKey, allPermissions, cacheEntryOptions);
        }

        return allPermissions ?? new List<PermissionDto>();
    }

    public async Task<IEnumerable<RolePermissionMappingDto>> GetDetailedRolePermissionMappingsAsync()
    {
        try
        {
            var cacheKey = "detailed_role_permission_mappings";

            if (!_cache.TryGetValue(cacheKey, out List<RolePermissionMappingDto>? mappings))
            {
                // Get all roles from the database
                var allRoles = await _context.Roles
                    .Select(r => new
                    {
                        r.Id,
                        r.Name,
                        Description = EF.Property<string>(r, "Description")
                    })
                    .ToListAsync();

                mappings = new List<RolePermissionMappingDto>();

                foreach (var role in allRoles)
                {
                    // Get permissions for this role
                    var rolePermissions = await _context.RolePermissions
                        .Include(rp => rp.Permission)
                        .Where(rp => rp.RoleId == role.Id && rp.Permission.IsActive)
                        .Select(rp => new PermissionDto
                        {
                            Id = rp.Permission.Id,
                            Name = rp.Permission.Name,
                            Description = rp.Permission.Description,
                            Category = rp.Permission.Category,
                            IsActive = rp.Permission.IsActive,
                            CreatedAt = rp.Permission.CreatedAt
                        })
                        .OrderBy(p => p.Category)
                        .ThenBy(p => p.Name)
                        .ToListAsync();

                    mappings.Add(new RolePermissionMappingDto
                    {
                        RoleId = role.Id,
                        RoleName = role.Name ?? "Unknown",
                        RoleDescription = role.Description,
                        Permissions = rolePermissions
                    });
                }

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(CacheExpirationMinutes));

                _cache.Set(cacheKey, mappings, cacheEntryOptions);
            }

            return mappings ?? new List<RolePermissionMappingDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting detailed role permission mappings");
            return new List<RolePermissionMappingDto>();
        }
    }

    public async Task<bool> GrantPermissionToRoleByNameAsync(string roleName, string permissionName, string? grantedBy = null)
    {
        // Find role by name
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
        if (role == null)
        {
            throw new InvalidOperationException($"Role '{roleName}' not found");
        }

        // Use enhanced method with roleId
        return await GrantPermissionToRoleWithResultAsync(role.Id, permissionName, grantedBy);
    }

    public async Task<bool> RevokePermissionFromRoleByNameAsync(string roleName, string permissionName)
    {
        // Find role by name
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
        if (role == null)
        {
            throw new InvalidOperationException($"Role '{roleName}' not found");
        }

        // Use enhanced method with roleId
        return await RevokePermissionFromRoleWithResultAsync(role.Id, permissionName);
    }

    private async Task<bool> GrantPermissionToRoleWithResultAsync(string roleId, string permissionName, string? grantedBy = null)
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
            await _cacheManager.ClearRoleCacheAsync(roleId);

            _logger.LogInformation("Granted permission '{Permission}' to role '{RoleId}' by '{GrantedBy}'",
                permissionName, roleId, grantedBy ?? "System");

            return true; // Permission was newly granted
        }
        else
        {
            _logger.LogWarning("Permission '{Permission}' already granted to role '{RoleId}' - no action taken",
                permissionName, roleId);

            return false; // Permission already existed
        }
    }

    private async Task<bool> RevokePermissionFromRoleWithResultAsync(string roleId, string permissionName)
    {
        var rolePermission = await _context.RolePermissions
            .Include(rp => rp.Permission)
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.Permission.Name == permissionName);

        if (rolePermission != null)
        {
            _context.RolePermissions.Remove(rolePermission);
            await _context.SaveChangesAsync();

            // Clear cache for this role
            await _cacheManager.ClearRoleCacheAsync(roleId);

            _logger.LogInformation("Revoked permission '{Permission}' from role '{RoleId}'",
                permissionName, roleId);

            return true; // Permission was revoked
        }

        _logger.LogWarning("Permission '{Permission}' was not assigned to role '{RoleId}' - no action taken",
            permissionName, roleId);

        return false; // Permission was not assigned
    }

    public async Task<BulkOperationResult> BulkGrantPermissionsAsync(List<RolePermissionMappingRequest> permissions, string? grantedBy = null)
    {
        var result = new BulkOperationResult();
        result.TotalOperations = permissions.Count;

        try
        {
            // Validate input
            if (!permissions.Any())
            {
                result.ErrorMessages.Add("Permissions list cannot be empty");
                result.FailedOperations = 0;
                return result;
            }

            // Get unique role and permission names
            var roleNames = permissions.Select(p => p.RoleName).Distinct().ToList();
            var permissionNames = permissions.Select(p => p.PermissionName).Distinct().ToList();

            // Get all roles and permissions at once
            var roles = await _context.Roles
                .Where(r => roleNames.Contains(r.Name))
                .ToDictionaryAsync(r => r.Name, r => r);

            var permissionsMap = await _context.Permissions
                .Where(p => permissionNames.Contains(p.Name) && p.IsActive)
                .ToDictionaryAsync(p => p.Name, p => p);

            // Get existing role-permission mappings
            var roleIds = roles.Values.Select(r => r.Id).ToList();
            var permissionIds = permissionsMap.Values.Select(p => p.Id).ToList();

            var existingMappings = await _context.RolePermissions
                .Where(rp => roleIds.Contains(rp.RoleId) && permissionIds.Contains(rp.PermissionId))
                .Select(rp => new { rp.RoleId, rp.PermissionId })
                .ToHashSetAsync();

            var rolePermissionsToAdd = new List<RolePermission>();
            var affectedRoleIds = new HashSet<string>();

            // Process each specific permission mapping
            foreach (var permissionMapping in permissions)
            {
                var roleName = permissionMapping.RoleName;
                var permissionName = permissionMapping.PermissionName;

                // Check if role exists
                if (!roles.TryGetValue(roleName, out var role))
                {
                    result.ErrorMessages.Add($"Role '{roleName}' not found");
                    result.FailedOperations++;
                    continue;
                }

                // Check if permission exists
                if (!permissionsMap.TryGetValue(permissionName, out var permission))
                {
                    result.ErrorMessages.Add($"Permission '{permissionName}' not found or inactive");
                    result.FailedOperations++;
                    continue;
                }

                // Check if mapping already exists
                var mapping = new { RoleId = role.Id, PermissionId = permission.Id };
                if (existingMappings.Contains(mapping))
                {
                    result.SkippedOperations++;
                    result.SkippedDetails.Add($"Permission '{permissionName}' already assigned to role '{roleName}'");
                    continue;
                }

                // Add to batch for insertion
                rolePermissionsToAdd.Add(new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = permission.Id,
                    GrantedAt = DateTime.UtcNow,
                    GrantedBy = grantedBy
                });

                affectedRoleIds.Add(role.Id);
                result.SuccessfulOperations++;
                result.SuccessDetails.Add($"Granted permission '{permissionName}' to role '{roleName}'");
            }

            // Add new role-permission mappings
            if (rolePermissionsToAdd.Any())
            {
                _context.RolePermissions.AddRange(rolePermissionsToAdd);
                await _context.SaveChangesAsync();

                // Clear cache for affected roles
                foreach (var roleId in affectedRoleIds)
                {
                    await _cacheManager.ClearRoleCacheAsync(roleId);
                }

                _logger.LogInformation(
                    "Bulk granted {Count} permissions across {RoleCount} roles by '{GrantedBy}'",
                    rolePermissionsToAdd.Count, affectedRoleIds.Count, grantedBy ?? "System");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during bulk permission grant operation");
            result.ErrorMessages.Add($"Unexpected error: {ex.Message}");
            result.FailedOperations = result.TotalOperations - result.SuccessfulOperations - result.SkippedOperations;
            return result;
        }
    }

    public async Task<BulkOperationResult> BulkRevokePermissionsAsync(List<RolePermissionMappingRequest> permissions)
    {
        var result = new BulkOperationResult();
        result.TotalOperations = permissions.Count;

        try
        {
            // Validate input
            if (!permissions.Any())
            {
                result.ErrorMessages.Add("Permissions list cannot be empty");
                result.FailedOperations = 0;
                return result;
            }

            // Get unique role and permission names
            var roleNames = permissions.Select(p => p.RoleName).Distinct().ToList();
            var permissionNames = permissions.Select(p => p.PermissionName).Distinct().ToList();

            // Get all roles at once
            var roles = await _context.Roles
                .Where(r => roleNames.Contains(r.Name))
                .ToDictionaryAsync(r => r.Name, r => r);

            // Get existing role-permission mappings that match our specific criteria
            var roleIds = roles.Values.Select(r => r.Id).ToList();

            var existingMappings = await _context.RolePermissions
                .Include(rp => rp.Permission)
                .Include(rp => rp.Role)
                .Where(rp => roleIds.Contains(rp.RoleId) && permissionNames.Contains(rp.Permission.Name))
                .ToListAsync();

            var mappingsToRemove = new List<RolePermission>();
            var affectedRoleIds = new HashSet<string>();

            // Process each specific permission mapping
            foreach (var permissionMapping in permissions)
            {
                var roleName = permissionMapping.RoleName;
                var permissionName = permissionMapping.PermissionName;

                // Check if role exists
                if (!roles.TryGetValue(roleName, out var role))
                {
                    result.ErrorMessages.Add($"Role '{roleName}' not found");
                    result.FailedOperations++;
                    continue;
                }

                // Find the specific mapping to remove
                var mapping = existingMappings
                    .FirstOrDefault(rp => rp.RoleId == role.Id && rp.Permission.Name == permissionName);

                if (mapping != null)
                {
                    mappingsToRemove.Add(mapping);
                    affectedRoleIds.Add(role.Id);
                    result.SuccessfulOperations++;
                    result.SuccessDetails.Add($"Revoked permission '{permissionName}' from role '{roleName}'");
                }
                else
                {
                    result.SkippedOperations++;
                    result.SkippedDetails.Add($"Permission '{permissionName}' was not assigned to role '{roleName}'");
                }
            }

            // Remove the mappings
            if (mappingsToRemove.Any())
            {
                _context.RolePermissions.RemoveRange(mappingsToRemove);
                await _context.SaveChangesAsync();

                // Clear cache for affected roles
                foreach (var roleId in affectedRoleIds)
                {
                    await _cacheManager.ClearRoleCacheAsync(roleId);
                }

                _logger.LogInformation(
                    "Bulk revoked {Count} permission mappings across {RoleCount} roles",
                    mappingsToRemove.Count, affectedRoleIds.Count);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during bulk permission revoke operation");
            result.ErrorMessages.Add($"Unexpected error: {ex.Message}");
            result.FailedOperations = result.TotalOperations - result.SuccessfulOperations - result.SkippedOperations;
            return result;
        }
    }
}