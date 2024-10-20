namespace Willow.IoTService.Deployment.Service.Application.HealthChecks;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
    ///     Check health.
    /// </summary>
    /// <param name="stoppingToken">Cancellation Token.</param>
    /// <returns>A <see cref="Task" /> representing running of health checks.</returns>
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
                        stoppingToken);
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

/// <summary>
///     Remove masstransit health checks.
/// </summary>
public class RemoveMasstransitHealthChecks : IConfigureOptions<HealthCheckServiceOptions>
{
    /// <summary>
    ///     Method to disable the default healthchecks added by MassTransit package.
    /// </summary>
    /// <param name="options">HealthCheckServiceOptions.</param>
    public void Configure(HealthCheckServiceOptions options)
    {
        var masstransitChecks = options.Registrations.Where(x => x.Tags.Contains("masstransit")).ToList();

        foreach (var check in masstransitChecks)
        {
            options.Registrations.Remove(check);
        }
    }
}
