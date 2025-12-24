using MediatR;
using AuthManSys.Application.Common.Models;

namespace AuthManSys.Application.Modules.Users.UserInformation.Queries;

public record GetUserInformationByUsernameQuery(string Username) : IRequest<UserInformationResponse>;