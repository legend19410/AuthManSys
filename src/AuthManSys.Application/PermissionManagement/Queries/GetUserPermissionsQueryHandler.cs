using MediatR;
using AuthManSys.Application.Common.Interfaces;

namespace AuthManSys.Application.PermissionManagement.Queries;

public class GetUserPermissionsQueryHandler : IRequestHandler<GetUserPermissionsQuery, object>
{
    private readonly IPermissionService _permissionService;

    public GetUserPermissionsQueryHandler(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    public async Task<object> Handle(GetUserPermissionsQuery request, CancellationToken cancellationToken)
    {
        return await _permissionService.GetUserPermissionsAsync(request.UserId);
    }
}