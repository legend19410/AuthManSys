using MediatR;
using AuthManSys.Application.Common.Models.Responses;

namespace AuthManSys.Application.Modules.Users.UpdateUser.Commands;

public record UpdateUserInformationCommand(
    string Username,
    string? FirstName,
    string? LastName,
    string? Email
) : IRequest<UpdateUserInformationResponse>;