using MediatR;

namespace AuthManSys.Application.PermissionManagement.Queries;

public record GetAllPermissionsQuery : IRequest<object>;