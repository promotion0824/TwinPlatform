namespace Willow.MappedTopologyIngestionApi;

using global::HealthChecks.AzureServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Willow.AzureDigitalTwins.SDK.Client;
using Willow.Extensions.Logging;
using Willow.MappedTopologyIngestionApi.HealthChecks;
using Willow.Model.Adt;
using Willow.Model.Requests;
using Willow.TopologyIngestion.Interfaces;

/// <summary>
/// Startup health checks to external resources.
/// </summary>
public class StartupHealthCheckService : BackgroundService
{
    //private readonly IADTService adtService;
    private readonly HealthCheckTwinsApi healthCheckTwinsApi;
    private readonly HealthCheckMappedApi healthCheckMappedApi;
    private readonly ITwinsClient twinsClient;
    private readonly AzureServiceBusQueueHealthCheck azureServiceBusQueueHealthCheck;
    private readonly IInputGraphManager inputGraphManager;
    private readonly ILogger logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="StartupHealthCheckService"/> class.
    /// </summary>
    /// <param name="healthCheckTwinsApi">A health check for the Twins API service.</param>
    /// <param name="healthCheckMappedApi">A health check for the Mapped API service.</param>
    /// <param name="twinsClient">An instance of a twins client for accessing the twins api.</param>
    /// <param name="azureServiceBusQueueHealthCheck">An instance of the AzureServiceBusQueueHealthCheck.</param>
    /// <param name="inputGraphManager">An instance of the Input Graph Manager to check health with.</param>
    /// <param name="logger">The logger.</param>
    public StartupHealthCheckService(
        HealthCheckTwinsApi healthCheckTwinsApi,
        HealthCheckMappedApi healthCheckMappedApi,
        ITwinsClient twinsClient,
        AzureServiceBusQueueHealthCheck azureServiceBusQueueHealthCheck,
        IInputGraphManager inputGraphManager,
        ILogger<StartupHealthCheckService> logger)
    {
        this.healthCheckTwinsApi = healthCheckTwinsApi ?? throw new ArgumentNullException(nameof(healthCheckTwinsApi));
        this.healthCheckMappedApi = healthCheckMappedApi;
        this.twinsClient = twinsClient;
        this.azureServiceBusQueueHealthCheck = azureServiceBusQueueHealthCheck;
        this.inputGraphManager = inputGraphManager;
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Check health.
    /// </summary>
    /// <param name="stoppingToken">The cancellation token to flag the process to stop.</param>
    /// <returns>An awaitable task.</returns>
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // See https://github.com/dotnet/runtime/issues/36063#issuecomment-518913079
        return Task.Run(async () =>
        {
            await Task.Yield();

            logger.LogInformation("Health Check service starting");

            // Delay between health checks - 2.5 minutes is enough for now
            TimeSpan delay = TimeSpan.FromSeconds(150);

            int totalRetries = 0;
            int maxRetries = 3;

            using (var timed = logger.TimeOperation("Check Health"))
            {
                //Retry until success or max retries have been reached
                while (!stoppingToken.IsCancellationRequested)
                {
                    var success = await CheckHealthStatus();

                    if (success || totalRetries > maxRetries)
                    {
                        break;
                    }

                    totalRetries++;

                    logger.LogInformation("Waiting {delay} before next health check. Total Retries {retries}/{maxRetries}", delay, totalRetries, maxRetries);

                    //wait a while
                    await Task.Delay(delay, stoppingToken);
                }
            }
        },
        stoppingToken);
    }

    private async Task<bool> CheckHealthStatus()
    {
        bool success = true;

        try
        {
            var context = new Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckContext();
            var token = new CancellationTokenSource(TimeSpan.FromSeconds(100));
            var twinsApiHealthCheck = CheckTwinsApiHealth();
            var mappedApiHealthCheck = CheckMappedApiHealth();
            var azureServiceBusHealthCheck = azureServiceBusQueueHealthCheck.CheckHealthAsync(context, token.Token);

            await Task.WhenAll(twinsApiHealthCheck, azureServiceBusHealthCheck, mappedApiHealthCheck);

            healthCheckTwinsApi.Current = HealthCheckTwinsApi.Healthy;
            healthCheckMappedApi.Current = HealthCheckMappedApi.Healthy;
        }
        catch (Exception ex)
        {
            success = false;
            logger.LogError(ex, "Startup health checks failed");
        }

        return success;
    }

    private async Task CheckTwinsApiHealth()
    {
        try
        {
            await twinsClient.GetTwinsAsync(new GetTwinsInfoRequest() { SourceType = SourceType.AdtQuery }, pageSize: 1);
        }
        catch
        {
            healthCheckTwinsApi.Current = HealthCheckTwinsApi.ConnectionFailed;
            throw;
        }
    }

    private async Task CheckMappedApiHealth()
    {
        try
        {
            var query = inputGraphManager.GetOrganizationQuery();
            var result = await inputGraphManager.GetTwinGraphAsync(query, cancellationToken: CancellationToken.None);
        }
        catch
        {
            healthCheckMappedApi.Current = HealthCheckMappedApi.ConnectionFailed;
            throw;
        }
    }
}
