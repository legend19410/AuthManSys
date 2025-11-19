namespace AuthManSys.Application.Common.Models.Responses;

public class SendTwoFactorCodeResponse
{
    public bool IsCodeSent { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime? CodeExpiresAt { get; set; }
}