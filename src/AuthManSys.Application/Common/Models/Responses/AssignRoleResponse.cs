namespace AuthManSys.Application.Common.Models.Responses;

public class AssignRoleResponse
{
    public bool IsAssigned { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string? RoleName { get; set; }
    public DateTime? AssignedAt { get; set; }
}