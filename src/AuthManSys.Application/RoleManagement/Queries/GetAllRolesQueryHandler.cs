using MediatR;
using AuthManSys.Application.Common.Models;
using AuthManSys.Application.Common.Interfaces;

namespace AuthManSys.Application.RoleManagement.Queries;

public class GetAllRolesQueryHandler : IRequestHandler<GetAllRolesQuery, IEnumerable<RoleDto>>
{
    private readonly IIdentityService _identityExtension;

    public GetAllRolesQueryHandler(IIdentityService identityExtension)
    {
        _identityExtension = identityExtension;
    }

    public async Task<IEnumerable<RoleDto>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await _identityExtension.GetAllRolesWithDetailsAsync();
        return roles;
    }
}