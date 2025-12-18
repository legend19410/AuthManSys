using MediatR;
using AuthManSys.Application.Common.Interfaces;

namespace AuthManSys.Application.PermissionManagement.Queries;

public class CheckPermissionQueryHandler : IRequestHandler<CheckPermissionQuery, bool>
{
    private readonly IPermissionService _permissionService;

    public CheckPermissionQueryHandler(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    public async Task<bool> Handle(CheckPermissionQuery request, CancellationToken cancellationToken)
    {
        return await _permissionService.UserHasPermissionAsync(request.UserId, request.PermissionName);
    }
}