using MediatR;

namespace AuthManSys.Application.Modules.Auth.PermissionManagement.Queries;

public record CheckPermissionQuery(
    string UserId,
    string PermissionName
) : IRequest<bool>;