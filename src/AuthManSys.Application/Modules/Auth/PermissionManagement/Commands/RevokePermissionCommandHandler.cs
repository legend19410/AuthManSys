using MediatR;
using AuthManSys.Application.Common.Interfaces;

namespace AuthManSys.Application.Modules.Auth.PermissionManagement.Commands;

public class RevokePermissionCommandHandler : IRequestHandler<RevokePermissionCommand, bool>
{
    private readonly IPermissionRepository _permissionRepository;
    private readonly IRoleRepository _roleRepository;

    public RevokePermissionCommandHandler(
        IPermissionRepository permissionRepository,
        IRoleRepository roleRepository)
    {
        _permissionRepository = permissionRepository;
        _roleRepository = roleRepository;
    }

    public async Task<bool> Handle(RevokePermissionCommand request, CancellationToken cancellationToken)
    {
        // Get the role first to ensure it exists
        var role = await _roleRepository.GetByNameAsync(request.RoleName);
        if (role == null)
        {
            return false;
        }

        // Check if the role currently has the permission
        var hasPermission = await _permissionRepository.RoleHasPermissionAsync(role.Id, request.PermissionName);
        if (!hasPermission)
        {
            return false; // Permission was not assigned to this role
        }

        // Revoke the permission
        await _permissionRepository.RemovePermissionFromRoleAsync(role.Id, request.PermissionName);
        return true;
    }
}