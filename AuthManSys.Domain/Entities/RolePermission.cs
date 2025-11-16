using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace AuthManSys.Domain.Entities;

public class RolePermission
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string RoleId { get; set; } = string.Empty;

    [Required]
    public int PermissionId { get; set; }

    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;

    public string? GrantedBy { get; set; }

    // Navigation properties
    public virtual IdentityRole Role { get; set; } = null!;
    public virtual Permission Permission { get; set; } = null!;
}