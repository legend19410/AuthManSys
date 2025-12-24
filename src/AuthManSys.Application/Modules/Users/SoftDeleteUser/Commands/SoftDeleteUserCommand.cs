using MediatR;
using AuthManSys.Application.Common.Models.Responses;

namespace AuthManSys.Application.Modules.Users.SoftDeleteUser.Commands;

public record SoftDeleteUserCommand(
    string Username,
    string DeletedBy
) : IRequest<SoftDeleteUserResponse>;