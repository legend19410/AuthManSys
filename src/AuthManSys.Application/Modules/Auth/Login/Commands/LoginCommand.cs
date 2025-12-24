using MediatR;
using AuthManSys.Application.Common.Models;
using AuthManSys.Application.Common.Models.Responses;

namespace AuthManSys.Application.Modules.Auth.Login.Commands;

public record LoginCommand(
    string Username,
    string Password,
    bool RememberMe
) : IRequest<LoginResponse>;
