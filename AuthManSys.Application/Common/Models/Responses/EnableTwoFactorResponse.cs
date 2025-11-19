namespace AuthManSys.Application.Common.Models.Responses;

public class EnableTwoFactorResponse
{
    public bool IsEnabled { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public DateTime? EnabledAt { get; set; }
}