using MediatR;
using AuthManSys.Application.Common.Models;

namespace AuthManSys.Application.Modules.Auth.PermissionManagement.Commands;

public record BulkRevokePermissionsCommand(
    IList<RolePermissionMappingRequest> Permissions
) : IRequest<object>;