using AuthManSys.Application.Common.Interfaces;
using AuthManSys.Application.Common.Models;
using AuthManSys.Domain.Entities;
using AuthManSys.Infrastructure.Database.DbContext;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AuthManSys.Infrastructure.Database.Repositories;

public class UserDataRepository : IUserRepository
{
    private readonly AuthManSysDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserDataRepository(
        AuthManSysDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<ApplicationUser?> GetByIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user?.IsDeleted == true ? null : user;
    }

    public async Task<ApplicationUser?> GetByUserIdAsync(int userId)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.UserId == userId && !u.IsDeleted);
    }

    public async Task<ApplicationUser?> GetByUsernameAsync(string username)
    {
        var user = await _userManager.FindByNameAsync(username);
        return user?.IsDeleted == true ? null : user;
    }

    public async Task<ApplicationUser?> GetByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user?.IsDeleted == true ? null : user;
    }

    public async Task<ApplicationUser?> GetByGoogleIdAsync(string googleId)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.GoogleId == googleId && !u.IsDeleted);
    }

    public async Task<IEnumerable<ApplicationUser>> GetAllAsync(bool includeDeleted = false)
    {
        var query = _context.Users.AsQueryable();

        if (!includeDeleted)
        {
            query = query.Where(u => !u.IsDeleted);
        }

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<ApplicationUser>> GetPaginatedAsync(int pageNumber, int pageSize, bool includeDeleted = false)
    {
        var query = _context.Users.AsQueryable();

        if (!includeDeleted)
        {
            query = query.Where(u => !u.IsDeleted);
        }

        return await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync(bool includeDeleted = false)
    {
        var query = _context.Users.AsQueryable();

        if (!includeDeleted)
        {
            query = query.Where(u => !u.IsDeleted);
        }

        return await query.CountAsync();
    }

    public async Task<IdentityResult> CreateAsync(ApplicationUser user, string password)
    {
        return await _userManager.CreateAsync(user, password);
    }

    public async Task<IdentityResult> UpdateAsync(ApplicationUser user)
    {
        return await _userManager.UpdateAsync(user);
    }

    public async Task<IdentityResult> DeleteAsync(ApplicationUser user)
    {
        return await _userManager.DeleteAsync(user);
    }

    public async Task<IdentityResult> SoftDeleteAsync(ApplicationUser user, string deletedBy)
    {
        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;
        user.DeletedBy = deletedBy;

        return await _userManager.UpdateAsync(user);
    }

    public async Task<IEnumerable<ApplicationUser>> SearchUsersAsync(string searchTerm, int pageNumber = 1, int pageSize = 10)
    {
        var query = _context.Users
            .Where(u => !u.IsDeleted &&
                       (u.UserName!.Contains(searchTerm) ||
                        u.Email!.Contains(searchTerm) ||
                        u.FirstName.Contains(searchTerm) ||
                        u.LastName.Contains(searchTerm)));

        return await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<ApplicationUser>> GetUsersByStatusAsync(string status, int pageNumber = 1, int pageSize = 10)
    {
        var query = _context.Users
            .Where(u => !u.IsDeleted && u.Status == status);

        return await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<UserInformationResponse?> GetUserInformationAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Where(u => !u.IsDeleted)
            .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);

        if (user == null)
        {
            return null;
        }

        return await BuildUserInformationResponseAsync(user, cancellationToken);
    }

    public async Task<UserInformationResponse?> GetUserInformationByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Where(u => !u.IsDeleted)
            .FirstOrDefaultAsync(u => u.UserName == username, cancellationToken);

        if (user == null)
        {
            return null;
        }

        return await BuildUserInformationResponseAsync(user, cancellationToken);
    }

    public async Task<PagedResponse<UserInformationResponse>> GetAllUsersAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        var query = _context.Users.Where(u => !u.IsDeleted).AsQueryable();

        // Apply search filter
        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            query = query.Where(u =>
                u.UserName!.Contains(request.SearchTerm) ||
                u.Email!.Contains(request.SearchTerm) ||
                u.FirstName.Contains(request.SearchTerm) ||
                u.LastName.Contains(request.SearchTerm));
        }

        // Apply sorting
        switch (request.SortBy.ToLower())
        {
            case "username":
                query = request.SortDescending ? query.OrderByDescending(u => u.UserName) : query.OrderBy(u => u.UserName);
                break;
            case "email":
                query = request.SortDescending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email);
                break;
            case "firstname":
                query = request.SortDescending ? query.OrderByDescending(u => u.FirstName) : query.OrderBy(u => u.FirstName);
                break;
            case "lastname":
                query = request.SortDescending ? query.OrderByDescending(u => u.LastName) : query.OrderBy(u => u.LastName);
                break;
            default:
                query = request.SortDescending ? query.OrderByDescending(u => u.UserId) : query.OrderBy(u => u.UserId);
                break;
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var users = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        // Convert to UserInformationResponse
        var userResponses = new List<UserInformationResponse>();
        foreach (var user in users)
        {
            var userResponse = await BuildUserInformationResponseAsync(user, cancellationToken);
            if (userResponse != null)
            {
                userResponses.Add(userResponse);
            }
        }

        return new PagedResponse<UserInformationResponse>
        {
            Items = userResponses.AsReadOnly(),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<int> GetMaxUserIdAsync()
    {
        var maxUserId = await _context.Users
            .Where(u => !u.IsDeleted)
            .MaxAsync(u => (int?)u.UserId) ?? 0;
        return maxUserId;
    }

    private async Task<UserInformationResponse> BuildUserInformationResponseAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        // Use LastPasswordChangedDate as a proxy for creation date, or a reasonable default
        DateTime createdAt = user.LastPasswordChangedDate != default(DateTime)
            ? user.LastPasswordChangedDate
            : DateTime.UtcNow.AddDays(-30); // Default fallback if not set

        // Get user roles from Identity framework using the proper DbSets
        var userRoleQuery = from ur in _context.Set<ApplicationUserRole>()
                           join r in _context.Set<IdentityRole>() on ur.RoleId equals r.Id
                           where ur.UserId == user.Id
                           select new {
                               RoleId = r.Id,
                               RoleName = r.Name,
                               RoleDescription = EF.Property<string>(r, "Description"),
                               AssignedAt = ur.AssignedAt
                           };

        var userRoleData = await userRoleQuery.ToListAsync(cancellationToken);

        var userRoles = userRoleData.Select(rd => new UserRoleDto(
            rd.RoleId, // Use original string role ID
            rd.RoleName ?? "Unknown",
            rd.RoleDescription ?? rd.RoleName ?? "Unknown", // Using role name as description temporarily
            rd.AssignedAt == default(DateTime) ? createdAt : rd.AssignedAt // Use user creation date as fallback for default DateTime
        )).ToList();

        // IsActive: user is active if not locked out
        bool isActive = !user.LockoutEnabled || user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow;

        return new UserInformationResponse(
            user.UserId,
            user.UserName ?? string.Empty,
            user.Email ?? string.Empty,
            user.FirstName,
            user.LastName,
            isActive,
            createdAt,
            user.LastLoginAt, // LastLoginAt - now tracked in ApplicationUser
            user.IsTwoFactorEnabled,
            userRoles.AsReadOnly()
        );
    }

    // Role management methods - kept minimal
    public async Task<IList<string>> GetRolesAsync(ApplicationUser user)
    {
        return await _userManager.GetRolesAsync(user);
    }

    public async Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string role)
    {
        return await _userManager.AddToRoleAsync(user, role);
    }

    public async Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string role, int? assignedBy)
    {
        // Simplified version - just add to role without tracking assignment details
        // TODO: Implement assignment tracking when we have proper DI setup
        return await _userManager.AddToRoleAsync(user, role);
    }

    public async Task<IdentityResult> RemoveFromRoleAsync(ApplicationUser user, string role)
    {
        return await _userManager.RemoveFromRoleAsync(user, role);
    }

    public async Task<bool> IsInRoleAsync(ApplicationUser user, string role)
    {
        return await _userManager.IsInRoleAsync(user, role);
    }

    public async Task<IEnumerable<ApplicationUser>> GetUsersInRoleAsync(string roleName)
    {
        var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);
        return usersInRole.Where(u => !u.IsDeleted);
    }

    // Password management methods
    public async Task<bool> CheckPasswordAsync(ApplicationUser user, string password)
    {
        return await _userManager.CheckPasswordAsync(user, password);
    }

    public async Task<IdentityResult> ChangePasswordAsync(ApplicationUser user, string currentPassword, string newPassword)
    {
        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

        if (result.Succeeded)
        {
            user.LastPasswordChangedDate = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
        }

        return result;
    }

    // Email confirmation methods
    public async Task<bool> IsEmailConfirmedAsync(ApplicationUser user)
    {
        return await _userManager.IsEmailConfirmedAsync(user);
    }

    public async Task<IdentityResult> ConfirmEmailAsync(ApplicationUser user, string token)
    {
        string decodedToken = Base64UrlEncoder.Decode(token);
        return await _userManager.ConfirmEmailAsync(user, decodedToken);
    }

    public async Task<string> GenerateEmailConfirmationTokenAsync(ApplicationUser user)
    {
        string token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        return Base64UrlEncoder.Encode(token);
    }

    public async Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user)
    {
        string token = await _userManager.GeneratePasswordResetTokenAsync(user);
        return Base64UrlEncoder.Encode(token);
    }

    public async Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string token, string newPassword)
    {
        string decodedToken = Base64UrlEncoder.Decode(token);
        var result = await _userManager.ResetPasswordAsync(user, decodedToken, newPassword);

        if (result.Succeeded)
        {
            user.LastPasswordChangedDate = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            var userIsLockedOut = await _userManager.IsLockedOutAsync(user);
            if (userIsLockedOut)
            {
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow);
            }
        }

        return result;
    }

    // Two factor authentication methods
    public async Task<IdentityResult> EnableTwoFactorAsync(ApplicationUser user)
    {
        user.IsTwoFactorEnabled = true;
        return await _userManager.UpdateAsync(user);
    }

    public async Task<IdentityResult> DisableTwoFactorAsync(ApplicationUser user)
    {
        user.IsTwoFactorEnabled = false;
        user.TwoFactorCode = null;
        user.TwoFactorCodeExpiration = null;
        user.TwoFactorCodeGeneratedAt = null;
        return await _userManager.UpdateAsync(user);
    }

    public async Task<IdentityResult> UpdateTwoFactorCodeAsync(ApplicationUser user, string code, DateTime expiration)
    {
        user.TwoFactorCode = code;
        user.TwoFactorCodeExpiration = expiration;
        user.TwoFactorCodeGeneratedAt = DateTime.UtcNow;
        return await _userManager.UpdateAsync(user);
    }

    // Token generation methods
    public async Task<string> GenerateUserTokenAsync(ApplicationUser user, string tokenProvider, string purpose)
    {
        return await _userManager.GenerateUserTokenAsync(user, tokenProvider, purpose);
    }

    public async Task<bool> VerifyUserTokenAsync(ApplicationUser user, string tokenProvider, string purpose, string token)
    {
        return await _userManager.VerifyUserTokenAsync(user, tokenProvider, purpose, token);
    }

    // Refresh token methods - simplified, but keeping for backwards compatibility
    public Task<string> GenerateRefreshTokenAsync(ApplicationUser user, string jwtId)
    {
        throw new NotImplementedException("Use IIdentityProvider.GenerateRefreshTokenAsync instead");
    }

    public Task<bool> ValidateRefreshTokenAsync(string refreshToken, string jwtId)
    {
        throw new NotImplementedException("Use IIdentityProvider.ValidateRefreshTokenAsync instead");
    }

    public Task<ApplicationUser?> GetUserByRefreshTokenAsync(string refreshToken)
    {
        throw new NotImplementedException("Use IIdentityProvider.GetUserByRefreshTokenAsync instead");
    }
}