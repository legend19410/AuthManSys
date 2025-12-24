using MediatR;
using AuthManSys.Application.Common.Models.Responses;

namespace AuthManSys.Application.Modules.Auth.RoleManagement.Commands;

public record CreateRoleCommand(
    string RoleName,
    string? Description
) : IRequest<CreateRoleResponse>;