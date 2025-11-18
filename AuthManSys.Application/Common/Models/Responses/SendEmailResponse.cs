namespace AuthManSys.Application.Common.Models.Responses;

public class SendEmailResponse
{
    public bool IsEmailSent { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}