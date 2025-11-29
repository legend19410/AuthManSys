using MediatR;
using AuthManSys.Application.Common.Models.Responses;

namespace AuthManSys.Application.Security.Commands.RefreshToken;

public record RefreshTokenCommand(
    string RefreshToken
) : IRequest<RefreshTokenResponse>;