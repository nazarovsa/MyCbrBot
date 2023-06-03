using Insight.TelegramBot.Handling.Matchers.TextMatchers;

namespace MyCbrBot.Domain.Handlers.HelpMessage;

public sealed class HelpMessageMatcher : TextEqualsUpdateMatcher
{
    public HelpMessageMatcher()
    {
        Template = "/help";
    }
}