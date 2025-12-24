using MediatR;
using AuthManSys.Application.Common.Models;

namespace AuthManSys.Application.Modules.Users.UserInformation.Queries;

public record GetAllUsersQuery(PagedRequest Request) : IRequest<PagedResponse<UserInformationResponse>>;