using MediatR;

namespace AuthManSys.Application.Modules.Auth.PermissionManagement.Queries;

public record GetUserPermissionsQuery(
    string UserId
) : IRequest<object>;