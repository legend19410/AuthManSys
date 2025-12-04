using AuthManSys.Domain.Entities;
using AuthManSys.Application.Common.Models;
using Microsoft.AspNetCore.Identity;

namespace AuthManSys.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<ApplicationUser?> FindByUserNameAsync(string userName);
    Task<ApplicationUser?> FindByEmailAsync(string email);
    Task<bool> IsEmailConfirmedAsync(string userName);
    Task<bool> CheckPasswordAsync(ApplicationUser applicationUser, string password);
    Task<IList<string>> GetUserRolesAsync(ApplicationUser user);
    Task<IdentityResult> CreateUserAsync(string username, string email, string password, string firstName, string lastName);
    Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string role);
    Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string role, int? assignedBy);
    Task<IdentityResult> RemoveFromRoleAsync(ApplicationUser user, string role);
    string GenerateToken(string username, string email, string userId);
    Task<IdentityResult> ConfirmEmailAsync(string userName, string token);
    Task<string> GenerateEmailConfirmationTokenAsync(string userName);
    Task<IdentityResult> UpdateEmailConfirmationTokenAsync(string userName, string token);
    Task<ApplicationUser?> FindByIdAsync(string userId);
    Task<ApplicationUser?> FindByUserIdAsync(int userId);
    Task<IdentityResult> UpdateUserAsync(ApplicationUser user);
    Task<IdentityResult> CreateRoleAsync(string roleName, string? description = null);
    Task<bool> RoleExistsAsync(string roleName);
    Task<IdentityResult> EnableTwoFactorAsync(ApplicationUser user);
    Task<IdentityResult> DisableTwoFactorAsync(ApplicationUser user);
    Task<IdentityResult> UpdateTwoFactorCodeAsync(ApplicationUser user, string code, DateTime expiration);
    Task<IEnumerable<string>> GetAllRolesAsync();
    Task<IEnumerable<RoleDto>> GetAllRolesWithDetailsAsync();
    Task<string> GenerateRefreshTokenAsync(ApplicationUser user, string jwtId);
    Task<bool> ValidateRefreshTokenAsync(string refreshToken, string jwtId);
    Task<ApplicationUser?> GetUserByRefreshTokenAsync(string refreshToken);
}