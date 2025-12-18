using MediatR;

namespace AuthManSys.Application.PermissionManagement.Commands;

public record GrantPermissionCommand(
    string RoleName,
    string PermissionName,
    string? CurrentUser = null
) : IRequest<bool>;