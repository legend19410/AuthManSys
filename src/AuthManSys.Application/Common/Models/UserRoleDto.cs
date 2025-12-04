using System;

namespace AuthManSys.Application.Common.Models;

public class UserRoleDto
{
    public string RoleId { get; }
    public string RoleName { get; }
    public string? RoleDescription { get; }
    public DateTime AssignedAt { get; }

    public UserRoleDto(string roleId, string roleName, string? roleDescription, DateTime assignedAt)
    {
        RoleId = roleId;
        RoleName = roleName;
        RoleDescription = roleDescription;
        AssignedAt = assignedAt;
    }
}