using MediatR;
using AuthManSys.Application.Common.Interfaces;

namespace AuthManSys.Application.PermissionManagement.Commands;

public class RevokePermissionCommandHandler : IRequestHandler<RevokePermissionCommand, bool>
{
    private readonly IPermissionService _permissionService;

    public RevokePermissionCommandHandler(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    public async Task<bool> Handle(RevokePermissionCommand request, CancellationToken cancellationToken)
    {
        return await _permissionService.RevokePermissionFromRoleByNameAsync(
            request.RoleName,
            request.PermissionName);
    }
}