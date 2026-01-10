using AuthManSys.Domain.Entities;
using System.Security.Claims;

namespace AuthManSys.Application.Common.Services;

public interface IJwtService
{
    /// <summary>
    /// Generate JWT access token with user information and roles
    /// </summary>
    string GenerateAccessToken(string username, string email, string userId, IList<string>? roles = null);

    /// <summary>
    /// Generate JWT access token from User object
    /// </summary>
    string GenerateAccessToken(User user, IList<string>? roles = null);

    /// <summary>
    /// Validate JWT token and return claims principal
    /// </summary>
    ClaimsPrincipal? ValidateToken(string token);

    /// <summary>
    /// Extract user ID from JWT token
    /// </summary>
    string? GetUserIdFromToken(string token);

    /// <summary>
    /// Extract username from JWT token
    /// </summary>
    string? GetUsernameFromToken(string token);

    /// <summary>
    /// Check if token is expired
    /// </summary>
    bool IsTokenExpired(string token);
}