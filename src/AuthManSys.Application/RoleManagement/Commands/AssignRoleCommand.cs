using MediatR;
using AuthManSys.Application.Common.Models.Responses;

namespace AuthManSys.Application.RoleManagement.Commands;

public record AssignRoleCommand(
    string UserId,
    string RoleName,
    int? AssignedBy = null
) : IRequest<AssignRoleResponse>;