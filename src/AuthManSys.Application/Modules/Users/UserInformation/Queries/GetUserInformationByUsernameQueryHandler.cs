using MediatR;
using AuthManSys.Application.Common.Models;
using AuthManSys.Application.Common.Interfaces;

namespace AuthManSys.Application.Modules.Users.UserInformation.Queries;

public class GetUserInformationByUsernameQueryHandler : IRequestHandler<GetUserInformationByUsernameQuery, UserInformationResponse>
{
    private readonly IUserRepository _userRepository;

    public GetUserInformationByUsernameQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserInformationResponse> Handle(GetUserInformationByUsernameQuery request, CancellationToken cancellationToken)
    {
        var result = await _userRepository.GetUserInformationByUsernameAsync(request.Username, cancellationToken);

        if (result == null)
        {
            throw new InvalidOperationException($"User with username '{request.Username}' not found or is inactive.");
        }

        return result;
    }
}