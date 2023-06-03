using Insight.TelegramBot.Handling.Matchers.TextMatchers;

namespace MyCbrBot.Domain.Handlers.TodayMessage;

public sealed class TodayMessageMatcher : TextStartWithUpdateMatcher
{
    public TodayMessageMatcher()
    {
        Template = "/today";
    }
}