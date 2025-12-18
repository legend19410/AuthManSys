using MediatR;
using AuthManSys.Application.Common.Interfaces;

namespace AuthManSys.Application.PermissionManagement.Commands;

public class BulkRevokePermissionsCommandHandler : IRequestHandler<BulkRevokePermissionsCommand, object>
{
    private readonly IPermissionService _permissionService;

    public BulkRevokePermissionsCommandHandler(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    public async Task<object> Handle(BulkRevokePermissionsCommand request, CancellationToken cancellationToken)
    {
        return await _permissionService.BulkRevokePermissionsAsync(request.Permissions.ToList());
    }
}