namespace MyCbrBot.Core.Dates;

public interface IDateTimeProvider
{
    DateTimeOffset UtcDateTimeOffset { get; }

    DateTimeOffset LocalDateTimeOffset { get; }

    DateTime UtcDateTime { get; }

    DateTime LocalDateTime { get; }
}