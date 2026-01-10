using AuthManSys.Application.Common.Interfaces;
using AuthManSys.Application.Common.Models;
using AuthManSys.Domain.Entities;
using AuthManSys.Infrastructure.Database.EFCore.DbContext;
using AuthManSys.Infrastructure.Database.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace AuthManSys.Infrastructure.Database.EFCore.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly AuthManSysDbContext _context;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;

    public RoleRepository(
        AuthManSysDbContext context,
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager,
        IMapper mapper)
    {
        _context = context;
        _roleManager = roleManager;
        _userManager = userManager;
        _mapper = mapper;
    }

    public async Task<IdentityRole?> GetByIdAsync(string roleId)
    {
        return await _roleManager.FindByIdAsync(roleId);
    }

    public async Task<IdentityRole?> GetByNameAsync(string roleName)
    {
        return await _roleManager.FindByNameAsync(roleName);
    }

    public async Task<IEnumerable<IdentityRole>> GetAllAsync()
    {
        return await _roleManager.Roles.ToListAsync();
    }

    public async Task<IEnumerable<string>> GetAllRoleNamesAsync()
    {
        return await _roleManager.Roles.Select(r => r.Name!).ToListAsync();
    }

    public async Task<IEnumerable<RoleDto>> GetAllRolesWithDetailsAsync()
    {
        var roles = await _roleManager.Roles
            .Select(r => new {
                Id = r.Id,
                Name = r.Name!,
                NormalizedName = r.NormalizedName,
                Description = EF.Property<string>(r, "Description")
            })
            .ToListAsync();

        return roles.Select(r => new RoleDto(r.Id, r.Name, r.NormalizedName, r.Description));
    }

    public async Task<IdentityResult> CreateAsync(string roleName, string? description = null)
    {
        var role = new IdentityRole(roleName);
        return await _roleManager.CreateAsync(role);
    }

    public async Task<IdentityResult> UpdateAsync(IdentityRole role)
    {
        return await _roleManager.UpdateAsync(role);
    }

    public async Task<IdentityResult> DeleteAsync(IdentityRole role)
    {
        return await _roleManager.DeleteAsync(role);
    }

    public async Task<bool> RoleExistsAsync(string roleName)
    {
        return await _roleManager.RoleExistsAsync(roleName);
    }

    public async Task<int> GetRoleCountAsync()
    {
        return await _roleManager.Roles.CountAsync();
    }

    public async Task<IEnumerable<string>> GetUsersInRoleAsync(string roleName)
    {
        var users = await _userManager.GetUsersInRoleAsync(roleName);
        return users.Where(u => !u.IsDeleted).Select(u => u.Id);
    }

    public async Task<int> GetUserCountInRoleAsync(string roleName)
    {
        var users = await _userManager.GetUsersInRoleAsync(roleName);
        return users.Count(u => !u.IsDeleted);
    }

    public async Task<IEnumerable<string>> GetRolesForUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || user.IsDeleted)
            return new List<string>();

        return await _userManager.GetRolesAsync(user);
    }

    public async Task<IEnumerable<IdentityRole>> GetPaginatedRolesAsync(int pageNumber, int pageSize)
    {
        return await _roleManager.Roles
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<IdentityRole>> SearchRolesAsync(string searchTerm)
    {
        return await _roleManager.Roles
            .Where(r => r.Name!.Contains(searchTerm) ||
                       (r.NormalizedName != null && r.NormalizedName.Contains(searchTerm.ToUpper())))
            .ToListAsync();
    }
}