namespace AuthManSys.Domain.Entities;

public class RolePermission
{
    public int Id { get; private set; }
    public Guid RoleId { get; private set; }
    public int PermissionId { get; private set; }
    public DateTime GrantedAt { get; private set; }
    public string? GrantedBy { get; private set; }

    // Navigation properties for domain logic
    public Role Role { get; private set; } = null!;
    public Permission Permission { get; private set; } = null!;

    private RolePermission() { } // For ORM

    public RolePermission(
        Guid roleId,
        int permissionId,
        string? grantedBy = null
    )
    {
        RoleId = roleId;
        PermissionId = permissionId;
        GrantedAt = DateTime.UtcNow;
        GrantedBy = grantedBy;
    }

    // Domain behaviors
    public bool IsGrantedBy(string userId)
        => GrantedBy == userId;

    public bool IsForRole(Guid roleId)
        => RoleId == roleId;

    public bool IsForPermission(int permissionId)
        => PermissionId == permissionId;
}