using MediatR;
using AuthManSys.Application.Common.Models.Responses;

namespace AuthManSys.Application.TwoFactor.Commands;

public record SendTwoFactorCodeCommand(
    string Username
) : IRequest<SendTwoFactorCodeResponse>;