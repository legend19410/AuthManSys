using AuthManSys.Application.Common.Interfaces;
using AuthManSys.Application.Common.Models;
using AuthManSys.Domain.Entities;
using AuthManSys.Infrastructure.Database.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AuthManSys.Infrastructure.Database.Repositories;

public class TokenRepository : ITokenRepository
{
    private readonly AuthManSysDbContext _context;
    private readonly JwtSettings _jwtSettings;

    public TokenRepository(
        AuthManSysDbContext context,
        IOptions<JwtSettings> jwtSettings)
    {
        _context = context;
        _jwtSettings = jwtSettings.Value;
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

    public async Task<bool> MarkTokenAsUsedAsync(string refreshToken)
    {
        var storedRefreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == refreshToken);

        if (storedRefreshToken == null)
            return false;

        storedRefreshToken.Used = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> InvalidateRefreshTokenAsync(string refreshToken)
    {
        var storedRefreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == refreshToken);

        if (storedRefreshToken == null)
            return false;

        storedRefreshToken.Invalidated = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> InvalidateAllUserTokensAsync(int userId)
    {
        var userTokens = await _context.RefreshTokens
            .Where(x => x.UserId == userId && !x.Invalidated)
            .ToListAsync();

        if (!userTokens.Any())
            return false;

        foreach (var token in userTokens)
        {
            token.Invalidated = true;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> CleanupExpiredTokensAsync()
    {
        var expiredTokens = await _context.RefreshTokens
            .Where(x => x.ExpirationDate < DateTime.UtcNow)
            .ToListAsync();

        if (!expiredTokens.Any())
            return 0;

        _context.RefreshTokens.RemoveRange(expiredTokens);
        await _context.SaveChangesAsync();
        return expiredTokens.Count;
    }

    public async Task<IEnumerable<RefreshToken>> GetUserActiveTokensAsync(int userId)
    {
        return await _context.RefreshTokens
            .Where(x => x.UserId == userId &&
                       !x.Used &&
                       !x.Invalidated &&
                       x.ExpirationDate > DateTime.UtcNow)
            .OrderByDescending(x => x.CreationDate)
            .ToListAsync();
    }
}