using System;


namespace AuthManSys.Application.Common.Interfaces
{
    /// <summary>
    /// JWT Authentication Settings
    /// </summary>
    public interface IJwtSettings
    {
        string Secret { get; set; }
        string Issuer { get; set; }
        string Audience { get; set; }
        int ExpiryMinutes { get; set; }
        TimeSpan TokenExpiration { get; set; }
        int RefreshTokenTimeSpanInDays { get; set; }
        int SessionTimeoutInMinutes { get; set; }
    }
}