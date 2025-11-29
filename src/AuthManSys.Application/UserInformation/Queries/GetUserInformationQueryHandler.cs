using MediatR;
using AuthManSys.Application.Common.Models;
using AuthManSys.Application.Common.Interfaces;

namespace AuthManSys.Application.UserInformation.Queries;

public class GetUserInformationQueryHandler : IRequestHandler<GetUserInformationQuery, UserInformationResponse>
{
    private readonly IAuthManSysDbContext _dbContext;

    public GetUserInformationQueryHandler(IAuthManSysDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserInformationResponse> Handle(GetUserInformationQuery request, CancellationToken cancellationToken)
    {
        var result = await _dbContext.GetUserInformationAsync(request.UserId, cancellationToken);

        if (result == null)
        {
            throw new InvalidOperationException($"User with ID {request.UserId} not found or is inactive.");
        }

        return result;
    }
}