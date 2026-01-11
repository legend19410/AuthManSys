namespace AuthManSys.Application.Common.Models;

public class UserOperationResult
{
    public bool Succeeded { get; set; }
    public IEnumerable<string> Errors { get; set; } = new List<string>();

    public static UserOperationResult Success()
    {
        return new UserOperationResult { Succeeded = true };
    }

    public static UserOperationResult Failure(params string[] errors)
    {
        return new UserOperationResult
        {
            Succeeded = false,
            Errors = errors
        };
    }

    public static UserOperationResult Failure(IEnumerable<string> errors)
    {
        return new UserOperationResult
        {
            Succeeded = false,
            Errors = errors
        };
    }
}