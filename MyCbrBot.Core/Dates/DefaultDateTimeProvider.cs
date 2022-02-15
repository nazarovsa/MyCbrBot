namespace MyCbrBot.Core.Dates;

public sealed class DefaultDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcDateTimeOffset => DateTimeOffset.UtcNow;

    public DateTimeOffset LocalDateTimeOffset => DateTimeOffset.Now;

    public DateTime UtcDateTime => DateTime.UtcNow;

    public DateTime LocalDateTime => DateTime.Now;
}