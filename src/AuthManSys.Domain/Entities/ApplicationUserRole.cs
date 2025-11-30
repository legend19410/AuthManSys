using Microsoft.AspNetCore.Identity;

namespace AuthManSys.Domain.Entities;

public class ApplicationUserRole : IdentityUserRole<string>
{
    public DateTime AssignedAt { get; set; }
    public int? AssignedBy { get; set; }
}