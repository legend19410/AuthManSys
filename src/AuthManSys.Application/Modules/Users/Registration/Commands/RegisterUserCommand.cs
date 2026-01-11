using MediatR;
using AuthManSys.Application.Common.Models.Responses;

namespace AuthManSys.Application.Modules.Users.Registration.Commands;

public record RegisterUserCommand(
    string Username,
    string Email,
    string Password,
    string FirstName,
    string LastName) : IRequest<RegisterResponse>;