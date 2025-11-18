using MediatR;
using AuthManSys.Application.Common.Models;

namespace AuthManSys.Application.UserInformation.Queries;

public record GetAllUsersQuery(PagedRequest Request) : IRequest<PagedResponse<UserInformationResponse>>;