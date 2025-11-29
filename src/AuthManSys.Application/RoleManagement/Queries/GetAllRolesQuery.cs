using MediatR;
using AuthManSys.Application.Common.Models;

namespace AuthManSys.Application.RoleManagement.Queries;

public record GetAllRolesQuery() : IRequest<IEnumerable<RoleDto>>;