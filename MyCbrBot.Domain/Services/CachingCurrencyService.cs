using Insight.Cbr;
using Microsoft.Extensions.Caching.Memory;

namespace MyCbrBot.Domain.Services;

public sealed class CachingCurrencyService : ICachingCurrencyService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ICurrencyService _currencyService;

    public CachingCurrencyService(IMemoryCache memoryCache, ICurrencyService currencyService)
    {
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        _currencyService = currencyService ?? throw new ArgumentNullException(nameof(currencyService));
    }

    public async Task<IReadOnlyCollection<CurrencyRate>?> GetCurrencyRatesToDate(DateTime date, CancellationToken cancellationToken = new CancellationToken())
    {
        var cacheKey = GetCacheKey(date);
        if (_memoryCache.TryGetValue(cacheKey, out CurrencyRate[] cachedValues))
            return cachedValues;

        var rates = await _currencyService.GetCurrencyRatesToDate(date, cancellationToken);
        if (rates != null)
            // TODO : Add to config.
            _memoryCache.Set(cacheKey, rates.ToArray(), TimeSpan.FromDays(3));

        return rates;
    }

    private string GetCacheKey(DateTime date) => $"{date:ddMMyyyy}";
}
