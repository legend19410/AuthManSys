namespace AuthManSys.Application.Common.Models.Responses;

public class ResetPasswordResponse
{
    public bool IsReset { get; }
    public string Message { get; }

    public ResetPasswordResponse(bool isReset, string message)
    {
        IsReset = isReset;
        Message = message;
    }
}