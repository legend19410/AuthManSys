namespace AuthManSys.Application.Common.Models.Responses;

public class ChangePasswordResponse
{
    public bool IsChanged { get; }
    public string Message { get; }

    public ChangePasswordResponse(bool isChanged, string message)
    {
        IsChanged = isChanged;
        Message = message;
    }
}