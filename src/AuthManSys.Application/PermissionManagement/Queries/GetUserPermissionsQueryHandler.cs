using MediatR;
using AuthManSys.Application.Common.Interfaces;

namespace AuthManSys.Application.PermissionManagement.Queries;

public class GetUserPermissionsQueryHandler : IRequestHandler<GetUserPermissionsQuery, object>
{
    private readonly IPermissionRepository _permissionRepository;

    public GetUserPermissionsQueryHandler(IPermissionRepository permissionRepository)
    {
        _permissionRepository = permissionRepository;
    }

    public async Task<object> Handle(GetUserPermissionsQuery request, CancellationToken cancellationToken)
    {
        var permissions = await _permissionRepository.GetUserPermissionsAsync(request.UserId);
        return permissions;
    }
}