namespace AuthManSys.Application.Common.Models.Responses;

public class RefreshTokenResponse
{
    public bool IsSuccess { get; set; }
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public string? Message { get; set; }
    public DateTime? TokenExpiration { get; set; }
    public DateTime? RefreshTokenExpiration { get; set; }
}