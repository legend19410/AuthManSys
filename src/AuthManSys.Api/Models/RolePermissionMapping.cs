using System.ComponentModel.DataAnnotations;

namespace AuthManSys.Api.Models;

public class RolePermissionMappingRequest
{
    [Required]
    public string RoleName { get; set; } = string.Empty;

    [Required]
    public string PermissionName { get; set; } = string.Empty;
}