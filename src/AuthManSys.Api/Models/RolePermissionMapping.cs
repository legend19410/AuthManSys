using System.ComponentModel.DataAnnotations;

namespace AuthManSys.Api.Models;

public class RolePermissionMapping
{
    [Required]
    public string RoleName { get; set; } = string.Empty;

    [Required]
    public string PermissionName { get; set; } = string.Empty;
}