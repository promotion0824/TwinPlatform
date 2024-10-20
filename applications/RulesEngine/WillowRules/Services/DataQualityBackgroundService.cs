using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Willow.Rules.Logging;

namespace Willow.Rules.Services;

/// <summary>
/// Background service that listens for messages on DataQualityService to batch and process to the Willow ADT Api
/// </summary>
public class DataQualityBackgroundService : BackgroundService
{
    private readonly IDataQualityService dataQualityService;
    private readonly ILogger<DataQualityBackgroundService> logger;

    /// <summary>
    /// Creates a new <see cref="DataQualityBackgroundService" />
    /// </summary>
    public DataQualityBackgroundService(
        IDataQualityService dataQualityService,
        ILogger<DataQualityBackgroundService> logger)
    {
        this.dataQualityService = dataQualityService ?? throw new ArgumentNullException(nameof(dataQualityService));
        this.logger = logger;
    }

    /// <summary>
    /// Main loop for background service
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("DataQuality sender background service starting");

        var throttleLogger = logger.Throttle(TimeSpan.FromSeconds(30));

        //Continue reading messages else writer will fill queue
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var batch = await dataQualityService.Reader.ReadMultipleAsync(maxBatchSize: 500, cancellationToken: stoppingToken);

                using (var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(120)))
                {
                    var combinedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(tokenSource.Token, stoppingToken);
                    await dataQualityService.SendCapabilityStatusUpdate(batch, combinedCancellationTokenSource.Token);
                }

                throttleLogger.LogInformation("Sent a batch of {count} capabilities to ADT", batch.Count);
            }
            catch (OperationCanceledException)
            {
                // ignore, shutting down
            }
            catch (Exception ex)
            {
                throttleLogger.LogError(ex, "IDataQualityService failed to send batch to ADT");
            }
        }

        logger.LogInformation("DataQuality sender background service closing");
    }
}
