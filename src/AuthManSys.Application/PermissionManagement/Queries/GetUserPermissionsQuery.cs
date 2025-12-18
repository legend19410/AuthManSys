using MediatR;

namespace AuthManSys.Application.PermissionManagement.Queries;

public record GetUserPermissionsQuery(
    string UserId
) : IRequest<object>;