using MediatR;
using AuthManSys.Application.Common.Interfaces;
using AuthManSys.Application.Common.Models;

namespace AuthManSys.Application.Modules.Auth.PermissionManagement.Commands;

public class BulkRevokePermissionsCommandHandler : IRequestHandler<BulkRevokePermissionsCommand, object>
{
    private readonly IPermissionRepository _permissionRepository;
    private readonly IRoleRepository _roleRepository;

    public BulkRevokePermissionsCommandHandler(
        IPermissionRepository permissionRepository,
        IRoleRepository roleRepository)
    {
        _permissionRepository = permissionRepository;
        _roleRepository = roleRepository;
    }

    public async Task<object> Handle(BulkRevokePermissionsCommand request, CancellationToken cancellationToken)
    {
        // Convert to RolePermissionMapping format required by repository
        var mappings = new List<RolePermissionMapping>();

        foreach (var permission in request.Permissions)
        {
            mappings.Add(new RolePermissionMapping
            {
                RoleName = permission.RoleName,
                PermissionName = permission.PermissionName
            });
        }

        return await _permissionRepository.BulkRemovePermissionsFromRolesAsync(mappings);
    }
}