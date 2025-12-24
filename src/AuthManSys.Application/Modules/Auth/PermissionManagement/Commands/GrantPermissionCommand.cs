using MediatR;

namespace AuthManSys.Application.Modules.Auth.PermissionManagement.Commands;

public record GrantPermissionCommand(
    string RoleName,
    string PermissionName,
    string? CurrentUser = null
) : IRequest<bool>;