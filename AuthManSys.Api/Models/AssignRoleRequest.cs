using System.ComponentModel.DataAnnotations;

namespace AuthManSys.Api.Models;

public class AssignRoleRequest
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string RoleName { get; set; } = string.Empty;
}