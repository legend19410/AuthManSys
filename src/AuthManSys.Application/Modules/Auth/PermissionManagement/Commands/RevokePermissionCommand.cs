using MediatR;

namespace AuthManSys.Application.Modules.Auth.PermissionManagement.Commands;

public record RevokePermissionCommand(
    string RoleName,
    string PermissionName
) : IRequest<bool>;