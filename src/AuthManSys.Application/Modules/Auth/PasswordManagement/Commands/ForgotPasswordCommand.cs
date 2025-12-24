using MediatR;
using AuthManSys.Application.Common.Models.Responses;

namespace AuthManSys.Application.Modules.Auth.PasswordManagement.Commands;

public record ForgotPasswordCommand(
    string Email
) : IRequest<ForgotPasswordResponse>;