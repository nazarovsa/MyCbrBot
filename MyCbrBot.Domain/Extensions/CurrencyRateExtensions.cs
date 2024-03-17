using Insight.Cbr;

namespace MyCbrBot.Domain.Extensions;

public static class CurrencyRateExtensions
{
    public static string GetString(this CurrencyRate rate)
    {
        return
            $"<code>{rate.GetNameWithCodes()}</code>\n<b>{rate.Par} {rate.Code} = {rate.Rate} ₽</b>";
    }

    public static string GetNameWithCodes(this CurrencyRate rate)
    {
        return $"[{rate.Code}] ({rate.IsoCode}) {rate.Name}";
    }

    public static string CurrencyNotFound(this string currency)
    {
        return $"Курс валюты 💲 не найден: {currency}";
    }
}

public static class CurrencyRateEnumerableExtensions
{
    public static CurrencyRate? FirstOrDefaultByKey(this IEnumerable<CurrencyRate> rates, string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentNullException(nameof(key));
        }

        return rates.FirstOrDefault(x =>
            key.Equals(x.IsoCode, StringComparison.OrdinalIgnoreCase) ||
            key.Equals(x.Name, StringComparison.OrdinalIgnoreCase) ||
            key.Equals(x.Code, StringComparison.OrdinalIgnoreCase));
    }
}