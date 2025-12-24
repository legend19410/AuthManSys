using MediatR;
using AuthManSys.Application.Common.Models.Responses;

namespace AuthManSys.Application.Modules.Auth.RefreshToken.Commands;

public record RefreshTokenCommand(
    string RefreshToken
) : IRequest<RefreshTokenResponse>;