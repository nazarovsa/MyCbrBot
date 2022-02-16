using System.Globalization;
using System.Text;
using Insight.Cbr;
using Insight.TelegramBot;
using Insight.TelegramBot.Models;
using Microsoft.Extensions.Logging;
using MyCbrBot.Core.Dates;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MyCbrBot.Domain.TelegramBot;

public sealed class UpdateProcessor : IUpdateProcessor
{
    // TODO: Add logs!
    private readonly ILogger<UpdateProcessor> _logger;
    private readonly ICurrencyService _currencyService;
    private readonly IBot _bot;
    private readonly IDateTimeProvider _dateTimeProvider;

    public UpdateProcessor(ILogger<UpdateProcessor> logger,
        ICurrencyService currencyService,
        IBot bot,
        IDateTimeProvider dateTimeProvider)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _currencyService = currencyService ?? throw new ArgumentNullException(nameof(currencyService));
        _bot = bot ?? throw new ArgumentNullException(nameof(bot));
        _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
    }

    public async Task ProcessUpdate(Update update, CancellationToken cancellationToken = new CancellationToken())
    {
        switch (update.Type)
        {
            case UpdateType.Message:
                if (update.Message.Text.Equals("/start"))
                {
                    await _bot.SendMessageAsync(new TextMessage(update.Message.Chat)
                    {
                        Text =
                            "Привет, Я - 🤖 MyCbrBot!\nЯ могу поделиться курсами валют 💲 согласно ставке Центрального Банка России на сегодня или предшествующую дату."
                    }, cancellationToken);
                }
                else if (update.Message.Text.Equals("/help"))
                {
                    await _bot.SendMessageAsync(new TextMessage(update.Message.Chat)
                    {
                        Text =
                            @"Я могу прислать курсы валют согласно ставке Центрального Банка России на сегодня или указанную дату.

Для того, чтобы получить доступные курсы валют на сегодня по Московскому времени отправьте команду '/today'. Если вас интересует курс конкретной валюты, добавьте ее код или ISO код через пробел.
Например, '/today usd' или '/today 840' вернут курс доллара на сегодняшний день.

Для того, чтобы получить доступные курсы валют на дату по Московскому времени отправьте команду 'date' c датой через пробел в формате dd.MM.yyyy или dd-MM-yyyy. Аналогично с '/today' вы можете запросить курс по конкретной валюте, для этого добавьте ее код или ISO код через пробел после даты.
Например, '/date 01.01.2020' вернет все доступные курсы валют на 01.01.2020 соответственно. '/date 01.01.2020 840' или '/date 01.01.2020 usd' вернут курс доллара на 01.01.2020."
                    }, cancellationToken);
                }
                else if (update.Message.Text.StartsWith("/today"))
                {
                    var args = update.Message.Text
                        .Trim()
                        .Remove(0, "/today".Length)
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
                            sb.Append($"<b>{rate.Name} ({rate.Code} - {rate.IsoCode})</b>: {rate.Par} - {rate.Rate}\n");

                        await _bot.SendMessageAsync(new TextMessage(update.Message.Chat)
                        {
                            Text = sb.ToString(),
                            ParseMode = ParseMode.Html
                        }, cancellationToken);
                    }
                    else
                    {
                        sb.Append("Курс валюты 💲 на сегодня:\n");
                        var rate = rates.FirstOrDefault(x =>
                            args.Equals(x.IsoCode, StringComparison.InvariantCultureIgnoreCase) ||
                            args.Equals(x.Name, StringComparison.InvariantCultureIgnoreCase) ||
                            args.Equals(x.Code, StringComparison.InvariantCultureIgnoreCase));

                        if (rate == null)
                            sb.Append($"Валюта по вашему запросу не найдена 😔: {args}");
                        else
                            sb.Append($"<b>{rate.Name} ({rate.Code} - {rate.IsoCode})</b>: {rate.Par} - {rate.Rate}");

                        await _bot.SendMessageAsync(new TextMessage(update.Message.Chat)
                        {
                            Text = sb.ToString(),
                            ParseMode = ParseMode.Html
                        }, cancellationToken);
                    }
                }
                else if (update.Message.Text.StartsWith("/date"))
                {
                    var args = update.Message.Text
                        .Trim()
                        .Remove(0, "/date".Length)
                        .Trim()
                        .Split();

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
                            sb.Append($"<b>{rate.Name} ({rate.Code} - {rate.IsoCode})</b>: {rate.Par} - {rate.Rate}\n");

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
                            sb.Append($"<b>{rate.Name} ({rate.Code} - {rate.IsoCode})</b>: {rate.Par} - {rate.Rate}");

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

                break;
            default:
                throw new NotImplementedException();
        }
    }
}