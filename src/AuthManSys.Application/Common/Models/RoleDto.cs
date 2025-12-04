namespace AuthManSys.Application.Common.Models;

public class RoleDto
{
    public string Id { get; }
    public string Name { get; }
    public string? NormalizedName { get; }
    public string? Description { get; }

    public RoleDto(string id, string name, string? normalizedName = null, string? description = null)
    {
        Id = id;
        Name = name;
        NormalizedName = normalizedName;
        Description = description;
    }
}