using MediatR;

namespace AuthManSys.Application.PermissionManagement.Queries;

public record CheckPermissionQuery(
    string UserId,
    string PermissionName
) : IRequest<bool>;