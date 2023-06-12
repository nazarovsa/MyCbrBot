using System.Globalization;
using System.Text;
using Insight.TelegramBot;
using Insight.TelegramBot.Handling.Handlers;
using Insight.TelegramBot.Models;
using MyCbrBot.Core.Dates;
using MyCbrBot.Domain.Services;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MyCbrBot.Domain.Handlers.DateMessage;

public sealed class DateMessageHandler : IMatchingUpdateHandler<DateMessageMatcher>
{
    private readonly IBot _bot;
    private readonly ICachingCurrencyService _currencyService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public DateMessageHandler(IBot bot,
        ICachingCurrencyService currencyService,
        IDateTimeProvider dateTimeProvider)
    {
        _bot = bot ?? throw new ArgumentNullException(nameof(bot));
        _currencyService = currencyService ?? throw new ArgumentNullException(nameof(currencyService));
        _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
    }

    public async Task Handle(Update update, CancellationToken cancellationToken = default)
    {
        var args = update.Message!
            .Text!
            .Replace("/date", string.Empty)
            .Trim()
            .Split(' ');

        if (args.Length == 0)
        {
            await _bot.SendMessageAsync(new TextMessage(update.Message.Chat)
            {
                Text =
                    "Введенная команда не соответствует одному из шаблонов 😔: '/date дата' или '/date дата код_валюты'.",
                ParseMode = ParseMode.Html
            }, cancellationToken);
            return;
        }

        if (!DateTime.TryParseExact(args.First(), new[] { "dd-MM-yyyy", "dd.MM.yyyy" }, null,
                DateTimeStyles.None, out var date))
        {
            await _bot.SendMessageAsync(new TextMessage(update.Message.Chat)
            {
                Text =
                    "Переданная дата не соответствует одному из форматов 😔: 'dd-MM-yyyy' или 'dd.MM.yyyy'"
            }, cancellationToken);
            return;
        }

        if (date.Date > _dateTimeProvider.UtcDateTime.Date.AddHours(3).Date)
        {
            await _bot.SendMessageAsync(new TextMessage(update.Message.Chat)
            {
                Text = "К сожалению, я не могу предоставить курс на будущую дату. ☹️"
            }, cancellationToken);
            return;
        }

        var rates = await _currencyService.GetCurrencyRatesToDate(
            date,
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
        if (args.Length == 1)
        {
            sb.Append($"Курсы валют 💲 на {date:dd-MM-yyyy}:\n");
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
        else if (args.Length == 2)
        {
            sb.Append($"Курс валюты 💲 на {date:dd-MM-yyyy}:\n");
            var rate = rates.FirstOrDefault(x =>
                args.Last().Equals(x.IsoCode, StringComparison.InvariantCultureIgnoreCase) ||
                args.Last().Equals(x.Name, StringComparison.InvariantCultureIgnoreCase) ||
                args.Last().Equals(x.Code, StringComparison.InvariantCultureIgnoreCase));

            if (rate == null)
                sb.Append($"Курс валюты 💲 по вашему запросу не найден: {args.Last()}");
            else
                sb.Append(rate.GetString());

            await _bot.SendMessageAsync(new TextMessage(update.Message.Chat)
            {
                Text = sb.ToString(),
                ParseMode = ParseMode.Html
            }, cancellationToken);
        }
        else
        {
            await _bot.SendMessageAsync(new TextMessage(update.Message.Chat)
            {
                Text =
                    "Введенная команда не соответствует одному из шаблонов 😔: '/date дата' или '/date дата код_валюты'.",
                ParseMode = ParseMode.Html
            }, cancellationToken);
        }
    }
}