using Generated;
using Insight.Cbr;
using Insight.TelegramBot;
using Insight.TelegramBot.Configurations;
using Insight.TelegramBot.Web;
using Insight.TelegramBot.Web.Hosts;
using Microsoft.Extensions.Options;
using MyCbrBot.Core.Dates;
using MyCbrBot.Domain.TelegramBot;
using Newtonsoft.Json.Serialization;
using Serilog;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureAppConfiguration((ctx, configurationBuilder) =>
{
    configurationBuilder.AddJsonFile($"appsettings.{ctx.HostingEnvironment.EnvironmentName.ToLowerInvariant()}.json", false, true);
    configurationBuilder.AddJsonFile("appsettings.protected.json", true, true);
    configurationBuilder.AddEnvironmentVariables();
});

builder.Host.UseSerilog((ctx, logBuilder) =>
{
    logBuilder.ReadFrom.Configuration(ctx.Configuration)
        .Enrich.FromLogContext();
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddHealthChecks();

builder.Services.AddMvc()
    .AddUpdateController()
    .AddNewtonsoftJson(
        opt => opt.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver());

builder.Services.AddSingleton<IDateTimeProvider, DefaultDateTimeProvider>();

builder.Services.AddTransient(ctx => new DailyInfoSoapClient(DailyInfoSoapClient.EndpointConfiguration.DailyInfoSoap));
builder.Services.AddTransient<ICurrencyService, CurrencyService>();

builder.Services.Configure<BotConfiguration>(builder.Configuration.GetSection(nameof(BotConfiguration)));

builder.Services.AddTransient<IBot, BotClient>();
builder.Services.AddScoped<IUpdateProcessor, UpdateProcessor>();
builder.Services.AddTransient<ITelegramBotClient, TelegramBotClient>(c =>
    new TelegramBotClient(c.GetService<IOptions<BotConfiguration>>()?.Value.Token,
        c.GetService<IHttpClientFactory>().CreateClient()));

builder.Services.AddHostedService(c =>
    new TelegramBotWebHookHost(c.GetService<ITelegramBotClient>(),
        c.GetService<IOptions<BotConfiguration>>()?.Value));

var app = builder.Build();

app.UseRouting();
app.UseHealthChecks("healthcheck");

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    var botConfiguration = new BotConfiguration();
    builder.Configuration.GetSection(nameof(BotConfiguration)).Bind(botConfiguration);
    if (botConfiguration.WebHookConfiguration.UseWebHook)
        endpoints.AddUpdateControllerRoute(botConfiguration.WebHookConfiguration.WebHookPath);
});

app.Run();