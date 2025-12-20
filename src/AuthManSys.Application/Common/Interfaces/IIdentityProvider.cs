using AuthManSys.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace AuthManSys.Application.Common.Interfaces;

public interface IIdentityProvider
{
    Task<ApplicationUser?> FindByUserNameAsync(string userName);
    Task<ApplicationUser?> FindByEmailAsync(string email);
    Task<ApplicationUser?> FindByIdAsync(string userId);
    Task<bool> CheckPasswordAsync(ApplicationUser user, string password);
    Task<IList<string>> GetUserRolesAsync(ApplicationUser user);
    Task<IdentityResult> CreateUserAsync(string username, string email, string password, string firstName, string lastName);
    Task<IdentityResult> UpdateUserAsync(ApplicationUser user);
    Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string role);
    Task<IdentityResult> RemoveFromRoleAsync(ApplicationUser user, string role);
    Task<IdentityResult> ChangePasswordAsync(ApplicationUser user, string currentPassword, string newPassword);
    Task<IdentityResult> ConfirmEmailAsync(ApplicationUser user, string token);
    Task<string> GenerateEmailConfirmationTokenAsync(ApplicationUser user);
    Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user);
    Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string token, string newPassword);
    Task<bool> IsEmailConfirmedAsync(ApplicationUser user);
    string GenerateJwtToken(string username, string email, string userId);
    Task<string> GenerateRefreshTokenAsync(ApplicationUser user, string jwtId);
    Task<bool> ValidateRefreshTokenAsync(string refreshToken, string jwtId);
    Task<ApplicationUser?> GetUserByRefreshTokenAsync(string refreshToken);
}