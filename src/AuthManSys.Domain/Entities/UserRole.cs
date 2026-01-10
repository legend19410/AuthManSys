namespace AuthManSys.Domain.Entities;

public class UserRole
{
    public int UserId { get; private set; }
    public Guid RoleId { get; private set; }
    public DateTime AssignedAt { get; private set; }
    public int? AssignedBy { get; private set; }

    // Navigation properties for domain logic
    public User User { get; private set; } = null!;
    public Role Role { get; private set; } = null!;

    private UserRole() { } // For ORM

    public UserRole(
        int userId,
        Guid roleId,
        int? assignedBy = null
    )
    {
        UserId = userId;
        RoleId = roleId;
        AssignedAt = DateTime.UtcNow;
        AssignedBy = assignedBy;
    }

    // Domain behaviors
    public bool IsAssignedBy(int userId)
        => AssignedBy == userId;

    public bool IsForUser(int userId)
        => UserId == userId;

    public bool IsForRole(Guid roleId)
        => RoleId == roleId;

    public bool WasAssignedAfter(DateTime date)
        => AssignedAt > date;
}