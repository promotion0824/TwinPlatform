namespace Willow.MappedTelemetryAdaptor.HealthChecks;

using Willow.Extensions.Logging;

/// <summary>
///     Startup health checks to external resources.
/// </summary>
public class StartupHealthCheckService : BackgroundService
{
    private readonly ILogger logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="StartupHealthCheckService" /> class.
    /// </summary>
    /// <param name="logger">Logger.</param>
    /// <exception cref="ArgumentNullException">Throws exception when logger is not provided.</exception>
    public StartupHealthCheckService(ILogger<StartupHealthCheckService> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    ///     Check health.
    /// </summary>
    /// <param name="stoppingToken">Cancellation Token.</param>
    /// <returns>A <see cref="Task" /> representing execution of health check.</returns>
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // See https://github.com/dotnet/runtime/issues/36063#issuecomment-518913079
        return Task.Run(
                        async () =>
                        {
                            await Task.Yield();

                            this.logger.LogInformation("Health Check service starting");

                            var delay = TimeSpan.FromSeconds(30);

                            var totalRetries = 0;
                            const int maxRetries = 3;

                            using var timed = this.logger.TimeOperation("Check Health");
                            while (!stoppingToken.IsCancellationRequested)
                            {
                                var success = await this.CheckHealthStatus();

                                if (success || totalRetries > maxRetries)
                                {
                                    break;
                                }

                                totalRetries++;

                                this.logger.LogInformation("Waiting {Delay} before next health check. Total Retries {Retries}/{MaxRetries}", delay, totalRetries, maxRetries);

                                //wait a while
                                await Task.Delay(delay, stoppingToken);
                            }
                        },
                        stoppingToken);
    }

    private async Task<bool> CheckHealthStatus()
    {
        var success = true;

        try
        {
            var token = new CancellationTokenSource(TimeSpan.FromSeconds(100));

            // Add any dependent resources health checks here
            // Temp line to avoid warning for not having an await in the method
            return await Task.FromResult(success);
        }
        catch (Exception ex)
        {
            success = false;
            this.logger.LogError(ex, "Startup health checks failed");
        }

        return success;
    }
}
