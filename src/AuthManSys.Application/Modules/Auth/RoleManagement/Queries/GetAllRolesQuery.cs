using MediatR;
using AuthManSys.Application.Common.Models;

namespace AuthManSys.Application.Modules.Auth.RoleManagement.Queries;

public record GetAllRolesQuery() : IRequest<IEnumerable<RoleDto>>;