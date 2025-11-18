using AuthManSys.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace AuthManSys.Application.Common.Interfaces;

public interface IIdentityExtension
{
    Task<ApplicationUser?> FindByUserNameAsync(string userName);
    Task<ApplicationUser?> FindByEmailAsync(string email);
    Task<bool> IsEmailConfirmedAsync(string userName);
    Task<bool> CheckPasswordAsync(ApplicationUser applicationUser, string password);
    Task<IList<string>> GetUserRolesAsync(ApplicationUser user);
    Task<IdentityResult> CreateUserAsync(string username, string email, string password, string firstName, string lastName);
    Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string role);
    string GenerateToken(string username, string email, string userId);
    Task<IdentityResult> ConfirmEmailAsync(string userName, string token);
    Task<string> GenerateEmailConfirmationTokenAsync(string userName);
    Task<IdentityResult> UpdateEmailConfirmationTokenAsync(string userName, string token);
    Task<ApplicationUser?> FindByIdAsync(string userId);
    Task<IdentityResult> UpdateUserAsync(ApplicationUser user);
}