namespace AuthManSys.Application.Common.Models.Responses;

public class VerifyTwoFactorCodeResponse
{
    public bool IsVerified { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? AccessToken { get; set; }
    public DateTime? VerifiedAt { get; set; }
}