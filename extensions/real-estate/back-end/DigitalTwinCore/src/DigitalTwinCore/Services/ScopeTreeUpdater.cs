using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using DigitalTwinCore.Services.Cacheless;
using Microsoft.Extensions.Caching.Memory;
using DigitalTwinCore.Services.AdtApi;
using DigitalTwinCore.Models;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Diagnostics.HealthChecks;


namespace DigitalTwinCore.Services
{
    /// <summary>
    /// Keep a scope tree always up to date in memory. Refresh every 15 minutes.
    /// </summary>
    public class ScopeTreeUpdaterService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IMemoryCache _memoryCache;
        private readonly AzureDigitalTwinsSettings _adtSettings;
        private readonly ILogger<ScopeTreeUpdaterService> _logger;
        private bool _isRunning = false;

        public bool IsRunning => _isRunning;

        public ScopeTreeUpdaterService(
            IServiceScopeFactory serviceScopeFactory,
            IMemoryCache memoryCache,
            IOptions<AzureDigitalTwinsSettings> adtSettings,
            ILogger<ScopeTreeUpdaterService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _memoryCache = memoryCache;
            _adtSettings = adtSettings.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await Task.Yield();
                _isRunning = true;
                await UpdateCache();
                using var timer = new PeriodicTimer(TimeSpan.FromMinutes(15));
                while (await timer.WaitForNextTickAsync(stoppingToken)) {
                    await UpdateCache();
                }
            }
            catch (Exception ex)
            {
                _isRunning = false;
                _logger.LogError(ex, "ScopeTreeUpdaterService failed");
            }
        }

        async Task UpdateCache() {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var adtService = scope.ServiceProvider.GetRequiredService<IDigitalTwinService>() as CachelessAdtService;
                adtService.SiteAdtSettings = SiteAdtSettings.CreateInstance(Guid.Empty, null, _adtSettings);
                await adtService.UpdateScopeTree();
            }
        }
    }

    public class ScopeTreeUpdaterHealthCheck : IHealthCheck
    {
        private readonly ScopeTreeUpdaterService _scopeTreeUpdaterService;

        public ScopeTreeUpdaterHealthCheck(ScopeTreeUpdaterService scopeTreeUpdaterService)
        {
            _scopeTreeUpdaterService = scopeTreeUpdaterService;
        }

        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = new CancellationToken())
        {
            if (_scopeTreeUpdaterService?.IsRunning == true)
            {
                return Task.FromResult(HealthCheckResult.Healthy());
            }
            else
            {
                return Task.FromResult(HealthCheckResult.Unhealthy(
                    "ScopeTreeUpdaterService is not running. It should be in single tenant but not multi tenant."));
            }
        }
    }
}
