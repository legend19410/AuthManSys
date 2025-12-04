namespace AuthManSys.Application.Common.Models;

public class RolePermissionMappingDto
{
    public string RoleId { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string? RoleDescription { get; set; }
    public List<PermissionDto> Permissions { get; set; } = new List<PermissionDto>();
}