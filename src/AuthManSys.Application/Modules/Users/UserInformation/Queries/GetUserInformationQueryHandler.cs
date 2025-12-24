using MediatR;
using AuthManSys.Application.Common.Models;
using AuthManSys.Application.Common.Interfaces;

namespace AuthManSys.Application.Modules.Users.UserInformation.Queries;

public class GetUserInformationQueryHandler : IRequestHandler<GetUserInformationQuery, UserInformationResponse>
{
    private readonly IUserRepository _userRepository;

    public GetUserInformationQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserInformationResponse> Handle(GetUserInformationQuery request, CancellationToken cancellationToken)
    {
        var result = await _userRepository.GetUserInformationAsync(request.UserId, cancellationToken);

        if (result == null)
        {
            throw new InvalidOperationException($"User with ID {request.UserId} not found or is inactive.");
        }

        return result;
    }
}