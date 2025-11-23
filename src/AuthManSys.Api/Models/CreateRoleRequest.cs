using System.ComponentModel.DataAnnotations;

namespace AuthManSys.Api.Models;

public class CreateRoleRequest
{
    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string RoleName { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Description { get; set; }
}