using MediatR;
using AuthManSys.Application.Common.Models.Responses;

namespace AuthManSys.Application.Modules.Auth.TwoFactor.Commands;

public record VerifyTwoFactorCodeCommand(
    string Username,
    string Code
) : IRequest<VerifyTwoFactorCodeResponse>;