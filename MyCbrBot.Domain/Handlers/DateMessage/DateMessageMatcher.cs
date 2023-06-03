using Insight.TelegramBot.Handling.Matchers.TextMatchers;

namespace MyCbrBot.Domain.Handlers.DateMessage;

public sealed class DateMessageMatcher : TextStartWithUpdateMatcher
{
    public DateMessageMatcher()
    {
        Template = "/date";
    }
}