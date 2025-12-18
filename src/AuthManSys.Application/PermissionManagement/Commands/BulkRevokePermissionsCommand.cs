using MediatR;
using AuthManSys.Application.Common.Models;

namespace AuthManSys.Application.PermissionManagement.Commands;

public record BulkRevokePermissionsCommand(
    IList<RolePermissionMappingRequest> Permissions
) : IRequest<object>;