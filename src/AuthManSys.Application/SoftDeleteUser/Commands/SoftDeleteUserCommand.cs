using MediatR;
using AuthManSys.Application.Common.Models.Responses;

namespace AuthManSys.Application.SoftDeleteUser.Commands;

public record SoftDeleteUserCommand(
    string Username,
    string DeletedBy
) : IRequest<SoftDeleteUserResponse>;