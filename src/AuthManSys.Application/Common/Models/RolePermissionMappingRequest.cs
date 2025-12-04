namespace AuthManSys.Application.Common.Models;

public class RolePermissionMappingRequest
{
    public string RoleName { get; set; } = string.Empty;
    public string PermissionName { get; set; } = string.Empty;
}