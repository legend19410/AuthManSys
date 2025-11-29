using MediatR;
using AuthManSys.Application.Common.Models;

namespace AuthManSys.Application.UserInformation.Queries;

public record GetUserInformationByUsernameQuery(string Username) : IRequest<UserInformationResponse>;