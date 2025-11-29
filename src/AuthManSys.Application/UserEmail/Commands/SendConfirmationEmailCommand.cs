using MediatR;
using AuthManSys.Application.Common.Models.Responses;

namespace AuthManSys.Application.UserEmail.Commands;

public record SendConfirmationEmailCommand(
    string Username
) : IRequest<SendEmailResponse>;