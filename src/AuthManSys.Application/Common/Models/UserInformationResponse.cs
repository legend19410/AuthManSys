using System;

namespace AuthManSys.Application.Common.Models;

public class UserInformationResponse
{
    public int Id { get; }
    public string Username { get; }
    public string Email { get; }
    public string? FirstName { get; }
    public string? LastName { get; }
    public bool IsActive { get; }
    public DateTime CreatedAt { get; }
    public DateTime? LastLoginAt { get; }
    public bool IsTwoFactorEnabled { get; }
    public IReadOnlyList<UserRoleDto> Roles { get; }

    public UserInformationResponse(
        int id,
        string username,
        string email,
        string? firstName,
        string? lastName,
        bool isActive,
        DateTime createdAt,
        DateTime? lastLoginAt,
        bool isTwoFactorEnabled,
        IReadOnlyList<UserRoleDto> roles)
    {
        Id = id;
        Username = username;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        IsActive = isActive;
        CreatedAt = createdAt;
        LastLoginAt = lastLoginAt;
        IsTwoFactorEnabled = isTwoFactorEnabled;
        Roles = roles;
    }
}