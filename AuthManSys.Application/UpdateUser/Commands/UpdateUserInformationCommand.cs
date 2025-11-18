using MediatR;
using AuthManSys.Application.Common.Models.Responses;

namespace AuthManSys.Application.UpdateUser.Commands;

public record UpdateUserInformationCommand(
    string Username,
    string? FirstName,
    string? LastName,
    string? Email
) : IRequest<UpdateUserInformationResponse>;