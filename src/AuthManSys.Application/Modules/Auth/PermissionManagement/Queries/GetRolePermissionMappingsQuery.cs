using MediatR;

namespace AuthManSys.Application.Modules.Auth.PermissionManagement.Queries;

public record GetRolePermissionMappingsQuery : IRequest<object>;