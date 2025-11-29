namespace AuthManSys.Application.Common.Models.Responses;

public class ConfirmEmailResponse
{
    public bool IsConfirmed { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}