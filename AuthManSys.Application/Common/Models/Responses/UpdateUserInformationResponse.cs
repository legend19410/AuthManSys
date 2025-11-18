namespace AuthManSys.Application.Common.Models.Responses;

public class UpdateUserInformationResponse
{
    public bool IsUpdated { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Username { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
}