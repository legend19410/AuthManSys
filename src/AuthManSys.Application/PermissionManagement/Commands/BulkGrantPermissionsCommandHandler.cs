using MediatR;
using AuthManSys.Application.Common.Interfaces;

namespace AuthManSys.Application.PermissionManagement.Commands;

public class BulkGrantPermissionsCommandHandler : IRequestHandler<BulkGrantPermissionsCommand, object>
{
    private readonly IPermissionService _permissionService;

    public BulkGrantPermissionsCommandHandler(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    public async Task<object> Handle(BulkGrantPermissionsCommand request, CancellationToken cancellationToken)
    {
        return await _permissionService.BulkGrantPermissionsAsync(request.Permissions.ToList(), request.CurrentUser);
    }
}