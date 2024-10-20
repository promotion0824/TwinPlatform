using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PlatformPortalXL.Infrastructure.Security;
using PlatformPortalXL.ServicesApi.WeatherbitApi;
using Willow.Platform.Models;

namespace PlatformPortalXL.Services.SiteWeatherCache;

/// <summary>
/// A background service that fetches and caches weather data for all sites.
/// </summary>
public class SiteWeatherCacheHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _services;
    private readonly IAuth0Service _auth0Service;
    private readonly IWeatherService _weatherService;
    private readonly ILogger<SiteWeatherCacheHostedService> _logger;

    public SiteWeatherCacheHostedService(
        IServiceScopeFactory services,
        IAuth0Service auth0Service,
        IWeatherService weatherService,
        ILogger<SiteWeatherCacheHostedService> logger)
    {
        _services = services;
        _auth0Service = auth0Service;
        _weatherService = weatherService;
        _logger = logger;
    }

    public (bool? Success, int SiteCount) LastUpdateInfo { get; private set; } = (null, 0);

    /// <summary>
    /// Called when the host service starts, at a regular interval fetches and caches weather data for all sites.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Site weather cache service starting");

        try
        {
            await UpdateAllSitesWeatherCache();

            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(Constants.WeatherUpdateIntervalMinutes));
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await UpdateAllSitesWeatherCache();
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Site weather cache service stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Site weather cache service failed");
        }
    }

    private async Task UpdateAllSitesWeatherCache()
    {
        try
        {
            using var scope = _services.CreateScope();
            var weatherbitApiService = scope.ServiceProvider.GetRequiredService<IWeatherbitApiService>();
            var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
            var allSites = await GetAllSites(httpClientFactory);
            var locatedSites = allSites.Where(site => site.Latitude != null && site.Longitude != null).ToArray();

            foreach (var site in locatedSites)
            {
                await Task.Delay(Constants.InterCallDelayMilliseconds);

                var weather = await weatherbitApiService.GetCurrentWeatherAsync(site.Latitude!.Value, site.Longitude!.Value);
                if (weather == null)
                {
                    _logger.LogWarning("Failed to fetch weather for site {SiteId} Latitude {Latitude} Longitude {Longitude}", site.Id, site.Latitude, site.Longitude);
                    continue;
                }

                _weatherService.SetWeather(site.Id, weather);
            }

            LastUpdateInfo = (true, locatedSites.Length);
            _logger.LogInformation("Site weather cache updated weather for {SitesCount} sites", locatedSites.Length);
        }
        catch (Willow.Api.Client.RestException ex)
        {
            LastUpdateInfo = (false, 0);

            // Log Rest Exception details
            _logger.LogError(ex,
                "Site weather cache update failed due to API error. ApiName ({ApiName}) StatusCode ({StatusCode}): {context}",
                ex.ApiName,
                ex.StatusCode,
                new
                {
                    ex.Url,
                    ex.ResponseMessage.Headers,
                    ex.Response
                });
        }
        catch (Exception ex)
        {
            LastUpdateInfo = (false, 0);
            _logger.LogError(ex, "Site weather cache update failed");
        }
    }

    /// <summary>
    /// Get all valid sites from SiteCore.
    /// </summary>
    /// <param name="httpClientFactory">HttpClientFactory</param>
    /// <returns>List of Sites</returns>
    private async Task<List<Site>> GetAllSites(IHttpClientFactory httpClientFactory)
    {
        var accessToken = await _auth0Service.FetchMachineToMachineToken(ApiServiceNames.SiteCore);

        using var client = httpClientFactory.CreateClient(ApiServiceNames.SiteCore);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await client.GetAsync("sites");
        await response.EnsureSuccessStatusCode(ApiServiceNames.SiteCore);
        return await response.Content.ReadAsAsync<List<Site>>();
    }
}
