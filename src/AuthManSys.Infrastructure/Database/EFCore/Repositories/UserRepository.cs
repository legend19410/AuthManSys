using AuthManSys.Application.Common.Interfaces;
using AuthManSys.Application.Common.Models;
using AuthManSys.Domain.Entities;
using AuthManSys.Infrastructure.Database.EFCore.DbContext;
using AuthManSys.Infrastructure.Database.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using AuthManSys.Application.Common.Helpers;
using AutoMapper;

namespace AuthManSys.Infrastructure.Database.EFCore.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AuthManSysDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;

    public UserRepository(
        AuthManSysDbContext context,
        UserManager<ApplicationUser> userManager,
        IMapper mapper)
    {
        _context = context;
        _userManager = userManager;
        _mapper = mapper;
    }

    public async Task<User?> GetByIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user?.IsDeleted == true) return null;
        return user != null ? _mapper.Map<User>(user) : null;
    }

    public async Task<User?> GetByUserIdAsync(int userId)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.UserId == userId && !u.IsDeleted);
        return user != null ? _mapper.Map<User>(user) : null;
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        var user = await _userManager.FindByNameAsync(username);
        if (user?.IsDeleted == true) return null;
        return user != null ? _mapper.Map<User>(user) : null;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user?.IsDeleted == true) return null;
        return user != null ? _mapper.Map<User>(user) : null;
    }

    public async Task<User?> GetByGoogleIdAsync(string googleId)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.GoogleId == googleId && !u.IsDeleted);
        return user != null ? _mapper.Map<User>(user) : null;
    }

    public async Task<IEnumerable<User>> GetAllAsync(bool includeDeleted = false)
    {
        var query = _context.Users.AsQueryable();

        if (!includeDeleted)
        {
            query = query.Where(u => !u.IsDeleted);
        }

        var users = await query.ToListAsync();
        return _mapper.Map<IEnumerable<User>>(users);
    }

    public async Task<IEnumerable<User>> GetPaginatedAsync(int pageNumber, int pageSize, bool includeDeleted = false)
    {
        var query = _context.Users.AsQueryable();

        if (!includeDeleted)
        {
            query = query.Where(u => !u.IsDeleted);
        }

        var users = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return _mapper.Map<IEnumerable<User>>(users);
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

    public async Task<IdentityResult> CreateAsync(User user, string password)
    {
        var applicationUser = _mapper.Map<ApplicationUser>(user);
        return await _userManager.CreateAsync(applicationUser, password);
    }

    public async Task<IdentityResult> UpdateAsync(User user)
    {
        var applicationUser = _mapper.Map<ApplicationUser>(user);
        return await _userManager.UpdateAsync(applicationUser);
    }

    public async Task<IdentityResult> DeleteAsync(User user)
    {
        var applicationUser = _mapper.Map<ApplicationUser>(user);
        return await _userManager.DeleteAsync(applicationUser);
    }

    public async Task<IdentityResult> SoftDeleteAsync(User user, string deletedBy)
    {
        var applicationUser = _mapper.Map<ApplicationUser>(user);
        applicationUser.IsDeleted = true;
        applicationUser.DeletedAt = DateTime.UtcNow;
        applicationUser.DeletedBy = deletedBy;

        return await _userManager.UpdateAsync(applicationUser);
    }

    public async Task<IEnumerable<User>> SearchUsersAsync(string searchTerm, int pageNumber = 1, int pageSize = 10)
    {
        var query = _context.Users
            .Where(u => !u.IsDeleted &&
                       (u.UserName!.Contains(searchTerm) ||
                        u.Email!.Contains(searchTerm) ||
                        u.FirstName.Contains(searchTerm) ||
                        u.LastName.Contains(searchTerm)));

        var users = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return _mapper.Map<IEnumerable<User>>(users);
    }

    public async Task<IEnumerable<User>> GetUsersByStatusAsync(string status, int pageNumber = 1, int pageSize = 10)
    {
        var query = _context.Users
            .Where(u => !u.IsDeleted && u.Status == status);

        var users = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return _mapper.Map<IEnumerable<User>>(users);
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
    public async Task<IList<string>> GetRolesAsync(User user)
    {
        var applicationUser = _mapper.Map<ApplicationUser>(user);
        return await _userManager.GetRolesAsync(applicationUser);
    }

    public async Task<IdentityResult> AddToRoleAsync(User user, string role)
    {
        var applicationUser = _mapper.Map<ApplicationUser>(user);
        return await _userManager.AddToRoleAsync(applicationUser, role);
    }

    public async Task<IdentityResult> AddToRoleAsync(User user, string role, int? assignedBy)
    {
        // Simplified version - just add to role without tracking assignment details
        // TODO: Implement assignment tracking when we have proper DI setup
        var applicationUser = _mapper.Map<ApplicationUser>(user);
        return await _userManager.AddToRoleAsync(applicationUser, role);
    }

    public async Task<IdentityResult> RemoveFromRoleAsync(User user, string role)
    {
        var applicationUser = _mapper.Map<ApplicationUser>(user);
        return await _userManager.RemoveFromRoleAsync(applicationUser, role);
    }

    public async Task<bool> IsInRoleAsync(User user, string role)
    {
        var applicationUser = _mapper.Map<ApplicationUser>(user);
        return await _userManager.IsInRoleAsync(applicationUser, role);
    }

    public async Task<IEnumerable<User>> GetUsersInRoleAsync(string roleName)
    {
        var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);
        var filteredUsers = usersInRole.Where(u => !u.IsDeleted);
        return _mapper.Map<IEnumerable<User>>(filteredUsers);
    }

    // Password management methods
    public async Task<bool> CheckPasswordAsync(User user, string password)
    {
       // var applicationUser = _mapper.Map<ApplicationUser>(user);
        var applicationUser = await _userManager.FindByIdAsync(user.Id);
        return await _userManager.CheckPasswordAsync(applicationUser, password);
    }

    public async Task<IdentityResult> ChangePasswordAsync(User user, string currentPassword, string newPassword)
    {
        var applicationUser = _mapper.Map<ApplicationUser>(user);
        var result = await _userManager.ChangePasswordAsync(applicationUser, currentPassword, newPassword);

        if (result.Succeeded)
        {
            applicationUser.LastPasswordChangedDate = DateTime.UtcNow;
            await _userManager.UpdateAsync(applicationUser);
        }

        return result;
    }

    // Email confirmation methods
    public async Task<bool> IsEmailConfirmedAsync(User user)
    {
        var applicationUser = await _userManager.FindByIdAsync(user.Id);
        return await _userManager.IsEmailConfirmedAsync(applicationUser);
    }

    public async Task<IdentityResult> ConfirmEmailAsync(User user, string token)
    {
        var applicationUser = await _userManager.FindByIdAsync(user.Id);
        string decodedToken = Base64UrlEncoder.Decode(token);
        return await _userManager.ConfirmEmailAsync(applicationUser, decodedToken);
    }

    public async Task<string> GenerateEmailConfirmationTokenAsync(User user)
    {
        var applicationUser = await _userManager.FindByIdAsync(user.Id);
        string token = await _userManager.GenerateEmailConfirmationTokenAsync(applicationUser);
        return Base64UrlEncoder.Encode(token);
    }

    public async Task<string> GeneratePasswordResetTokenAsync(User user)
    {
        var applicationUser = await _userManager.FindByIdAsync(user.Id);
        string token = await _userManager.GeneratePasswordResetTokenAsync(applicationUser);
        return Base64UrlEncoder.Encode(token);
    }

    public async Task<IdentityResult> ResetPasswordAsync(User user, string token, string newPassword)
    {
        var applicationUser = await _userManager.FindByIdAsync(user.Id);
        string decodedToken = Base64UrlEncoder.Decode(token);
        var result = await _userManager.ResetPasswordAsync(applicationUser, decodedToken, newPassword);

        if (result.Succeeded)
        {
            applicationUser.LastPasswordChangedDate = DateTime.UtcNow;
            await _userManager.UpdateAsync(applicationUser);

            var userIsLockedOut = await _userManager.IsLockedOutAsync(applicationUser);
            if (userIsLockedOut)
            {
                await _userManager.SetLockoutEndDateAsync(applicationUser, DateTimeOffset.UtcNow);
            }
        }

        return result;
    }

    // Two factor authentication methods
    public async Task<IdentityResult> EnableTwoFactorAsync(User user)
    {
        var applicationUser = await _userManager.FindByIdAsync(user.Id);
        applicationUser.IsTwoFactorEnabled = true;
        return await _userManager.UpdateAsync(applicationUser);
    }

    public async Task<IdentityResult> DisableTwoFactorAsync(User user)
    {
        var applicationUser = await _userManager.FindByIdAsync(user.Id);
        applicationUser.IsTwoFactorEnabled = false;
        applicationUser.TwoFactorCode = null;
        applicationUser.TwoFactorCodeExpiration = null;
        applicationUser.TwoFactorCodeGeneratedAt = null;
        return await _userManager.UpdateAsync(applicationUser);
    }

    public async Task<IdentityResult> UpdateTwoFactorCodeAsync(User user, string code, DateTime expiration)
    {
        var applicationUser = await _userManager.FindByIdAsync(user.Id);
        applicationUser.TwoFactorCode = code;
        applicationUser.TwoFactorCodeExpiration = expiration;
        applicationUser.TwoFactorCodeGeneratedAt = DateTime.UtcNow;
        return await _userManager.UpdateAsync(applicationUser);
    }

    // Token generation methods
    public async Task<string> GenerateUserTokenAsync(User user, string tokenProvider, string purpose)
    {
        var applicationUser = await _userManager.FindByIdAsync(user.Id);
        return await _userManager.GenerateUserTokenAsync(applicationUser, tokenProvider, purpose);
    }

    public async Task<bool> VerifyUserTokenAsync(User user, string tokenProvider, string purpose, string token)
    {
        var applicationUser = await _userManager.FindByIdAsync(user.Id);
        return await _userManager.VerifyUserTokenAsync(applicationUser, tokenProvider, purpose, token);
    }

    // Additional methods moved from IdentityProvider
    public async Task<User?> FindByUserNameAsync(string userName)
    {
        var user = await _userManager.FindByNameAsync(userName);
        if (user?.IsDeleted == true) return null;
        return user != null ? _mapper.Map<User>(user) : null;
    }

    public async Task<User?> FindByEmailAsync(string email)
    {
        var applicationUser = await _userManager.FindByEmailAsync(email);
        if (applicationUser?.IsDeleted == true) return null;
        return applicationUser != null ? _mapper.Map<User>(applicationUser) : null;
    }

    public async Task<User?> FindByIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user?.IsDeleted == true) return null;
        return user != null ? _mapper.Map<User>(user) : null;
    }

    public async Task<IdentityResult> CreateUserAsync(string username, string email, string password, string firstName, string lastName)
    {
        var maxId = await GetMaxUserIdAsync();
        maxId += 1;
        var user = new ApplicationUser
        {
            UserId = maxId,
            UserName = username,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            EmailConfirmed = false,
            LockoutEnabled = true
        };

        return await _userManager.CreateAsync(user, password);
    }

    public async Task<IdentityResult> UpdateUserAsync(User user)
    {
        var applicationUser = _mapper.Map<ApplicationUser>(user);
        return await _userManager.UpdateAsync(applicationUser);
    }

    public async Task<IList<string>> GetUserRolesAsync(User user)
    {
        var applicationUser = _mapper.Map<ApplicationUser>(user);
        return await _userManager.GetRolesAsync(applicationUser);
    }
}