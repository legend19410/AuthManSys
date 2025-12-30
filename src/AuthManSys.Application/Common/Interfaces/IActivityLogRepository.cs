using AuthManSys.Domain.Entities;
using AuthManSys.Domain.Enums;

namespace AuthManSys.Application.Common.Interfaces
{
    public interface IActivityLogRepository
    {
        Task LogActivityAsync(
            int? userId,
            ActivityEventType eventType,
            string? description = null,
            string? ipAddress = null,
            string? device = null,
            string? platform = null,
            string? location = null,
            object? metadata = null,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<UserActivityLog>> GetUserActivitiesAsync(
            int userId,
            int pageNumber = 1,
            int pageSize = 50,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<UserActivityLog>> GetLastNUserActivitiesAsync(
            int userId,
            int count,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<UserActivityLog>> GetActivitiesByEventTypeAsync(
            ActivityEventType eventType,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int pageNumber = 1,
            int pageSize = 50,
            CancellationToken cancellationToken = default);

        Task<int> GetActivityCountAsync(
            int? userId = null,
            ActivityEventType? eventType = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            CancellationToken cancellationToken = default);
    }
}