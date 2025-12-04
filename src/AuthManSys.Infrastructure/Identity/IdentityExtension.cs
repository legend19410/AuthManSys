using AuthManSys.Application.Common.Exceptions;
using AuthManSys.Application.Common.Interfaces;
using AuthManSys.Application.Common.Helpers;
using AuthManSys.Domain.Entities;
using AuthManSys.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using AuthManSys.Application.Common.Models;


namespace AuthManSys.Infrastructure.Identity
{
    public class IdentityExtension : IIdentityExtension
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly JwtSettings jwtSettings;
        private readonly IAuthManSysDbContext dbContext;


        public IdentityExtension(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<JwtSettings> jwtSettings,
            IAuthManSysDbContext dbContext
        )
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.roleManager = roleManager;
            this.jwtSettings = jwtSettings.Value;
            this.dbContext = dbContext;
        }

        public async Task<ApplicationUser?> FindByUserNameAsync(string userName)
        {
            var user = await userManager.FindByNameAsync(userName);

            // Return null if user is soft deleted
            if (user?.IsDeleted == true)
                return null;

            return user;
        }

        public async Task<bool> CheckPasswordAsync(ApplicationUser user, string password)
        {
            var isPasswordVerified = await userManager.CheckPasswordAsync(user, password);

            return isPasswordVerified;
        }

        public async Task<IList<string>> GetUserRolesAsync(ApplicationUser user)
        {
            var roles = await userManager.GetRolesAsync(user);
            return roles;
        }


        public async Task<SignInResult> PasswordSignInAsync(string userName, string password)
        {
            return await signInManager.PasswordSignInAsync(userName, password, false, true);
        }

      

        public async Task<IdentityResult> ChangePasswordAsync(string userName, string currentPassword, string newPassword)
        {
            var applicationUser = await userManager.FindByNameAsync(userName);

            if (applicationUser == null)
            {
                throw new NotFoundException("application user", userName);
            }

            var result = await userManager.ChangePasswordAsync(applicationUser, currentPassword, newPassword);

            if (result.Succeeded)
            {
                // Update Last Password Changed Date
                applicationUser.LastPasswordChangedDate = DateTime.UtcNow;
                await userManager.UpdateAsync(applicationUser);
            }

            return result;
        }

        public async Task<bool> Register(ApplicationUser user, string password)
        {
            var result = await userManager.CreateAsync(user, password);
            return result.Succeeded;
        }

        public async Task<IdentityResult> ConfirmEmailAsync(string userName, string token)
        {
            var applicationUser = await userManager.FindByNameAsync(userName);
            string decodedToken = Base64UrlEncoder.Decode(token);

            return await userManager.ConfirmEmailAsync(applicationUser, decodedToken);
        }

        public async Task<string> GenerateEmailConfirmationTokenAsync(string userName)
        {
            var applicationUser = await userManager.FindByNameAsync(userName);
            string token = await userManager.GenerateEmailConfirmationTokenAsync(applicationUser);

            return Base64UrlEncoder.Encode(token);
        }

        public async Task<IdentityResult> UpdateEmailConfirmationTokenAsync(string userName, string token)
        {
            var applicationUser = await userManager.FindByNameAsync(userName);
            applicationUser.EmailConfirmationToken = token;

            var result = await userManager.UpdateAsync(applicationUser);

            return result;
        }

        public async Task<IdentityResult> UpdatePasswordResetTokenAsync(string userName, string token)
        {
            var applicationUser = await userManager.FindByNameAsync(userName);
            applicationUser.PasswordResetToken = token;

            var result = await userManager.UpdateAsync(applicationUser);

            return result;
        }

        public async Task<IdentityResult> UpdatePasswordAsync(string userName, string token, string password)
        {
            var applicationUser = await userManager.FindByNameAsync(userName);
            string decodedToken = Base64UrlEncoder.Decode(token);

            var result = await userManager.ResetPasswordAsync(applicationUser, decodedToken, password);

            if (result.Succeeded)
            {
                // Update Last Password Changed Date
                applicationUser.LastPasswordChangedDate = DateTime.UtcNow;
                await userManager.UpdateAsync(applicationUser);

                var userIsLockedOut = await userManager.IsLockedOutAsync(applicationUser);
                if (userIsLockedOut)
                {
                    await userManager.SetLockoutEndDateAsync(applicationUser, DateTimeOffset.UtcNow);
                }
            }

            return result;
        }

        public async Task<string> GeneratePasswordResetTokenAsync(string userName)
        {
            var applicationUser = await userManager.FindByNameAsync(userName);
            string token = await userManager.GeneratePasswordResetTokenAsync(applicationUser);

            return Base64UrlEncoder.Encode(token);
        }

        public async Task<string> GenerateRequestVerificationTokenAsync(string userName)
        {
            var applicationUser = await userManager.FindByNameAsync(userName);
            string token = await userManager.GenerateUserTokenAsync(applicationUser, TokenOptions.DefaultPhoneProvider, "RequestVerification");

            return token;
        }

        public async Task<bool> VerifyUserRequestTokenAsync(string userName, string token)
        {
            var applicationUser = await userManager.FindByNameAsync(userName);

            if (applicationUser == null) { return false; }
            if (applicationUser.RequestVerificationTokenExpiration == null) { return false; }
            if (DateTime.Compare(DateTime.UtcNow, applicationUser.RequestVerificationTokenExpiration.Value) > 0) { return false; }

            var result = await userManager.VerifyUserTokenAsync(applicationUser, TokenOptions.DefaultPhoneProvider, "RequestVerification", token);
            if (result) { await userManager.UpdateSecurityStampAsync(applicationUser); }
            return result;
        }

        public async Task<IdentityResult> UpdateRequestVerificationTokenAsync(string userName, string token, double expirationInMinutes)
        {
            var applicationUser = await userManager.FindByNameAsync(userName);
            applicationUser.RequestVerificationToken = token;
            if (token == null) { applicationUser.RequestVerificationTokenExpiration = null; }
            else { applicationUser.RequestVerificationTokenExpiration = DateTime.UtcNow.AddMinutes(expirationInMinutes); }

            var result = await userManager.UpdateAsync(applicationUser);

            return result;
        }

        public async Task<bool> IsEmailConfirmedAsync(string userName)
        {
            var user = await userManager.FindByNameAsync(userName);
            var result = await userManager.IsEmailConfirmedAsync(user);
            return result;
        }



    public string GenerateToken(string username, string email, string userId)
    {
        if (string.IsNullOrEmpty(username))
            throw new ArgumentException("Username cannot be null or empty", nameof(username));
        if (string.IsNullOrEmpty(email))
            throw new ArgumentException("Email cannot be null or empty", nameof(email));
        if (string.IsNullOrEmpty(userId))
            throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(this.jwtSettings.SecretKey);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Email, email),
            new Claim("username", username),
            new Claim("email", email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(this.jwtSettings.ExpirationInMinutes),
            Issuer = this.jwtSettings.Issuer,
            Audience = this.jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public async Task<ApplicationUser?> FindByEmailAsync(string email)
    {
        var user = await userManager.FindByEmailAsync(email);

        // Return null if user is soft deleted
        if (user?.IsDeleted == true)
            return null;

        return user;
    }

    public async Task<IdentityResult> CreateUserAsync(string username, string email, string password, string firstName, string lastName)
    {
        var user = new ApplicationUser
        {
            UserName = username,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            EmailConfirmed = false, // Will need email confirmation
            EmailConfirmationToken = string.Empty,
            PasswordResetToken = string.Empty,
            RequestVerificationToken = string.Empty,
            TermsConditionsAccepted = false,
            LastPasswordChangedDate = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(user, password);
        return result;
    }

    public async Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string role)
    {
        return await userManager.AddToRoleAsync(user, role);
    }

    public async Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string role, int? assignedBy)
    {
        var context = (AuthManSysDbContext)dbContext;
        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            // First add the role using UserManager
            var result = await userManager.AddToRoleAsync(user, role);
            if (!result.Succeeded)
            {
                await transaction.RollbackAsync();
                return result;
            }

            // Then update the UserRole record with our custom fields
            var roleEntity = await roleManager.FindByNameAsync(role);
            if (roleEntity != null)
            {
                var userRole = await context.Set<ApplicationUserRole>()
                    .FirstOrDefaultAsync(ur => ur.UserId == user.Id && ur.RoleId == roleEntity.Id);

                if (userRole != null)
                {
                    userRole.AssignedAt = JamaicaTimeHelper.Now;
                    userRole.AssignedBy = assignedBy;
                    await context.SaveChangesAsync();
                }
            }

            await transaction.CommitAsync();
            return IdentityResult.Success;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<IdentityResult> RemoveFromRoleAsync(ApplicationUser user, string role)
    {
        return await userManager.RemoveFromRoleAsync(user, role);
    }

    public async Task<ApplicationUser?> FindByIdAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);

        // Return null if user is soft deleted
        if (user?.IsDeleted == true)
            return null;

        return user;
    }

    public async Task<ApplicationUser?> FindByUserIdAsync(int userId)
    {
        var user = await userManager.Users
            .FirstOrDefaultAsync(u => u.UserId == userId && !u.IsDeleted);

        return user;
    }

    public async Task<IdentityResult> UpdateUserAsync(ApplicationUser user)
    {
        return await userManager.UpdateAsync(user);
    }

    public async Task<IdentityResult> CreateRoleAsync(string roleName, string? description = null)
    {
        var role = new IdentityRole(roleName);
        return await roleManager.CreateAsync(role);
    }

    public async Task<bool> RoleExistsAsync(string roleName)
    {
        return await roleManager.RoleExistsAsync(roleName);
    }

    public async Task<IdentityResult> EnableTwoFactorAsync(ApplicationUser user)
    {
        user.IsTwoFactorEnabled = true;
        return await userManager.UpdateAsync(user);
    }

    public async Task<IdentityResult> DisableTwoFactorAsync(ApplicationUser user)
    {
        user.IsTwoFactorEnabled = false;
        user.TwoFactorCode = null;
        user.TwoFactorCodeExpiration = null;
        user.TwoFactorCodeGeneratedAt = null;
        return await userManager.UpdateAsync(user);
    }

    public async Task<IdentityResult> UpdateTwoFactorCodeAsync(ApplicationUser user, string code, DateTime expiration)
    {
        user.TwoFactorCode = code;
        user.TwoFactorCodeExpiration = expiration;
        user.TwoFactorCodeGeneratedAt = DateTime.UtcNow;
        return await userManager.UpdateAsync(user);
    }

    public async Task<IEnumerable<string>> GetAllRolesAsync()
    {
        var roles = await roleManager.Roles.Select(r => r.Name!).ToListAsync();
        return roles;
    }

    public async Task<IEnumerable<RoleDto>> GetAllRolesWithDetailsAsync()
    {
        var roles = await roleManager.Roles
            .Select(r => new {
                Id = r.Id,
                Name = r.Name!,
                NormalizedName = r.NormalizedName,
                Description = EF.Property<string>(r, "Description")
            })
            .ToListAsync();

        return roles.Select(r => new RoleDto(r.Id, r.Name, r.NormalizedName, r.Description));
    }

    public async Task<string> GenerateRefreshTokenAsync(ApplicationUser user, string jwtId)
    {
        var refreshToken = new RefreshToken
        {
            Token = Guid.NewGuid().ToString(),
            JwtId = jwtId,
            UserId = user.UserId,
            CreationDate = DateTime.UtcNow,
            ExpirationDate = DateTime.UtcNow.AddDays(jwtSettings.RefreshTokenTimeSpanInDays),
            Used = false,
            Invalidated = false
        };

        await dbContext.RefreshTokens.AddAsync(refreshToken);
        await dbContext.SaveChangesAsync();

        return refreshToken.Token;
    }

    public async Task<bool> ValidateRefreshTokenAsync(string refreshToken, string jwtId)
    {
        var storedRefreshToken = await dbContext.RefreshTokens
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
        var storedRefreshToken = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == refreshToken);

        if (storedRefreshToken == null)
            return null;

        var user = await userManager.Users
            .FirstOrDefaultAsync(x => x.UserId == storedRefreshToken.UserId);

        return user;
    }
}
}