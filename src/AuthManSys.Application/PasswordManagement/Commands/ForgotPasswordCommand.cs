using MediatR;
using AuthManSys.Application.Common.Models.Responses;

namespace AuthManSys.Application.PasswordManagement.Commands;

public record ForgotPasswordCommand(
    string Email
) : IRequest<ForgotPasswordResponse>;