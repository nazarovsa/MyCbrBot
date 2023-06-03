using Generated;
using Insight.Cbr;
using Insight.TelegramBot;
using Insight.TelegramBot.Configurations;
using Insight.TelegramBot.Handling.Infrastructure;
using Insight.TelegramBot.UpdateProcessors;
using Insight.TelegramBot.Web;
using Insight.TelegramBot.Web.Hosts;
using Microsoft.Extensions.Options;
using MyCbrBot.Core.Dates;
using MyCbrBot.Domain.Services;
using MyCbrBot.Domain.TelegramBot;
using Newtonsoft.Json.Serialization;
using Serilog;
using Telegram.Bot;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureAppConfiguration((ctx, configurationBuilder) =>
{
    configurationBuilder.AddJsonFile($"appsettings.{ctx.HostingEnvironment.EnvironmentName.ToLowerInvariant()}.json",
        false, true);
    configurationBuilder.AddJsonFile("appsettings.protected.json", true, true);
    configurationBuilder.AddEnvironmentVariables();
});

builder.UseSerilog((ctx, logBuilder) =>
{
    logBuilder.ReadFrom.Configuration(ctx.Configuration)
        .Enrich.FromLogContext();
});

// Add services to the container.
builder.ConfigureServices((ctx, services) =>
{
    services.AddHttpClient();
    services.AddSingleton<IDateTimeProvider, DefaultDateTimeProvider>();
    services.AddTransient(_ =>
        new DailyInfoSoapClient(DailyInfoSoapClient.EndpointConfiguration.DailyInfoSoap));
    services.AddTransient<ICachingCurrencyService, CachingCurrencyService>();
    services.AddTransient<ICurrencyService, CurrencyService>();

    services.Configure<BotConfiguration>(ctx.Configuration.GetSection(nameof(BotConfiguration)));
    services.AddTransient<IBot, BotClient>();
    services.AddTelegramBotHandling(typeof(BotClient).Assembly);

    services.AddSingleton<ITelegramBotClient, TelegramBotClient>(c =>
        new TelegramBotClient(c.GetService<IOptions<BotConfiguration>>().Value.Token,
            new HttpClient()));

    services.AddPollingBotHost();

    services.AddMemoryCache();
});


var host = builder.Build();
await host.RunAsync();