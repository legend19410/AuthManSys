using AuthManSys.Application.Common.Interfaces;
using AuthManSys.Application.Common.Models;
using AuthManSys.Domain.Entities;
using AuthManSys.Infrastructure.Database.EFCore.DbContext;
using AuthManSys.Infrastructure.Database.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using AutoMapper;

namespace AuthManSys.Infrastructure.Database.EFCore.Repositories;

public class PermissionRepository : IPermissionRepository
{
    private readonly AuthManSysDbContext _context;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IMemoryCache _cache;
    private readonly IPermissionCacheManager _cacheManager;
    private readonly ILogger<PermissionRepository> _logger;
    private readonly IMapper _mapper;

    // Cache key constants
    private const string AllPermissionsCacheKey = "all_permissions";
    private const string UserPermissionsCacheKeyPattern = "user_permissions_";
    private const string RolePermissionsCacheKeyPattern = "role_permissions_";
    private const string DetailedRolePermissionMappingsCacheKey = "detailed_role_permission_mappings";

    public PermissionRepository(
        AuthManSysDbContext context,
        RoleManager<IdentityRole> roleManager,
        IMemoryCache cache,
        IPermissionCacheManager cacheManager,
        ILogger<PermissionRepository> logger,
        IMapper mapper)
    {
        _context = context;
        _roleManager = roleManager;
        _cache = cache;
        _cacheManager = cacheManager;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<AuthManSys.Domain.Entities.Permission?> GetByIdAsync(int permissionId)
    {
        var efPermission = await _context.Permissions.FindAsync(permissionId);
        return efPermission != null ? _mapper.Map<AuthManSys.Domain.Entities.Permission>(efPermission) : null;
    }

    public async Task<AuthManSys.Domain.Entities.Permission?> GetByNameAsync(string permissionName)
    {
        var efPermission = await _context.Permissions
            .FirstOrDefaultAsync(p => p.Name == permissionName);
        return efPermission != null ? _mapper.Map<AuthManSys.Domain.Entities.Permission>(efPermission) : null;
    }

    public async Task<IEnumerable<AuthManSys.Domain.Entities.Permission>> GetAllAsync()
    {
        var cacheKey = AllPermissionsCacheKey;

        if (_cache.TryGetValue(cacheKey, out IEnumerable<AuthManSys.Domain.Entities.Permission>? cachedPermissions) && cachedPermissions != null)
        {
            return cachedPermissions;
        }

        var efPermissions = await _context.Permissions.ToListAsync();
        var domainPermissions = _mapper.Map<IEnumerable<AuthManSys.Domain.Entities.Permission>>(efPermissions);

        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
            SlidingExpiration = TimeSpan.FromMinutes(5),
            Priority = CacheItemPriority.High
        };

        _cache.Set(cacheKey, domainPermissions, cacheOptions);
        return domainPermissions;
    }

    public async Task<IEnumerable<AuthManSys.Domain.Entities.Permission>> GetActiveAsync()
    {
        var efPermissions = await _context.Permissions
            .Where(p => p.IsActive)
            .ToListAsync();
        return _mapper.Map<IEnumerable<AuthManSys.Domain.Entities.Permission>>(efPermissions);
    }

    public async Task<IEnumerable<AuthManSys.Domain.Entities.Permission>> GetByCategoryAsync(string category)
    {
        var efPermissions = await _context.Permissions
            .Where(p => p.Category == category)
            .ToListAsync();
        return _mapper.Map<IEnumerable<AuthManSys.Domain.Entities.Permission>>(efPermissions);
    }

    public async Task<AuthManSys.Domain.Entities.Permission> CreateAsync(AuthManSys.Domain.Entities.Permission permission)
    {
        var efPermission = _mapper.Map<AuthManSys.Infrastructure.Database.Entities.Permission>(permission);
        _context.Permissions.Add(efPermission);
        await _context.SaveChangesAsync();

        // Clear cache
        _cache.Remove(AllPermissionsCacheKey);

        return _mapper.Map<AuthManSys.Domain.Entities.Permission>(efPermission);
    }

    public async Task<AuthManSys.Domain.Entities.Permission> UpdateAsync(AuthManSys.Domain.Entities.Permission permission)
    {
        var efPermission = _mapper.Map<AuthManSys.Infrastructure.Database.Entities.Permission>(permission);
        _context.Permissions.Update(efPermission);
        await _context.SaveChangesAsync();

        // Clear cache
        _cache.Remove(AllPermissionsCacheKey);

        return _mapper.Map<AuthManSys.Domain.Entities.Permission>(efPermission);
    }

    public async Task DeleteAsync(int permissionId)
    {
        var permission = await _context.Permissions.FindAsync(permissionId);
        if (permission != null)
        {
            _context.Permissions.Remove(permission);
            await _context.SaveChangesAsync();

            // Clear cache
            _cache.Remove(AllPermissionsCacheKey);
        }
    }

    public async Task<bool> ExistsAsync(string permissionName)
    {
        return await _context.Permissions.AnyAsync(p => p.Name == permissionName);
    }

    public async Task<IEnumerable<AuthManSys.Domain.Entities.Permission>> GetPermissionsForRoleAsync(string roleId)
    {
        var cacheKey = $"{RolePermissionsCacheKeyPattern}{roleId}";

        if (_cache.TryGetValue(cacheKey, out IEnumerable<AuthManSys.Domain.Entities.Permission>? cachedPermissions) && cachedPermissions != null)
        {
            return cachedPermissions;
        }

        var efPermissions = await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Include(rp => rp.Permission)
            .Select(rp => rp.Permission)
            .ToListAsync();

        var domainPermissions = _mapper.Map<IEnumerable<Domain.Entities.Permission>>(efPermissions);

        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
            SlidingExpiration = TimeSpan.FromMinutes(5),
            Priority = CacheItemPriority.Normal
        };

        _cache.Set(cacheKey, domainPermissions, cacheOptions);
        return domainPermissions;
    }

    public async Task<IEnumerable<AuthManSys.Domain.Entities.Permission>> GetPermissionsForRolesAsync(IEnumerable<string> roleIds)
    {
        var permissions = new List<AuthManSys.Domain.Entities.Permission>();

        foreach (var roleId in roleIds)
        {
            var rolePermissions = await GetPermissionsForRoleAsync(roleId);
            permissions.AddRange(rolePermissions);
        }

        return permissions.Distinct();
    }

    public async Task<IEnumerable<string>> GetRolesWithPermissionAsync(string permissionName)
    {
        var permission = await _context.Permissions
            .FirstOrDefaultAsync(p => p.Name == permissionName);

        if (permission == null)
            return new List<string>();

        var roleIds = await _context.RolePermissions
            .Where(rp => rp.PermissionId == permission.Id)
            .Select(rp => rp.RoleId)
            .ToListAsync();

        return roleIds;
    }

    public async Task AssignPermissionToRoleAsync(string roleId, string permissionName)
    {
        var permission = await _context.Permissions
            .FirstOrDefaultAsync(p => p.Name == permissionName);

        if (permission == null)
            throw new InvalidOperationException($"Permission '{permissionName}' not found");

        var existingMapping = await _context.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permission.Id);

        if (existingMapping == null)
        {
            var rolePermission = new Entities.RolePermission
            {
                RoleId = roleId,
                PermissionId = permission.Id
            };

            _context.RolePermissions.Add(rolePermission);
            await _context.SaveChangesAsync();

            // Clear cache - get users with this role and clear their caches
            var usersWithRole = await _context.UserRoles
                .Where(ur => ur.RoleId == roleId)
                .Select(ur => ur.UserId)
                .ToListAsync();
            _cacheManager.ClearRoleCache(roleId, usersWithRole);
        }
    }

    public async Task RemovePermissionFromRoleAsync(string roleId, string permissionName)
    {
        var permission = await _context.Permissions
            .FirstOrDefaultAsync(p => p.Name == permissionName);

        if (permission == null)
            return;

        var rolePermission = await _context.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permission.Id);

        if (rolePermission != null)
        {
            _context.RolePermissions.Remove(rolePermission);
            await _context.SaveChangesAsync();

            // Clear cache - get users with this role and clear their caches
            var usersWithRole = await _context.UserRoles
                .Where(ur => ur.RoleId == roleId)
                .Select(ur => ur.UserId)
                .ToListAsync();
            _cacheManager.ClearRoleCache(roleId, usersWithRole);
        }
    }

    public async Task<bool> RoleHasPermissionAsync(string roleId, string permissionName)
    {
        var permission = await _context.Permissions
            .FirstOrDefaultAsync(p => p.Name == permissionName);

        if (permission == null)
            return false;

        return await _context.RolePermissions
            .AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permission.Id);
    }

    public async Task<BulkOperationResult> BulkAssignPermissionsToRolesAsync(IEnumerable<RolePermissionMapping> mappings)
    {
        var result = new BulkOperationResult();
        result.TotalOperations = mappings.Count();

        var roles = await _roleManager.Roles
            .Where(r => mappings.Select(m => m.RoleName).Contains(r.Name!))
            .ToDictionaryAsync(r => r.Name!, r => r.Id);

        var permissions = await _context.Permissions
            .Where(p => mappings.Select(m => m.PermissionName).Contains(p.Name))
            .ToDictionaryAsync(p => p.Name, p => p.Id);

        foreach (var mapping in mappings)
        {
            try
            {
                if (!roles.ContainsKey(mapping.RoleName))
                {
                    result.FailedOperations++;
                    result.ErrorMessages.Add($"Role '{mapping.RoleName}' not found");
                    continue;
                }

                if (!permissions.ContainsKey(mapping.PermissionName))
                {
                    result.FailedOperations++;
                    result.ErrorMessages.Add($"Permission '{mapping.PermissionName}' not found");
                    continue;
                }

                var roleId = roles[mapping.RoleName];
                var permissionId = permissions[mapping.PermissionName];

                var existingMapping = await _context.RolePermissions
                    .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

                if (existingMapping != null)
                {
                    result.SkippedOperations++;
                    result.SkippedDetails.Add($"Role '{mapping.RoleName}' already has permission '{mapping.PermissionName}'");
                    continue;
                }

                var rolePermission = new Entities.RolePermission
                {
                    RoleId = roleId,
                    PermissionId = permissionId
                };

                _context.RolePermissions.Add(rolePermission);
                result.SuccessfulOperations++;
                result.SuccessDetails.Add($"Successfully granted permission '{mapping.PermissionName}' to role '{mapping.RoleName}'");
            }
            catch (Exception ex)
            {
                result.FailedOperations++;
                result.ErrorMessages.Add($"Failed to grant permission '{mapping.PermissionName}' to role '{mapping.RoleName}': {ex.Message}");
            }
        }

        if (result.SuccessfulOperations > 0)
        {
            await _context.SaveChangesAsync();

            // Clear cache for all affected roles
            foreach (var roleName in mappings.Select(m => m.RoleName).Distinct())
            {
                if (roles.TryGetValue(roleName, out var roleId))
                {
                    var usersWithRole = await _context.UserRoles
                        .Where(ur => ur.RoleId == roleId)
                        .Select(ur => ur.UserId)
                        .ToListAsync();
                    _cacheManager.ClearRoleCache(roleId, usersWithRole);
                }
            }
        }

        return result;
    }

    public async Task<BulkOperationResult> BulkRemovePermissionsFromRolesAsync(IEnumerable<RolePermissionMapping> mappings)
    {
        var result = new BulkOperationResult();
        result.TotalOperations = mappings.Count();

        var roles = await _roleManager.Roles
            .Where(r => mappings.Select(m => m.RoleName).Contains(r.Name!))
            .ToDictionaryAsync(r => r.Name!, r => r.Id);

        var permissions = await _context.Permissions
            .Where(p => mappings.Select(m => m.PermissionName).Contains(p.Name))
            .ToDictionaryAsync(p => p.Name, p => p.Id);

        foreach (var mapping in mappings)
        {
            try
            {
                if (!roles.ContainsKey(mapping.RoleName))
                {
                    result.FailedOperations++;
                    result.ErrorMessages.Add($"Role '{mapping.RoleName}' not found");
                    continue;
                }

                if (!permissions.ContainsKey(mapping.PermissionName))
                {
                    result.FailedOperations++;
                    result.ErrorMessages.Add($"Permission '{mapping.PermissionName}' not found");
                    continue;
                }

                var roleId = roles[mapping.RoleName];
                var permissionId = permissions[mapping.PermissionName];

                var existingMapping = await _context.RolePermissions
                    .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

                if (existingMapping == null)
                {
                    result.SkippedOperations++;
                    result.SkippedDetails.Add($"Role '{mapping.RoleName}' does not have permission '{mapping.PermissionName}'");
                    continue;
                }

                _context.RolePermissions.Remove(existingMapping);
                result.SuccessfulOperations++;
                result.SuccessDetails.Add($"Successfully revoked permission '{mapping.PermissionName}' from role '{mapping.RoleName}'");
            }
            catch (Exception ex)
            {
                result.FailedOperations++;
                result.ErrorMessages.Add($"Failed to revoke permission '{mapping.PermissionName}' from role '{mapping.RoleName}': {ex.Message}");
            }
        }

        if (result.SuccessfulOperations > 0)
        {
            await _context.SaveChangesAsync();

            // Clear cache for all affected roles
            foreach (var roleName in mappings.Select(m => m.RoleName).Distinct())
            {
                if (roles.TryGetValue(roleName, out var roleId))
                {
                    var usersWithRole = await _context.UserRoles
                        .Where(ur => ur.RoleId == roleId)
                        .Select(ur => ur.UserId)
                        .ToListAsync();
                    _cacheManager.ClearRoleCache(roleId, usersWithRole);
                }
            }
        }

        return result;
    }

    public async Task<IEnumerable<AuthManSys.Domain.Entities.Permission>> GetUserPermissionsAsync(string userId)
    {
        var cacheKey = $"{UserPermissionsCacheKeyPattern}{userId}";

        if (_cache.TryGetValue(cacheKey, out IEnumerable<AuthManSys.Domain.Entities.Permission>? cachedPermissions) && cachedPermissions != null)
        {
            return cachedPermissions;
        }

        var userRoles = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .ToListAsync();

        var efPermissions = await _context.RolePermissions
            .Where(rp => userRoles.Contains(rp.RoleId))
            .Include(rp => rp.Permission)
            .Select(rp => rp.Permission)
            .Distinct()
            .ToListAsync();

        var domainPermissions = _mapper.Map<IEnumerable<Domain.Entities.Permission>>(efPermissions);

        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
            SlidingExpiration = TimeSpan.FromMinutes(5),
            Priority = CacheItemPriority.Normal
        };

        _cache.Set(cacheKey, domainPermissions, cacheOptions);
        return domainPermissions;
    }

    public async Task<IEnumerable<string>> GetUserPermissionNamesAsync(string userId)
    {
        var permissions = await GetUserPermissionsAsync(userId);
        return permissions.Select(p => p.Name);
    }

    public async Task<bool> UserHasPermissionAsync(string userId, string permissionName)
    {
        var userPermissions = await GetUserPermissionNamesAsync(userId);
        return userPermissions.Contains(permissionName);
    }

    public async Task<Dictionary<string, List<string>>> GetDetailedRolePermissionMappingsAsync()
    {
        var cacheKey = DetailedRolePermissionMappingsCacheKey;

        if (_cache.TryGetValue(cacheKey, out Dictionary<string, List<string>>? cachedMappings) && cachedMappings != null)
        {
            return cachedMappings;
        }

        var mappings = await _context.RolePermissions
            .Include(rp => rp.Role)
            .Include(rp => rp.Permission)
            .GroupBy(rp => rp.Role.Name!)
            .ToDictionaryAsync(
                g => g.Key,
                g => g.Select(rp => rp.Permission.Name).ToList()
            );

        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60),
            SlidingExpiration = TimeSpan.FromMinutes(10),
            Priority = CacheItemPriority.High
        };

        _cache.Set(cacheKey, mappings, cacheOptions);
        return mappings;
    }

    public async Task<IEnumerable<AuthManSys.Domain.Entities.Permission>> SearchPermissionsAsync(string searchTerm)
    {
        var efPermissions = await _context.Permissions
            .Where(p => p.Name.Contains(searchTerm) ||
                       (p.Description != null && p.Description.Contains(searchTerm)) ||
                       (p.Category != null && p.Category.Contains(searchTerm)))
            .ToListAsync();
        return _mapper.Map<IEnumerable<AuthManSys.Domain.Entities.Permission>>(efPermissions);
    }

    public async Task<IEnumerable<AuthManSys.Domain.Entities.Permission>> GetPaginatedAsync(int pageNumber, int pageSize)
    {
        var efPermissions = await _context.Permissions
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return _mapper.Map<IEnumerable<AuthManSys.Domain.Entities.Permission>>(efPermissions);
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _context.Permissions.CountAsync();
    }

    public async Task<IEnumerable<string>> GetCategoriesAsync()
    {
        return await _context.Permissions
            .Where(p => p.Category != null)
            .Select(p => p.Category!)
            .Distinct()
            .ToListAsync();
    }

    public async Task<Dictionary<string, int>> GetPermissionCountByCategoryAsync()
    {
        return await _context.Permissions
            .Where(p => p.Category != null)
            .GroupBy(p => p.Category!)
            .ToDictionaryAsync(g => g.Key, g => g.Count());
    }

    public async Task ClearPermissionCacheAsync()
    {
        _cacheManager.ClearAllPermissionCaches();
        await Task.CompletedTask;
    }

    public async Task ClearUserPermissionCacheAsync(string userId)
    {
        _cacheManager.ClearUserCache(userId);
        await Task.CompletedTask;
    }

    public async Task ClearRolePermissionCacheAsync(string roleId)
    {
        var usersWithRole = await _context.UserRoles
            .Where(ur => ur.RoleId == roleId)
            .Select(ur => ur.UserId)
            .ToListAsync();
        _cacheManager.ClearRoleCache(roleId, usersWithRole);
    }
}