using System;

namespace AuthManSys.Application.Common.Models.Responses;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new List<string>();
    public bool RequiresTwoFactor { get; set; }
    public string? Message { get; set; }
    public DateTime? RefreshTokenExpiration { get; set; }
}