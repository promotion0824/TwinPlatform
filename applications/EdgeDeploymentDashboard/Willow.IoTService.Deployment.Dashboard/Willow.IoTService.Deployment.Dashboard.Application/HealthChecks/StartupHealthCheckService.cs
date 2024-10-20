namespace Willow.IoTService.Deployment.Dashboard.Application.HealthChecks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Willow.Extensions.Logging;

/// <summary>
///     Startup health checks to external resources.
/// </summary>
public class StartupHealthCheckService(
    ILogger<StartupHealthCheckService> logger,
    HealthCheckServiceBus healthCheckServiceBus,
    HealthCheckSql healthCheckSql)
    : BackgroundService
{
    /// <summary>
    /// Healthcheck.
    /// </summary>
    /// <param name="stoppingToken">The cancellation token that can be used to stop the execution of the method.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // See https://github.com/dotnet/runtime/issues/36063#issuecomment-518913079
        return Task.Run(
            async () =>
        {
            await Task.Yield();

            logger.LogInformation("Health Check service starting");

            var delay = TimeSpan.FromSeconds(30);

            var totalRetries = 0;
            const int maxRetries = 3;

            using var timed = logger.TimeOperation("Check Health");
            while (!stoppingToken.IsCancellationRequested)
            {
                var success = await this.CheckHealthStatus();

                if (success || totalRetries > maxRetries)
                {
                    break;
                }

                totalRetries++;

                logger.LogInformation("Waiting {Delay} before next health check. Total Retries {Retries}/{MaxRetries}", delay, totalRetries, maxRetries);

                //wait a while
                await Task.Delay(delay, stoppingToken);
            }
        },
            cancellationToken: stoppingToken);
    }

    private async Task<bool> CheckHealthStatus()
    {
        var success = true;

        try
        {
            // Add any dependent resources health checks here
            healthCheckServiceBus.Current = HealthCheckServiceBus.Starting;
            healthCheckSql.Current = HealthCheckSql.Starting;

            // Temp line to avoid warning for not having an await in the method
            return await Task.FromResult(success);
        }
        catch (Exception ex)
        {
            success = false;
            logger.LogError(ex, "Startup health checks failed");
        }

        return success;
    }
}
