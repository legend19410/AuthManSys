using Microsoft.AspNetCore.Identity;
using AuthManSys.Application.Common.Models;

namespace AuthManSys.Application.Common.Interfaces;

public interface IRoleRepository
{
    // Basic CRUD operations
    Task<IdentityRole?> GetByIdAsync(string roleId);
    Task<IdentityRole?> GetByNameAsync(string roleName);
    Task<IEnumerable<IdentityRole>> GetAllAsync();
    Task<IEnumerable<string>> GetAllRoleNamesAsync();
    Task<IEnumerable<RoleDto>> GetAllRolesWithDetailsAsync();
    Task<IdentityResult> CreateAsync(string roleName, string? description = null);
    Task<IdentityResult> UpdateAsync(IdentityRole role);
    Task<IdentityResult> DeleteAsync(IdentityRole role);

    // Role validation and checks
    Task<bool> RoleExistsAsync(string roleName);
    Task<int> GetRoleCountAsync();

    // User-Role relationships
    Task<IEnumerable<string>> GetUsersInRoleAsync(string roleName);
    Task<int> GetUserCountInRoleAsync(string roleName);
    Task<IEnumerable<string>> GetRolesForUserAsync(string userId);

    // Role search and pagination
    Task<IEnumerable<IdentityRole>> GetPaginatedRolesAsync(int pageNumber, int pageSize);
    Task<IEnumerable<IdentityRole>> SearchRolesAsync(string searchTerm);
}