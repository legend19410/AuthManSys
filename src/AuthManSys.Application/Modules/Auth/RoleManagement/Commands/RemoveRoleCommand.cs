using MediatR;
using AuthManSys.Application.Common.Models.Responses;

namespace AuthManSys.Application.Modules.Auth.RoleManagement.Commands;

public record RemoveRoleCommand(
    int UserId,
    string RoleName
) : IRequest<RemoveRoleResponse>;