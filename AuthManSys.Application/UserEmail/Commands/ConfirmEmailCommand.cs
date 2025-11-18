using MediatR;
using AuthManSys.Application.Common.Models.Responses;

namespace AuthManSys.Application.UserEmail.Commands;

public record ConfirmEmailCommand(
    string Username,
    string Token
) : IRequest<ConfirmEmailResponse>;