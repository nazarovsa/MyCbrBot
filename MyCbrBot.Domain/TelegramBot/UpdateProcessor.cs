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
                        Text = "Hello, I'm MyCbrBot and I can send u currency rates of Russian Central Bank"
                    }, cancellationToken);
                }
                else if (update.Message.Text.StartsWith("/today"))
                {
                    var args = update.Message.Text
                        .Trim()
                        .Remove(0, "/today".Length)
                        .Trim();

                    // TODO: Check rates for null
                    var rates = await _currencyService.GetCurrencyRatesToDate(
                        _dateTimeProvider.UtcDateTime.AddHours(3),
                        cancellationToken);

                    var sb = new StringBuilder();
                    if (string.IsNullOrWhiteSpace(args))
                    {
                        sb.Append("Today currencies:\n");
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
                        sb.Append("Today rate:\n");
                        var rate = rates.FirstOrDefault(x =>
                            args.Equals(x.IsoCode, StringComparison.InvariantCultureIgnoreCase) ||
                            args.Equals(x.Name, StringComparison.InvariantCultureIgnoreCase) ||
                            args.Equals(x.Code, StringComparison.InvariantCultureIgnoreCase));

                        if (rate == null)
                            sb.Append($"There is no currency for: {args}");
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
                        throw new NotImplementedException("No args!");
                    }

                    if (!DateTime.TryParseExact(args.First(), new[] { "dd-MM-yyyy", "dd.MM.yyyy" }, null,
                            DateTimeStyles.None, out var date))
                    {
                        throw new NotImplementedException("Failed to parse date!");
                    }

                    // TODO: Check rates for null
                    var rates = await _currencyService.GetCurrencyRatesToDate(
                        date,
                        cancellationToken);

                    var sb = new StringBuilder($"Currencies for {date:dd-MM-yyyy}:\n");
                    if (args.Length == 1)
                    {
                        sb.Append($"Currency rates on {date:dd-MM-yyyy}:\n");
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
                        sb.Append($"Currency rate on {date:dd-MM-yyyy}:\n");
                        var rate = rates.FirstOrDefault(x =>
                            args.Last().Equals(x.IsoCode, StringComparison.InvariantCultureIgnoreCase) ||
                            args.Last().Equals(x.Name, StringComparison.InvariantCultureIgnoreCase) ||
                            args.Last().Equals(x.Code, StringComparison.InvariantCultureIgnoreCase));

                        if (rate == null)
                            sb.Append($"There is no currency for: {args.Last()}");
                        else
                            sb.Append($"<b>{rate.Name} ({rate.Code} - {rate.IsoCode})</b>: {rate.Par} - {rate.Rate}");

                        await _bot.SendMessageAsync(new TextMessage(update.Message.Chat)
                        {
                            Text = sb.ToString(),
                            ParseMode = ParseMode.Html
                        }, cancellationToken);
                    }
                    else
                        throw new InvalidOperationException("Wrong pattern!");
                }

                break;
            default:
                throw new NotImplementedException();
        }
    }
}