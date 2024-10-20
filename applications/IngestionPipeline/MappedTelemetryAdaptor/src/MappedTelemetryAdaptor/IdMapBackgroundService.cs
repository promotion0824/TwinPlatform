namespace Willow.MappedTelemetryAdaptor;

using Microsoft.Extensions.Options;
using Willow.MappedTelemetryAdaptor.Options;
using Willow.MappedTelemetryAdaptor.Services;

/// <summary>
/// Pulls connector mapping in adx every X interval.
/// </summary>
internal sealed class IdMapBackgroundService(
    IOptions<IdMappingCacheOption> idMappingCacheOption,
    IIdMapCacheService idMapCacheService,
    ILogger<IdMapBackgroundService> logger)
    : BackgroundService
{
    private readonly PeriodicTimer timer = new(TimeSpan.FromSeconds(idMappingCacheOption.Value.RefetchCacheSeconds));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await idMapCacheService.LoadIdMapping(stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error loading id mapping");
                }
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Service has been canceled");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Id mapping service exited");
        }
    }

    public override void Dispose()
    {
        timer.Dispose();
        base.Dispose();
    }
}
