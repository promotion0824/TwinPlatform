using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace PlatformPortalXL.Services.SiteWeatherCache;

/// <summary>
/// A health check to report the status of the site weather cache.
/// </summary>
/// <remarks>
/// Health check results include whether the cache was successfully updated and the number of sites for which weather
/// was cached.
/// </remarks>
public class SiteWeatherCacheHealthCheck : IHealthCheck
{
    private readonly SiteWeatherCacheHostedService _cacheSitesWeatherService;

    /// <summary>
    /// A health check to report the status of the site weather cache.
    /// </summary>
    /// <remarks>
    /// Health check results include whether the cache was successfully updated and the number of sites for which weather
    /// was cached.
    /// </remarks>
    public SiteWeatherCacheHealthCheck(SiteWeatherCacheHostedService cacheSitesWeatherService)
    {
        _cacheSitesWeatherService = cacheSitesWeatherService;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (_cacheSitesWeatherService.LastUpdateInfo.Success == false)
        {
            return Task.FromResult(HealthCheckResult.Degraded());
        }

        var description = _cacheSitesWeatherService.LastUpdateInfo.Success == null
            ? "Site weather cache not yet populated"
            : $"Site weather cache populated for {_cacheSitesWeatherService.LastUpdateInfo.SiteCount} sites";

        return Task.FromResult(HealthCheckResult.Healthy(description));
    }
}
