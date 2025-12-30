using System.Text.Json;
using AuthManSys.Application.Common.Interfaces;
using AuthManSys.Application.Common.Helpers;
using AuthManSys.Domain.Entities;
using AuthManSys.Domain.Enums;
using AuthManSys.Infrastructure.Database.DbContext;
using Microsoft.EntityFrameworkCore;

namespace AuthManSys.Infrastructure.Database.Repositories
{
    public class ActivityLogRepository : IActivityLogRepository
    {
        private readonly AuthManSysDbContext _context;

        public ActivityLogRepository(AuthManSysDbContext context)
        {
            _context = context;
        }

        public async Task LogActivityAsync(
            int? userId,
            ActivityEventType eventType,
            string? description = null,
            string? ipAddress = null,
            string? device = null,
            string? platform = null,
            string? location = null,
            object? metadata = null,
            CancellationToken cancellationToken = default)
        {
            var activityLog = new UserActivityLog
            {
                UserId = userId,
                EventType = eventType,
                EventTag = ActivityEventTagMapper.GetEventTag(eventType),
                Description = description,
                Timestamp = JamaicaTimeHelper.Now,
                IPAddress = ipAddress,
                Device = device,
                Platform = platform,
                Location = location,
                Metadata = metadata != null ? JsonSerializer.Serialize(metadata) : null
            };

            _context.UserActivityLogs.Add(activityLog);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<IEnumerable<UserActivityLog>> GetUserActivitiesAsync(
            int userId,
            int pageNumber = 1,
            int pageSize = 50,
            CancellationToken cancellationToken = default)
        {
            return await _context.UserActivityLogs
                .Where(log => log.UserId == userId)
                .OrderByDescending(log => log.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<UserActivityLog>> GetLastNUserActivitiesAsync(
            int userId,
            int count,
            CancellationToken cancellationToken = default)
        {
            return await _context.UserActivityLogs
                .Where(log => log.UserId == userId)
                .OrderByDescending(log => log.Timestamp)
                .Take(count)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<UserActivityLog>> GetActivitiesByEventTypeAsync(
            ActivityEventType eventType,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int pageNumber = 1,
            int pageSize = 50,
            CancellationToken cancellationToken = default)
        {
            var query = _context.UserActivityLogs.Where(log => log.EventType == eventType);

            if (fromDate.HasValue)
                query = query.Where(log => log.Timestamp >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(log => log.Timestamp <= toDate.Value);

            return await query
                .OrderByDescending(log => log.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> GetActivityCountAsync(
            int? userId = null,
            ActivityEventType? eventType = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.UserActivityLogs.AsQueryable();

            if (userId.HasValue)
                query = query.Where(log => log.UserId == userId.Value);

            if (eventType.HasValue)
                query = query.Where(log => log.EventType == eventType.Value);

            if (fromDate.HasValue)
                query = query.Where(log => log.Timestamp >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(log => log.Timestamp <= toDate.Value);

            return await query.CountAsync(cancellationToken);
        }
    }
}