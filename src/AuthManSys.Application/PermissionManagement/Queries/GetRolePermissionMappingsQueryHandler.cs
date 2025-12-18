using MediatR;
using AuthManSys.Application.Common.Interfaces;

namespace AuthManSys.Application.PermissionManagement.Queries;

public class GetRolePermissionMappingsQueryHandler : IRequestHandler<GetRolePermissionMappingsQuery, object>
{
    private readonly IPermissionService _permissionService;

    public GetRolePermissionMappingsQueryHandler(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    public async Task<object> Handle(GetRolePermissionMappingsQuery request, CancellationToken cancellationToken)
    {
        return await _permissionService.GetDetailedRolePermissionMappingsAsync();
    }
}