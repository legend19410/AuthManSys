namespace AuthManSys.Application.Common.Models.Responses;

public class RemoveRoleResponse
{
    public bool IsRemoved { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string? RoleName { get; set; }
    public DateTime? RemovedAt { get; set; }
}