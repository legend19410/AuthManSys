using MediatR;

namespace AuthManSys.Application.PermissionManagement.Queries;

public record GetRolePermissionMappingsQuery : IRequest<object>;