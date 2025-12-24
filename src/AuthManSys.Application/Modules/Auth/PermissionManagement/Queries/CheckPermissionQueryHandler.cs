using MediatR;
using AuthManSys.Application.Common.Interfaces;

namespace AuthManSys.Application.Modules.Auth.PermissionManagement.Queries;

public class CheckPermissionQueryHandler : IRequestHandler<CheckPermissionQuery, bool>
{
    private readonly IPermissionRepository _permissionRepository;

    public CheckPermissionQueryHandler(IPermissionRepository permissionRepository)
    {
        _permissionRepository = permissionRepository;
    }

    public async Task<bool> Handle(CheckPermissionQuery request, CancellationToken cancellationToken)
    {
        return await _permissionRepository.UserHasPermissionAsync(request.UserId, request.PermissionName);
    }
}