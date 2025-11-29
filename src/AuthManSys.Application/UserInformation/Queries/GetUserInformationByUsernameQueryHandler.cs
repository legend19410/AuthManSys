using MediatR;
using AuthManSys.Application.Common.Models;
using AuthManSys.Application.Common.Interfaces;

namespace AuthManSys.Application.UserInformation.Queries;

public class GetUserInformationByUsernameQueryHandler : IRequestHandler<GetUserInformationByUsernameQuery, UserInformationResponse>
{
    private readonly IAuthManSysDbContext _dbContext;

    public GetUserInformationByUsernameQueryHandler(IAuthManSysDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserInformationResponse> Handle(GetUserInformationByUsernameQuery request, CancellationToken cancellationToken)
    {
        var result = await _dbContext.GetUserInformationByUsernameAsync(request.Username, cancellationToken);

        if (result == null)
        {
            throw new InvalidOperationException($"User with username '{request.Username}' not found or is inactive.");
        }

        return result;
    }
}