using MediatR;

namespace AuthManSys.Application.PermissionManagement.Commands;

public record RevokePermissionCommand(
    string RoleName,
    string PermissionName
) : IRequest<bool>;