using Insight.Cbr;

namespace MyCbrBot.Domain.Services;

public static class CurrencyRateExtensions
{
    public static string GetString(this CurrencyRate rate)
    {
        return $"<code>[{rate.Code}] ({rate.IsoCode}) {rate.Name}</code>\n<b>{rate.Par} {rate.Code} = {rate.Rate} ₽</b>";
    }
}