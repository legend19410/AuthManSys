namespace AuthManSys.Domain.Entities;

public class Permission
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    private readonly List<RolePermission> _rolePermissions = new();
    public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

    public Permission() { } // For AutoMapper and ORM

    public Permission(
        string name,
        string? description = null,
        string? category = null
    )
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Permission name is required", nameof(name));

        Id = 0; // Assigned by persistence
        Name = name;
        Description = description;
        Category = category;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    // Domain behaviors (important)
    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void UpdateDetails(string name, string? description, string? category)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Permission name is required", nameof(name));

        Name = name;
        Description = description;
        Category = category;
    }
}