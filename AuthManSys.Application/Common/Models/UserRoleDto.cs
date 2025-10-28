using System;

namespace AuthManSys.Application.Common.Models;

public class UserRoleDto
{
    public int RoleId { get; }
    public string RoleName { get; }
    public string? RoleDescription { get; }
    public DateTime AssignedAt { get; }

    public UserRoleDto(int roleId, string roleName, string? roleDescription, DateTime assignedAt)
    {
        RoleId = roleId;
        RoleName = roleName;
        RoleDescription = roleDescription;
        AssignedAt = assignedAt;
    }
}