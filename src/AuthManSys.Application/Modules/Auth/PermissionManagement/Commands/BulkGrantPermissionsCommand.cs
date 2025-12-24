using MediatR;
using AuthManSys.Application.Common.Models;

namespace AuthManSys.Application.Modules.Auth.PermissionManagement.Commands;

public record BulkGrantPermissionsCommand(
    IList<RolePermissionMappingRequest> Permissions,
    string? CurrentUser = null
) : IRequest<object>;