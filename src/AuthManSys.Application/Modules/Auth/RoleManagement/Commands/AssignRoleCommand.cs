using MediatR;
using AuthManSys.Application.Common.Models.Responses;

namespace AuthManSys.Application.Modules.Auth.RoleManagement.Commands;

public record AssignRoleCommand(
    int UserId,
    string RoleName,
    int? AssignedBy = null
) : IRequest<AssignRoleResponse>;