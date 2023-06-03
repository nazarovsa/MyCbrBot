using Insight.TelegramBot;
using Insight.TelegramBot.Handling.Handlers;
using Insight.TelegramBot.Models;
using Telegram.Bot.Types;

namespace MyCbrBot.Domain.Handlers.StartMessage;

public sealed class StartMessageHandler : IMatchingUpdateHandler<StartMessageMatcher>
{
    private readonly IBot _bot;

    public StartMessageHandler(IBot bot)
    {
        _bot = bot;
    }

    public Task Handle(Update update, CancellationToken cancellationToken = default)
    {
        return _bot.SendMessageAsync(new TextMessage(update.Message!.Chat)
        {
            Text = "Привет, Я - 🤖 MyCbrBot!\nЯ могу поделиться курсами валют 💲 согласно ставке Центрального Банка России на сегодня или предшествующую дату."
        }, cancellationToken);
    }
}