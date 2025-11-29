namespace AuthManSys.Application.Common.Models;

public class RoleDto
{
    public string Id { get; }
    public string Name { get; }
    public string? NormalizedName { get; }

    public RoleDto(string id, string name, string? normalizedName = null)
    {
        Id = id;
        Name = name;
        NormalizedName = normalizedName;
    }
}