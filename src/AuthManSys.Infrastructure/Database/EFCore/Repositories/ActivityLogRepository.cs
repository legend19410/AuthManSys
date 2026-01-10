using System.Text.Json;
using AuthManSys.Application.Common.Interfaces;
using AuthManSys.Application.Common.Helpers;
using AuthManSys.Domain.Entities;
using AuthManSys.Domain.Enums;
using AuthManSys.Infrastructure.Database.EFCore.DbContext;
using AuthManSys.Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace AuthManSys.Infrastructure.Database.EFCore.Repositories
{
    public class ActivityLogRepository : IActivityLogRepository
    {
        private readonly AuthManSysDbContext _context;
        private readonly IMapper _mapper;

        public ActivityLogRepository(AuthManSysDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
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
            var efActivityLog = new AuthManSys.Infrastructure.Database.Entities.UserActivityLog
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

            _context.UserActivityLogs.Add(efActivityLog);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<IEnumerable<AuthManSys.Domain.Entities.UserActivityLog>> GetUserActivitiesAsync(
            int userId,
            int pageNumber = 1,
            int pageSize = 50,
            CancellationToken cancellationToken = default)
        {
            var efLogs = await _context.UserActivityLogs
                .Where(log => log.UserId == userId)
                .OrderByDescending(log => log.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return _mapper.Map<IEnumerable<AuthManSys.Domain.Entities.UserActivityLog>>(efLogs);
        }

        public async Task<IEnumerable<AuthManSys.Domain.Entities.UserActivityLog>> GetLastNUserActivitiesAsync(
            int userId,
            int count,
            CancellationToken cancellationToken = default)
        {
            var efLogs = await _context.UserActivityLogs
                .Where(log => log.UserId == userId)
                .OrderByDescending(log => log.Timestamp)
                .Take(count)
                .ToListAsync(cancellationToken);

            return _mapper.Map<IEnumerable<AuthManSys.Domain.Entities.UserActivityLog>>(efLogs);
        }

        public async Task<IEnumerable<AuthManSys.Domain.Entities.UserActivityLog>> GetActivitiesByEventTypeAsync(
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

            var efLogs = await query
                .OrderByDescending(log => log.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return _mapper.Map<IEnumerable<AuthManSys.Domain.Entities.UserActivityLog>>(efLogs);
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

        public async Task<IEnumerable<AuthManSys.Domain.Entities.UserActivityLog>> GetAllActivitiesAsync(
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int pageNumber = 1,
            int pageSize = 1000,
            CancellationToken cancellationToken = default)
        {
            var query = _context.UserActivityLogs.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(log => log.Timestamp >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(log => log.Timestamp <= toDate.Value);

            var efLogs = await query
                .OrderByDescending(log => log.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return _mapper.Map<IEnumerable<AuthManSys.Domain.Entities.UserActivityLog>>(efLogs);
        }
    }
}