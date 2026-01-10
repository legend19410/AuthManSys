using AuthManSys.Domain.Enums;

namespace AuthManSys.Domain.Entities;

public class UserActivityLog
{
    public long Id { get; private set; }
    public int? UserId { get; private set; }
    public ActivityEventType EventType { get; private set; }
    public string? EventTag { get; private set; }
    public string? Description { get; private set; }
    public DateTime Timestamp { get; private set; }
    public string? IPAddress { get; private set; }
    public string? Device { get; private set; }
    public string? Platform { get; private set; }
    public string? Location { get; private set; }
    public string? Metadata { get; private set; }

    private UserActivityLog() { } // For ORM

    public UserActivityLog(
        int? userId,
        ActivityEventType eventType,
        string? description = null,
        string? eventTag = null,
        string? ipAddress = null,
        string? device = null,
        string? platform = null,
        string? location = null,
        string? metadata = null
    )
    {
        UserId = userId;
        EventType = eventType;
        Description = description;
        EventTag = eventTag;
        IPAddress = ipAddress;
        Device = device;
        Platform = platform;
        Location = location;
        Metadata = metadata;
        Timestamp = DateTime.UtcNow;
    }

    // Domain behaviors
    public bool IsForUser(int userId)
        => UserId == userId;

    public bool IsOfType(ActivityEventType eventType)
        => EventType == eventType;

    public bool OccurredAfter(DateTime date)
        => Timestamp > date;

    public bool OccurredBefore(DateTime date)
        => Timestamp < date;

    public bool HasMetadata()
        => !string.IsNullOrEmpty(Metadata);

    public bool IsFromIP(string ipAddress)
        => IPAddress == ipAddress;
}