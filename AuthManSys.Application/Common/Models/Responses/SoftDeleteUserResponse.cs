namespace AuthManSys.Application.Common.Models.Responses;

public class SoftDeleteUserResponse
{
    public bool IsDeleted { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Username { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}