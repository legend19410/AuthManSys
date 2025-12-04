using System.ComponentModel.DataAnnotations;

namespace AuthManSys.Api.Models;

public class RemoveRoleRequest
{
    [Required]
    public int UserId { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string RoleName { get; set; } = string.Empty;
}