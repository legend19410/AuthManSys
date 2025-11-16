using AuthManSys.Application.Common.Exceptions;
using AuthManSys.Application.Common.Interfaces;
using AuthManSys.Domain.Entities;
using Microsoft.AspNetCore.Identity;

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
        private readonly JwtSettings jwtSettings;


        public IdentityExtension(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IOptions<JwtSettings> jwtSettings
        )
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.jwtSettings = jwtSettings.Value;

        
        }

        public async Task<ApplicationUser?> FindByUserNameAsync(string userName)
        {
            var user = await userManager.FindByNameAsync(userName);

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

    }
}