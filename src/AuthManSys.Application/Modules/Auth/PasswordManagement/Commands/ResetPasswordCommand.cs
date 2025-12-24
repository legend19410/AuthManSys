using MediatR;
using AuthManSys.Application.Common.Models.Responses;

namespace AuthManSys.Application.Modules.Auth.PasswordManagement.Commands;

public record ResetPasswordCommand(
    string Email,
    string Token,
    string NewPassword
) : IRequest<ResetPasswordResponse>;