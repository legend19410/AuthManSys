using AuthManSys.Application.Common.Models;
using AuthManSys.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthManSys.Application.Common.Interfaces;

public interface IAuthManSysDbContext
{
    DbSet<AuthManSys.Domain.Entities.RefreshToken> RefreshTokens { get; set; }
    DbSet<UserActivityLog> UserActivityLogs { get; set; }
    Task<UserInformationResponse?> GetUserInformationAsync(int userId, CancellationToken cancellationToken = default);
    Task<UserInformationResponse?> GetUserInformationByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<PagedResponse<UserInformationResponse>> GetAllUsersAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}