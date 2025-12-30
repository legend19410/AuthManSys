using AuthManSys.Domain.Entities;
using AuthManSys.Application.Common.Models;
using Microsoft.AspNetCore.Identity;

namespace AuthManSys.Application.Common.Interfaces;

public interface IUserRepository
{
    // Basic CRUD operations
    Task<ApplicationUser?> GetByIdAsync(string userId);
    Task<ApplicationUser?> GetByUserIdAsync(int userId);
    Task<ApplicationUser?> GetByUsernameAsync(string username);
    Task<ApplicationUser?> GetByEmailAsync(string email);
    Task<ApplicationUser?> FindByUserNameAsync(string userName);
    Task<ApplicationUser?> FindByEmailAsync(string email);
    Task<ApplicationUser?> FindByIdAsync(string userId);
    Task<IEnumerable<ApplicationUser>> GetAllAsync(bool includeDeleted = false);
    Task<IEnumerable<ApplicationUser>> GetPaginatedAsync(int pageNumber, int pageSize, bool includeDeleted = false);
    Task<int> GetTotalCountAsync(bool includeDeleted = false);
    Task<IdentityResult> CreateAsync(ApplicationUser user, string password);
    Task<IdentityResult> CreateUserAsync(string username, string email, string password, string firstName, string lastName);
    Task<IdentityResult> UpdateAsync(ApplicationUser user);
    Task<IdentityResult> UpdateUserAsync(ApplicationUser user);
    Task<IdentityResult> DeleteAsync(ApplicationUser user);
    Task<IdentityResult> SoftDeleteAsync(ApplicationUser user, string deletedBy);

    // Authentication related
    Task<bool> CheckPasswordAsync(ApplicationUser user, string password);
    Task<IdentityResult> ChangePasswordAsync(ApplicationUser user, string currentPassword, string newPassword);
    Task<bool> IsEmailConfirmedAsync(ApplicationUser user);
    Task<IdentityResult> ConfirmEmailAsync(ApplicationUser user, string token);
    Task<string> GenerateEmailConfirmationTokenAsync(ApplicationUser user);
    Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user);
    Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string token, string newPassword);

    // Role management
    Task<IList<string>> GetRolesAsync(ApplicationUser user);
    Task<IList<string>> GetUserRolesAsync(ApplicationUser user);
    Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string role);
    Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string role, int? assignedBy);
    Task<IdentityResult> RemoveFromRoleAsync(ApplicationUser user, string role);
    Task<bool> IsInRoleAsync(ApplicationUser user, string role);

    // Two-factor authentication
    Task<IdentityResult> EnableTwoFactorAsync(ApplicationUser user);
    Task<IdentityResult> DisableTwoFactorAsync(ApplicationUser user);
    Task<IdentityResult> UpdateTwoFactorCodeAsync(ApplicationUser user, string code, DateTime expiration);

    // Token management
    Task<string> GenerateUserTokenAsync(ApplicationUser user, string tokenProvider, string purpose);
    Task<bool> VerifyUserTokenAsync(ApplicationUser user, string tokenProvider, string purpose, string token);


    // Search and filtering
    Task<IEnumerable<ApplicationUser>> SearchUsersAsync(string searchTerm, int pageNumber = 1, int pageSize = 10);
    Task<IEnumerable<ApplicationUser>> GetUsersByStatusAsync(string status, int pageNumber = 1, int pageSize = 10);
    Task<IEnumerable<ApplicationUser>> GetUsersInRoleAsync(string roleName);

    // Complex user information queries
    Task<UserInformationResponse?> GetUserInformationAsync(int userId, CancellationToken cancellationToken = default);
    Task<UserInformationResponse?> GetUserInformationByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<PagedResponse<UserInformationResponse>> GetAllUsersAsync(PagedRequest request, CancellationToken cancellationToken = default);

    // Google OAuth specific methods
    Task<ApplicationUser?> GetByGoogleIdAsync(string googleId);
    Task<int> GetMaxUserIdAsync();
}