using MediatR;
using AuthManSys.Application.Common.Models.Responses;

namespace AuthManSys.Application.RoleManagement.Commands;

public record RemoveRoleCommand(
    string UserId,
    string RoleName
) : IRequest<RemoveRoleResponse>;