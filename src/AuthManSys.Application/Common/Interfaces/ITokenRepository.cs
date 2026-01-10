using AuthManSys.Domain.Entities;

namespace AuthManSys.Application.Common.Interfaces;

public interface ITokenRepository
{
    /// <summary>
    /// Generate and store a new refresh token
    /// </summary>
    Task<string> GenerateRefreshTokenAsync(User user, string jwtId);

    /// <summary>
    /// Validate if refresh token exists and is valid
    /// </summary>
    Task<bool> ValidateRefreshTokenAsync(string refreshToken, string jwtId);

    /// <summary>
    /// Get user by refresh token
    /// </summary>
    Task<User?> GetUserByRefreshTokenAsync(string refreshToken);

    /// <summary>
    /// Mark refresh token as used
    /// </summary>
    Task<bool> MarkTokenAsUsedAsync(string refreshToken);

    /// <summary>
    /// Invalidate a specific refresh token
    /// </summary>
    Task<bool> InvalidateRefreshTokenAsync(string refreshToken);

    /// <summary>
    /// Invalidate all refresh tokens for a user
    /// </summary>
    Task<bool> InvalidateAllUserTokensAsync(int userId);

    /// <summary>
    /// Clean up expired refresh tokens
    /// </summary>
    Task<int> CleanupExpiredTokensAsync();

    /// <summary>
    /// Get all active tokens for a user
    /// </summary>
    Task<IEnumerable<RefreshToken>> GetUserActiveTokensAsync(int userId);
}