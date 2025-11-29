namespace AuthManSys.Application.Common.Models.Responses;

public class ForgotPasswordResponse
{
    public bool IsEmailSent { get; }
    public string Message { get; }

    public ForgotPasswordResponse(bool isEmailSent, string message)
    {
        IsEmailSent = isEmailSent;
        Message = message;
    }
}