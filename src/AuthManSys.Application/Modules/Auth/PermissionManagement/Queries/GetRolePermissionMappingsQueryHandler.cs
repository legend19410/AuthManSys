using MediatR;
using AuthManSys.Application.Common.Interfaces;

namespace AuthManSys.Application.Modules.Auth.PermissionManagement.Queries;

public class GetRolePermissionMappingsQueryHandler : IRequestHandler<GetRolePermissionMappingsQuery, object>
{
    private readonly IPermissionRepository _permissionRepository;

    public GetRolePermissionMappingsQueryHandler(IPermissionRepository permissionRepository)
    {
        _permissionRepository = permissionRepository;
    }

    public async Task<object> Handle(GetRolePermissionMappingsQuery request, CancellationToken cancellationToken)
    {
        var mappings = await _permissionRepository.GetDetailedRolePermissionMappingsAsync();

        // Transform to frontend expected format: array of objects with roleName and permissions
        return mappings.Select(mapping => new
        {
            roleName = mapping.Key,
            permissions = mapping.Value.Select(permissionName => new { name = permissionName }).ToList()
        }).ToArray();
    }
}