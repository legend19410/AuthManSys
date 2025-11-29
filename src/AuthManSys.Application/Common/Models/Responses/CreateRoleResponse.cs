namespace AuthManSys.Application.Common.Models.Responses;

public class CreateRoleResponse
{
    public bool IsCreated { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? RoleId { get; set; }
    public string? RoleName { get; set; }
    public string? Description { get; set; }
    public DateTime? CreatedAt { get; set; }
}