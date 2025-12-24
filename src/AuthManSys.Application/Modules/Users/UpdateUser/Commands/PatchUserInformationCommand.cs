using MediatR;
using AuthManSys.Application.Common.Models.Responses;

namespace AuthManSys.Application.Modules.Users.UpdateUser.Commands;

public record PatchUserInformationCommand(
    string Username,
    string? FirstName,
    string? LastName,
    string? Email,
    bool UpdateFirstName,
    bool UpdateLastName,
    bool UpdateEmail
) : IRequest<UpdateUserInformationResponse>;