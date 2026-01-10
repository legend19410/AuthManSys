namespace AuthManSys.Domain.Entities;

public class RefreshToken
{
    public int Id { get; private set; }
    public string Token { get; private set; }
    public string JwtId { get; private set; }
    public int UserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsUsed { get; private set; }
    public bool IsInvalidated { get; private set; }

    private RefreshToken() { } // For ORM

    public RefreshToken(
        string token,
        string jwtId,
        int userId,
        DateTime expiresAt
    )
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token is required", nameof(token));

        if (string.IsNullOrWhiteSpace(jwtId))
            throw new ArgumentException("JWT Id is required", nameof(jwtId));

        if (expiresAt <= DateTime.UtcNow)
            throw new ArgumentException("Expiration must be in the future");

        Token = token;
        JwtId = jwtId;
        UserId = userId;
        CreatedAt = DateTime.UtcNow;
        ExpiresAt = expiresAt;
        IsUsed = false;
        IsInvalidated = false;
    }

    // --------------------
    // Domain Rules
    // --------------------

    public bool IsExpired()
        => DateTime.UtcNow >= ExpiresAt;

    public void MarkAsUsed()
    {
        if (IsUsed)
            throw new InvalidOperationException("Refresh token has already been used.");

        IsUsed = true;
    }

    public void Invalidate()
    {
        IsInvalidated = true;
    }

    public bool IsValid()
        => !IsUsed && !IsInvalidated && !IsExpired();
}