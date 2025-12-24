using MediatR;
using AuthManSys.Application.Common.Models.Responses;

namespace AuthManSys.Application.Modules.Auth.TwoFactor.Commands;

public record EnableTwoFactorCommand(
    int UserId,
    bool Enable
) : IRequest<EnableTwoFactorResponse>;