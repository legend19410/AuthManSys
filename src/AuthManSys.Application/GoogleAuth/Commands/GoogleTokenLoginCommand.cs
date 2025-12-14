using MediatR;
using AuthManSys.Application.Common.Models.Responses;

namespace AuthManSys.Application.GoogleAuth.Commands;

public record GoogleTokenLoginCommand(
    string IdToken,
    string Username
) : IRequest<LoginResponse>;