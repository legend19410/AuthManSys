using MediatR;
using AuthManSys.Application.Common.Interfaces;

namespace AuthManSys.Application.PermissionManagement.Commands;

public class GrantPermissionCommandHandler : IRequestHandler<GrantPermissionCommand, bool>
{
    private readonly IPermissionRepository _permissionRepository;
    private readonly IRoleRepository _roleRepository;

    public GrantPermissionCommandHandler(
        IPermissionRepository permissionRepository,
        IRoleRepository roleRepository)
    {
        _permissionRepository = permissionRepository;
        _roleRepository = roleRepository;
    }

    public async Task<bool> Handle(GrantPermissionCommand request, CancellationToken cancellationToken)
    {
        // Get the role first to ensure it exists
        var role = await _roleRepository.GetByNameAsync(request.RoleName);
        if (role == null)
        {
            return false;
        }

        // Check if the permission exists
        var permissionExists = await _permissionRepository.ExistsAsync(request.PermissionName);
        if (!permissionExists)
        {
            return false;
        }

        // Check if the role already has the permission
        var alreadyHasPermission = await _permissionRepository.RoleHasPermissionAsync(role.Id, request.PermissionName);
        if (alreadyHasPermission)
        {
            return false; // Permission was already assigned (not newly granted)
        }

        // Grant the permission
        await _permissionRepository.AssignPermissionToRoleAsync(role.Id, request.PermissionName);
        return true;
    }
}