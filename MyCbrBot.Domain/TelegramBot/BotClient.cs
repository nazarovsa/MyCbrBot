using Insight.TelegramBot;
using Insight.TelegramBot.Models;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;

namespace MyCbrBot.Domain.TelegramBot;

public sealed class BotClient : Bot
{
    private readonly ILogger<BotClient> _logger;

    public BotClient(ILogger<BotClient> logger, ITelegramBotClient client) : base(client)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override Task<Message> SendMessageAsync(TextMessage message,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return base.SendMessageAsync(message, cancellationToken);
        }
        catch (ChatNotInitiatedException ex)
        {
            _logger.LogError(ex, "Bot can't initiate conversation with a user: {message}", message);
        }

        // TODO : Грубо
        return null;
    }
}