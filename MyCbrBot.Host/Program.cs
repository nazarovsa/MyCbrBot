using Generated;
using Insight.Cbr;
using Insight.TelegramBot.Handling.Infrastructure;
using Insight.TelegramBot.Hosting.DependencyInjection.Infrastructure;
using Insight.TelegramBot.Hosting.Polling.ExceptionHandlers;
using MyCbrBot.Core.Dates;
using MyCbrBot.Domain.Services;
using MyCbrBot.Domain.TelegramBot;
using Serilog;

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
    services.AddSingleton<IDateTimeProvider, DefaultDateTimeProvider>();
    services.AddTransient(_ =>
        new DailyInfoSoapClient(DailyInfoSoapClient.EndpointConfiguration.DailyInfoSoap));
    services.AddTransient<ICachingCurrencyService, CachingCurrencyService>();
    services.AddTransient<ICurrencyService, CurrencyService>();

    services.AddMemoryCache();

    services.AddTelegramBot(bot =>
        bot.WithBot<BotClient>(ServiceLifetime.Transient)
            .WithTelegramBotClient(client => client
                .WithLifetime(ServiceLifetime.Singleton)
                .WithMicrosoftHttpClientFactory())
            .WithOptions(opt => opt.FromConfiguration(ctx.Configuration))
            .WithPolling(polling => polling.WithExceptionHandler<LoggingPollingExceptionHandler>()));
    services.AddTelegramBotHandling(typeof(BotClient).Assembly);
});

var host = builder.Build();
await host.RunAsync();