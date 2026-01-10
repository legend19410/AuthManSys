using AuthManSys.Domain.Entities;
using AuthManSys.Application.Common.Models;
using Microsoft.AspNetCore.Identity;

namespace AuthManSys.Application.Common.Interfaces;

public interface IUserRepository
{
    // Basic CRUD operations
    Task<User?> GetByIdAsync(string userId);
    Task<User?> GetByUserIdAsync(int userId);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> FindByUserNameAsync(string userName);
    Task<User?> FindByEmailAsync(string email);
    Task<User?> FindByIdAsync(string userId);
    Task<IEnumerable<User>> GetAllAsync(bool includeDeleted = false);
    Task<IEnumerable<User>> GetPaginatedAsync(int pageNumber, int pageSize, bool includeDeleted = false);
    Task<int> GetTotalCountAsync(bool includeDeleted = false);
    Task<IdentityResult> CreateAsync(User user, string password);
    Task<IdentityResult> CreateUserAsync(string username, string email, string password, string firstName, string lastName);
    Task<IdentityResult> UpdateAsync(User user);
    Task<IdentityResult> UpdateUserAsync(User user);
    Task<IdentityResult> DeleteAsync(User user);
    Task<IdentityResult> SoftDeleteAsync(User user, string deletedBy);

    // Authentication related
    Task<bool> CheckPasswordAsync(User user, string password);
    Task<IdentityResult> ChangePasswordAsync(User user, string currentPassword, string newPassword);
    Task<bool> IsEmailConfirmedAsync(User user);
    Task<IdentityResult> ConfirmEmailAsync(User user, string token);
    Task<string> GenerateEmailConfirmationTokenAsync(User user);
    Task<string> GeneratePasswordResetTokenAsync(User user);
    Task<IdentityResult> ResetPasswordAsync(User user, string token, string newPassword);

    // Role management
    Task<IList<string>> GetRolesAsync(User user);
    Task<IList<string>> GetUserRolesAsync(User user);
    Task<IdentityResult> AddToRoleAsync(User user, string role);
    Task<IdentityResult> AddToRoleAsync(User user, string role, int? assignedBy);
    Task<IdentityResult> RemoveFromRoleAsync(User user, string role);
    Task<bool> IsInRoleAsync(User user, string role);

    // Two-factor authentication
    Task<IdentityResult> EnableTwoFactorAsync(User user);
    Task<IdentityResult> DisableTwoFactorAsync(User user);
    Task<IdentityResult> UpdateTwoFactorCodeAsync(User user, string code, DateTime expiration);

    // Token management
    Task<string> GenerateUserTokenAsync(User user, string tokenProvider, string purpose);
    Task<bool> VerifyUserTokenAsync(User user, string tokenProvider, string purpose, string token);


    // Search and filtering
    Task<IEnumerable<User>> SearchUsersAsync(string searchTerm, int pageNumber = 1, int pageSize = 10);
    Task<IEnumerable<User>> GetUsersByStatusAsync(string status, int pageNumber = 1, int pageSize = 10);
    Task<IEnumerable<User>> GetUsersInRoleAsync(string roleName);

    // Complex user information queries
    Task<UserInformationResponse?> GetUserInformationAsync(int userId, CancellationToken cancellationToken = default);
    Task<UserInformationResponse?> GetUserInformationByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<PagedResponse<UserInformationResponse>> GetAllUsersAsync(PagedRequest request, CancellationToken cancellationToken = default);

    // Google OAuth specific methods
    Task<User?> GetByGoogleIdAsync(string googleId);
    Task<int> GetMaxUserIdAsync();
}