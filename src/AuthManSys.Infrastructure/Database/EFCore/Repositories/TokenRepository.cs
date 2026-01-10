using AuthManSys.Application.Common.Interfaces;
using AuthManSys.Application.Common.Models;
using AuthManSys.Domain.Entities;
using AuthManSys.Infrastructure.Database.EFCore.DbContext;
using AuthManSys.Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using AutoMapper;

namespace AuthManSys.Infrastructure.Database.EFCore.Repositories;

public class TokenRepository : ITokenRepository
{
    private readonly AuthManSysDbContext _context;
    private readonly JwtSettings _jwtSettings;
    private readonly IMapper _mapper;

    public TokenRepository(
        AuthManSysDbContext context,
        IOptions<JwtSettings> jwtSettings,
        IMapper mapper)
    {
        _context = context;
        _jwtSettings = jwtSettings.Value;
        _mapper = mapper;
    }

    public async Task<string> GenerateRefreshTokenAsync(User user, string jwtId)
    {
        var efRefreshToken = new AuthManSys.Infrastructure.Database.Entities.RefreshToken
        {
            Token = Guid.NewGuid().ToString(),
            JwtId = jwtId,
            UserId = user.UserId,
            CreationDate = DateTime.UtcNow,
            ExpirationDate = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenTimeSpanInDays),
            Used = false,
            Invalidated = false
        };

        await _context.RefreshTokens.AddAsync(efRefreshToken);
        await _context.SaveChangesAsync();

        return efRefreshToken.Token;
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

    public async Task<User?> GetUserByRefreshTokenAsync(string refreshToken)
    {
        var storedRefreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == refreshToken);

        if (storedRefreshToken == null)
            return null;

        var efUser = await _context.Users
            .FirstOrDefaultAsync(x => x.UserId == storedRefreshToken.UserId);

        if (efUser?.IsDeleted == true)
            return null;

        return efUser != null ? _mapper.Map<User>(efUser) : null;
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

    public async Task<IEnumerable<AuthManSys.Domain.Entities.RefreshToken>> GetUserActiveTokensAsync(int userId)
    {
        var efTokens = await _context.RefreshTokens
            .Where(x => x.UserId == userId &&
                       !x.Used &&
                       !x.Invalidated &&
                       x.ExpirationDate > DateTime.UtcNow)
            .OrderByDescending(x => x.CreationDate)
            .ToListAsync();

        return _mapper.Map<IEnumerable<AuthManSys.Domain.Entities.RefreshToken>>(efTokens);
    }
}