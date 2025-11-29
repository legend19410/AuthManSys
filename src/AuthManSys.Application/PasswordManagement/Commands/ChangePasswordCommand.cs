using MediatR;
using AuthManSys.Application.Common.Models.Responses;

namespace AuthManSys.Application.PasswordManagement.Commands;

public record ChangePasswordCommand(
    string Username,
    string CurrentPassword,
    string NewPassword
) : IRequest<ChangePasswordResponse>;