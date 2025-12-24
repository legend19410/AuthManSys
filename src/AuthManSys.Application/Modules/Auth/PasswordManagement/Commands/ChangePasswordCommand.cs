using MediatR;
using AuthManSys.Application.Common.Models.Responses;

namespace AuthManSys.Application.Modules.Auth.PasswordManagement.Commands;

public record ChangePasswordCommand(
    string Username,
    string CurrentPassword,
    string NewPassword
) : IRequest<ChangePasswordResponse>;