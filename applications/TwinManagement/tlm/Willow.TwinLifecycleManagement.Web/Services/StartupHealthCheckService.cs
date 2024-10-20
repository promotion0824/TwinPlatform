using Willow.Extensions.Logging;
using Willow.TwinLifecycleManagement.Web.Diagnostic;

/// <summary>
/// Startup health checks to external resources.
/// </summary>
public class StartupHealthCheckService : BackgroundService
{
    private readonly HealthCheckMTI _healthCheckMTI;
    private readonly ILogger _logger;
    private readonly HttpClient _mtiClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="StartupHealthCheckService"/> class.
    /// </summary>
    /// <param name="healthCheckMTI">A health check for MTI.</param>
    /// <param name="httpClientFactory">http client factory.</param>
    /// <param name="logger">The logger.</param>
    public StartupHealthCheckService(
        IHttpClientFactory httpClientFactory,
        HealthCheckMTI healthCheckMTI,
        ILogger<StartupHealthCheckService> logger)
    {
        _mtiClient = httpClientFactory.CreateClient("MTIAPI");
        _healthCheckMTI = healthCheckMTI;
        _logger = logger;
    }

    /// <summary>
    /// Check health.
    /// </summary>
    /// <param name="stoppingToken">The cancellation token to flag the process to stop.</param>
    /// <returns>An awaitable task.</returns>
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {

        // See https://github.com/dotnet/runtime/issues/36063#issuecomment-518913079
        return Task.Run(
            async () =>
        {
            await Task.Yield();

            _logger.LogInformation("Health Check service starting");

            TimeSpan delay = TimeSpan.FromSeconds(30);

            int totalRetries = 0;
            int maxRetries = 3;

            using (var timed = _logger.TimeOperation("Check Health"))
            {
                // Retry until success or max retries have been reached
                while (!stoppingToken.IsCancellationRequested)
                {
                    var success = await CheckHealthStatus();

                    if (success || totalRetries > maxRetries)
                    {
                        break;
                    }

                    totalRetries++;

                    _logger.LogInformation("Waiting {delay} before next health check. Total Retries {retries}/{maxRetries}", delay, totalRetries, maxRetries);

                    // wait a while
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
            var token = new CancellationTokenSource(TimeSpan.FromSeconds(100));
            var mtiHealthCheck = CheckMTIHealth();

            await Task.WhenAll(mtiHealthCheck);

            _healthCheckMTI.Current = HealthCheckMTI.Healthy;
        }
        catch (Exception ex)
        {
            success = false;
            _logger.LogError(ex, "Startup health checks failed");
        }

        return success;
    }

    private async Task CheckMTIHealth()
    {
        try
        {
            await MakeGetRequest("/Sync/HealthCheck");
        }
        catch
        {
            _healthCheckMTI.Current = HealthCheckMTI.FailingCalls;
            throw;
        }
    }

    private async Task<HttpResponseMessage> MakeGetRequest(string endpoint)
    {
        var urlBuilder = new System.Text.StringBuilder();
        urlBuilder.Append(endpoint);

        try
        {
            HttpResponseMessage response = await _mtiClient.GetAsync(urlBuilder.ToString());
            return response;
        }
        catch (Exception ex)
        {
            throw;
        }
    }
}
