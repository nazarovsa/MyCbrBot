using Insight.TelegramBot.Handling.Matchers.TextMatchers;

namespace MyCbrBot.Domain.Handlers.StartMessage;

public sealed class StartMessageMatcher : TextEqualsUpdateMatcher
{
    public StartMessageMatcher()
    {
        Template = "/start";
    }
}