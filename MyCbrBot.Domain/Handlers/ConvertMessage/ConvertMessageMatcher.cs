using Insight.TelegramBot.Handling.Matchers.TextMatchers;

namespace MyCbrBot.Domain.Handlers.ConvertMessage;

public sealed class ConvertMessageMatcher : TextStartWithUpdateMatcher
{
    public ConvertMessageMatcher()
    {
        Template = "/convert";
    }
}