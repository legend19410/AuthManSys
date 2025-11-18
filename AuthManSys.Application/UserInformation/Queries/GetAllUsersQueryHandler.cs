using MediatR;
using AuthManSys.Application.Common.Models;
using AuthManSys.Application.Common.Interfaces;

namespace AuthManSys.Application.UserInformation.Queries;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, PagedResponse<UserInformationResponse>>
{
    private readonly IAuthManSysDbContext _dbContext;

    public GetAllUsersQueryHandler(IAuthManSysDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResponse<UserInformationResponse>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        return await _dbContext.GetAllUsersAsync(request.Request, cancellationToken);
    }
}