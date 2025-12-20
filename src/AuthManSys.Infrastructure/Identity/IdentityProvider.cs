using AuthManSys.Application.Common.Interfaces;
using AuthManSys.Application.Common.Helpers;
using AuthManSys.Application.Common.Models;
using AuthManSys.Domain.Entities;
using AuthManSys.Infrastructure.Database.DbContext;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthManSys.Infrastructure.Identity;

public class IdentityProvider : IIdentityProvider
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly JwtSettings _jwtSettings;
    private readonly AuthManSysDbContext _context;

    public IdentityProvider(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<JwtSettings> jwtSettings,
        AuthManSysDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtSettings = jwtSettings.Value;
        _context = context;
    }

    public async Task<ApplicationUser?> FindByUserNameAsync(string userName)
    {
        var user = await _userManager.FindByNameAsync(userName);
        return user?.IsDeleted == true ? null : user;
    }

    public async Task<ApplicationUser?> FindByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user?.IsDeleted == true ? null : user;
    }

    public async Task<ApplicationUser?> FindByIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user?.IsDeleted == true ? null : user;
    }

    public async Task<bool> CheckPasswordAsync(ApplicationUser user, string password)
    {
        return await _userManager.CheckPasswordAsync(user, password);
    }

    public async Task<IList<string>> GetUserRolesAsync(ApplicationUser user)
    {
        return await _userManager.GetRolesAsync(user);
    }

    public async Task<IdentityResult> CreateUserAsync(string username, string email, string password, string firstName, string lastName)
    {
        var user = new ApplicationUser
        {
            UserName = username,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            EmailConfirmed = false,
            LockoutEnabled = true,
            // CreatedAt = JamaicaTimeHelper.Now  // Remove if ApplicationUser doesn't have CreatedAt
        };

        return await _userManager.CreateAsync(user, password);
    }

    public async Task<IdentityResult> UpdateUserAsync(ApplicationUser user)
    {
        return await _userManager.UpdateAsync(user);
    }

    public async Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string role)
    {
        return await _userManager.AddToRoleAsync(user, role);
    }

    public async Task<IdentityResult> RemoveFromRoleAsync(ApplicationUser user, string role)
    {
        return await _userManager.RemoveFromRoleAsync(user, role);
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

            // Remove lockout if user was locked out
            var userIsLockedOut = await _userManager.IsLockedOutAsync(user);
            if (userIsLockedOut)
            {
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow);
            }
        }

        return result;
    }

    public async Task<bool> IsEmailConfirmedAsync(ApplicationUser user)
    {
        return await _userManager.IsEmailConfirmedAsync(user);
    }

    public string GenerateJwtToken(string username, string email, string userId)
    {
        var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new Claim("username", username)
            }),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public async Task<string> GenerateRefreshTokenAsync(ApplicationUser user, string jwtId)
    {
        var refreshToken = new RefreshToken
        {
            Token = Guid.NewGuid().ToString(),
            JwtId = jwtId,
            UserId = user.UserId,
            CreationDate = DateTime.UtcNow,
            ExpirationDate = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenTimeSpanInDays),
            Used = false,
            Invalidated = false
        };

        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();

        return refreshToken.Token;
    }

    public async Task<bool> ValidateRefreshTokenAsync(string refreshToken, string jwtId)
    {
        var storedRefreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == refreshToken);

        if (storedRefreshToken == null)
            return false;

        if (DateTime.UtcNow > storedRefreshToken.ExpirationDate)
            return false;

        if (storedRefreshToken.Invalidated)
            return false;

        if (storedRefreshToken.Used)
            return false;

        if (storedRefreshToken.JwtId != jwtId)
            return false;

        return true;
    }

    public async Task<ApplicationUser?> GetUserByRefreshTokenAsync(string refreshToken)
    {
        var storedRefreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == refreshToken);

        if (storedRefreshToken == null)
            return null;

        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.UserId == storedRefreshToken.UserId);

        return user?.IsDeleted == true ? null : user;
    }
}