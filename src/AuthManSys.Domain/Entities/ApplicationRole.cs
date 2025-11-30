using Microsoft.AspNetCore.Identity;

namespace AuthManSys.Domain.Entities;

public class ApplicationRole : IdentityRole
{
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? CreatedBy { get; set; }
}