using System.Text;
using Insight.TelegramBot;
using Insight.TelegramBot.Handling.Handlers;
using Insight.TelegramBot.Models;
using MyCbrBot.Core.Dates;
using MyCbrBot.Domain.Extensions;
using MyCbrBot.Domain.Services;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MyCbrBot.Domain.Handlers.TodayMessage;

public sealed class TodayMessageHandler : IMatchingUpdateHandler<TodayMessageMatcher>
{
    private readonly IBot _bot;
    private readonly ICachingCurrencyService _currencyService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public TodayMessageHandler(IBot bot,
        ICachingCurrencyService cachingCurrencyService,
        IDateTimeProvider dateTimeProvider)
    {
        _bot = bot ?? throw new ArgumentNullException(nameof(bot));
        _currencyService =
            cachingCurrencyService ?? throw new ArgumentNullException(nameof(cachingCurrencyService));
        _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
    }

    public async Task Handle(Update update, CancellationToken cancellationToken = default)
    {
        var args = update.Message!
            .Text!
            .Replace("/today", string.Empty)
            .Trim();

        var rates = await _currencyService.GetCurrencyRatesToDate(
            _dateTimeProvider.UtcDateTime.AddHours(3),
            cancellationToken);

        if (rates == null)
        {
            await _bot.SendMessageAsync(new TextMessage(update.Message.Chat)
            {
                Text = "Не удалось получить курсы валют от Центрального Банка России. Попробуйте позже. 😔"
            }, cancellationToken);
            return;
        }

        var sb = new StringBuilder();
        if (string.IsNullOrWhiteSpace(args))
        {
            sb.Append("Курсы валют 💲 на сегодня:\n");
            foreach (var rate in rates)
            {
                sb.Append(rate.GetString());
                sb.Append("\n");
            }

            await _bot.SendMessageAsync(new TextMessage(update.Message.Chat)
            {
                Text = sb.ToString(),
                ParseMode = ParseMode.Html
            }, cancellationToken);
        }
        else
        {
            sb.Append("Курс валюты 💲 на сегодня:\n");
            var rate = rates.FirstOrDefaultByKey(args);

            if (rate == null)
                sb.Append($"Валюта по вашему запросу не найдена 😔: {args}");
            else
                sb.Append(rate.GetString());

            await _bot.SendMessageAsync(new TextMessage(update.Message.Chat)
            {
                Text = sb.ToString(),
                ParseMode = ParseMode.Html
            }, cancellationToken);
        }
    }
}