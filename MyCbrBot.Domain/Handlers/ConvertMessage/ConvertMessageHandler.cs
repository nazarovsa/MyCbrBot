using System.Globalization;
using Insight.TelegramBot;
using Insight.TelegramBot.Handling.Handlers;
using Insight.TelegramBot.Models;
using MyCbrBot.Core.Dates;
using MyCbrBot.Domain.Extensions;
using MyCbrBot.Domain.Services;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MyCbrBot.Domain.Handlers.ConvertMessage;

public sealed class ConvertMessageHandler : IMatchingUpdateHandler<ConvertMessageMatcher>
{
    private readonly IBot _bot;
    private readonly ICachingCurrencyService _currencyService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ConvertMessageHandler(IBot bot,
        ICachingCurrencyService currencyService,
        IDateTimeProvider dateTimeProvider)
    {
        _bot = bot ?? throw new ArgumentNullException(nameof(bot));
        _currencyService = currencyService ?? throw new ArgumentNullException(nameof(currencyService));
        _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
    }

    public async Task Handle(Update update, CancellationToken cancellationToken = default)
    {
        // parse args
        var args = update.Message!
            .Text!
            .Replace("/convert", string.Empty)
            .Trim()
            .Split(' ');

        if (args.Length < 2 || args.Length > 4)
        {
            await _bot.SendMessageAsync(new TextMessage(update.Message.Chat)
            {
                Text = "Передано неверное количество аргументов. Для помощи отправьте команду /help."
            }, cancellationToken);
            return;
        }

        if (!decimal.TryParse(args[0], out var sum))
        {
            await _bot.SendMessageAsync(new TextMessage(update.Message.Chat)
            {
                Text = "Не удалось извлечь сумму. Для помощи отправьте команду /help."
            }, cancellationToken);
            return;
        }

        var from = args[1];

        string? to = null;
        if (args.Length > 2)
        {
            to = args[2];
        }

        DateTime? date = null;
        if (args.Length == 4)
        {
            if (DateTime.TryParseExact(args[3], new[] { "dd-MM-yyyy", "dd.MM.yyyy" }, null,
                    DateTimeStyles.None, out var parsedDate))
            {
                date = parsedDate;
            }
            else
            {
                await _bot.SendMessageAsync(new TextMessage(update.Message.Chat)
                {
                    Text = "Переданная дата не соответствует одному из форматов 😔: 'dd-MM-yyyy' или 'dd.MM.yyyy'"
                }, cancellationToken);
                return;
            }
        }

        var dateToSearch = date ?? _dateTimeProvider.UtcDateTime;
        var rates = await _currencyService.GetCurrencyRatesToDate(dateToSearch, cancellationToken);

        if (rates == null)
        {
            await _bot.SendMessageAsync(new TextMessage(update.Message.Chat)
            {
                Text = "Не удалось получить курсы валют от Центрального Банка России. Попробуйте позже. 😔"
            }, cancellationToken);
            return;
        }
        
        if (from.Equals("rub", StringComparison.OrdinalIgnoreCase) ||
            from.Equals("840", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(to))
            {
                await _bot.SendMessageAsync(new TextMessage(update.Message.Chat)
                {
                    Text = "При конвертации рублей должна быть указана целевая валюта."
                }, cancellationToken);
                return;
            }

            var targetRate = rates.FirstOrDefaultByKey(to);
            if (targetRate == null)
            {
                await _bot.SendMessageAsync(new TextMessage(update.Message.Chat)
                {
                    Text = to.CurrencyNotFound(),
                    ParseMode = ParseMode.Html
                }, cancellationToken);
                return;
            }
            
            var fromRubResult = sum / targetRate.Rate * Convert.ToInt32(targetRate.Par);
            await _bot.SendMessageAsync(new TextMessage(update.Message.Chat)
            {
                Text = $"{sum} ₽ = {fromRubResult:F2} {targetRate.Code}",
            }, cancellationToken);
            return;
        }

        var fromRate = rates.FirstOrDefaultByKey(from);
        if (fromRate == null)
        {
            await _bot.SendMessageAsync(new TextMessage(update.Message.Chat)
            {
                Text = from.CurrencyNotFound(),
                ParseMode = ParseMode.Html
            }, cancellationToken);
            return;
        }

        if (string.IsNullOrWhiteSpace(to) ||
            to.Equals("rub", StringComparison.OrdinalIgnoreCase) ||
            to.Equals("840", StringComparison.OrdinalIgnoreCase))
        {
            var inRubles = sum * fromRate.Rate / Convert.ToInt32(fromRate.Par);
            await _bot.SendMessageAsync(new TextMessage(update.Message.From!.Id)
            {
                Text = $"{sum} {fromRate.Name} = {inRubles:F2} ₽",
                ParseMode = ParseMode.Html
            }, cancellationToken);
            return;
        }

        var toRate = rates.FirstOrDefaultByKey(to);
        if (toRate == null)
        {
            await _bot.SendMessageAsync(new TextMessage(update.Message.Chat)
            {
                Text = to.CurrencyNotFound(),
                ParseMode = ParseMode.Html
            }, cancellationToken);
            return;
        }

        var fromInRubles = sum * fromRate.Rate / Convert.ToInt32(fromRate.Par);
        var result = fromInRubles / toRate.Rate * Convert.ToInt32(toRate.Par);

        await _bot.SendMessageAsync(new TextMessage(update.Message.From!.Id)
        {
            Text = $"{sum} {fromRate.Code} = {result:F2} {toRate.Code}",
            ParseMode = ParseMode.Html
        }, cancellationToken);
    }
}