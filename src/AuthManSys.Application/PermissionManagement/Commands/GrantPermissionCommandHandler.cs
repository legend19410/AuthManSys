using MediatR;
using AuthManSys.Application.Common.Interfaces;

namespace AuthManSys.Application.PermissionManagement.Commands;

public class GrantPermissionCommandHandler : IRequestHandler<GrantPermissionCommand, bool>
{
    private readonly IPermissionService _permissionService;

    public GrantPermissionCommandHandler(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    public async Task<bool> Handle(GrantPermissionCommand request, CancellationToken cancellationToken)
    {
        return await _permissionService.GrantPermissionToRoleByNameAsync(
            request.RoleName,
            request.PermissionName,
            request.CurrentUser);
    }
}