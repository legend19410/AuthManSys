using MediatR;
using AuthManSys.Application.Common.Models;

namespace AuthManSys.Application.UserInformation.Queries;

public record GetUserInformationQuery(int UserId) : IRequest<UserInformationResponse>;