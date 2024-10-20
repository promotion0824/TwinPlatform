using System;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PlatformPortalXL.Dto;
using PlatformPortalXL.ServicesApi.WeatherbitApi;

namespace PlatformPortalXL.Services;

public interface IWeatherService
{
    void SetWeather(Guid siteId, WeatherbitSimpleDto weather);
    public WeatherDto GetWeather(Guid siteId);
}

/// <summary>
/// Manage the weather cache from here to avoid cache miss bugs.
/// </summary>
public class WeatherService : IWeatherService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<WeatherService> _logger;

    /// <summary>
    /// Manage the weather cache from here to avoid cache miss bugs.
    /// </summary>
    /// <param name="memoryCache">Our memory cache</param>
    /// <param name="logger">The logger</param>
    public WeatherService(IMemoryCache memoryCache, ILogger<WeatherService> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
    }

    // Put the cache key management in one place to avoid cache miss bugs.
    private static string SiteWeatherKey(Guid id) => $"{id}-weatherbit";

    /// <summary>
    /// Get the weather for a site from the cache.
    /// </summary>
    /// <param name="siteId">Site against which weather is stored</param>
    /// <returns>The temperature, code and icon for the site if found, otherwise null</returns>
    public WeatherDto GetWeather(Guid siteId)
    {
        var got = _memoryCache.TryGetValue<WeatherbitSimpleDto>(SiteWeatherKey(siteId), out var weatherBit);

        if (got)
        {
            return new WeatherDto
            {
                Temperature = weatherBit.Temp,
                Code = weatherBit.Code,
                Icon = weatherBit.Icon
            };
        }

        _logger.LogTrace("Weather not found in cache for site {SiteId}", siteId);

        return null;
    }

    /// <summary>
    /// Cache the weather against siteId.
    /// </summary>
    /// <param name="siteId">Site for this weather</param>
    /// <param name="weather">Temp, code and icon data for the weather at this site</param>
    public void SetWeather(Guid siteId, WeatherbitSimpleDto weather)
    {
        _memoryCache.Set(SiteWeatherKey(siteId), weather, TimeSpan.FromHours(Constants.CacheExpiryHours));

        using (_logger.BeginScope(new Dictionary<string, object> { { "Weather", weather } }))
        {
            _logger.LogDebug("Setting weather in cache for site {SiteId}", siteId);
        }
    }
}
