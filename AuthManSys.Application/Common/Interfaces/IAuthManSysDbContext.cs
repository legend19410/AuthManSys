using AuthManSys.Application.Common.Models;


namespace AuthManSys.Application.Common.Interfaces;

public interface IAuthManSysDbContext
{
    Task<UserInformationResponse?> GetUserInformationAsync(int userId, CancellationToken cancellationToken = default);
    Task<PagedResponse<UserInformationResponse>> GetAllUsersAsync(PagedRequest request, CancellationToken cancellationToken = default);
}