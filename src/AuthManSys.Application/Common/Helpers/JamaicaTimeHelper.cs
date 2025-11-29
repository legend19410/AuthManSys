namespace AuthManSys.Application.Common.Helpers;

public static class JamaicaTimeHelper
{
    // Jamaica is UTC-5 and does NOT observe daylight saving time
    private static readonly TimeZoneInfo JamaicaTimeZone = TimeZoneInfo.CreateCustomTimeZone(
        "Jamaica Standard Time",
        TimeSpan.FromHours(-5),
        "Jamaica Standard Time",
        "Jamaica Standard Time"
    );

    /// <summary>
    /// Gets the current time in Jamaica timezone (UTC-5, no daylight saving)
    /// </summary>
    public static DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, JamaicaTimeZone);

    /// <summary>
    /// Converts a UTC DateTime to Jamaica timezone
    /// </summary>
    public static DateTime FromUtc(DateTime utcDateTime)
    {
        if (utcDateTime.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("DateTime must be in UTC format", nameof(utcDateTime));
        }
        return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, JamaicaTimeZone);
    }

    /// <summary>
    /// Converts a Jamaica time to UTC
    /// </summary>
    public static DateTime ToUtc(DateTime jamaicaDateTime)
    {
        return TimeZoneInfo.ConvertTimeToUtc(jamaicaDateTime, JamaicaTimeZone);
    }
}