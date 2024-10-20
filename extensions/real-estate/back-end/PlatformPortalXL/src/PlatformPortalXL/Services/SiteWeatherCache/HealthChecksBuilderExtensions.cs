using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace PlatformPortalXL.Services.SiteWeatherCache;

public static class HealthChecksBuilderExtensions
{
    /// <summary>
    /// Add a health check to report the status of the site weather cache.
    /// </summary>
    public static IHealthChecksBuilder AddSiteWeatherCache(this IHealthChecksBuilder builder)
    {
        using var services = builder.Services.BuildServiceProvider();
        var environment = services.GetRequiredService<IWebHostEnvironment>();
        if (environment.IsEnvironment("Test"))
        {
            return builder;
        }

        return builder.Add(new HealthCheckRegistration("SiteWeatherCache",
            sp =>
            {
                var weatherCacheService = sp
                    .GetServices<IHostedService>()
                    .Where(d => d.GetType() == typeof(SiteWeatherCacheHostedService))
                    .Cast<SiteWeatherCacheHostedService>()
                    .Single();

                return new SiteWeatherCacheHealthCheck(weatherCacheService);
            },
            HealthStatus.Degraded,
            ["healthz"]));
    }
}
