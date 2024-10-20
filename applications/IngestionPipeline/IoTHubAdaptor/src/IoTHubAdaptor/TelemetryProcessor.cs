namespace Willow.LiveData.IoTHubAdaptor;

using Willow.LiveData.IoTHubAdaptor.Models;
using Willow.LiveData.IoTHubAdaptor.Services;
using Willow.LiveData.Pipeline;

/// <summary>
/// Base worker class for processing telemetry.
/// </summary>
internal class TelemetryProcessor(ILogger<TelemetryProcessor> logger,
                          ITransformService transformService,
                          ISender sender,
                          HealthCheckTelemetryProcessor healthCheckTelemetryProcessor)
    : ITelemetryProcessor<UnifiedTelemetryMessage>
{
    public Task<(int Succeeded, int Failed, int Skipped)> ProcessAsync(UnifiedTelemetryMessage telemetry, CancellationToken cancellationToken = default) => ProcessAsync([telemetry], cancellationToken);

    public async Task<(int Succeeded, int Failed, int Skipped)> ProcessAsync(IEnumerable<UnifiedTelemetryMessage> batch, CancellationToken cancellationToken = default)
    {
        int skipped = 0;
        int processed = 0;
        List<Telemetry> processedMessages = [];

        foreach (var message in batch)
        {
            if (message is null ||
                string.IsNullOrEmpty(message.Version) ||
                message.Values is null ||
                (message.ConnectorId is not null && !Guid.TryParse(message.ConnectorId, out Guid _)))
            {
                //Skip processing non-standard telemetry - for example external events sent direct to IoTHub
                logger.LogWarning("Skipping processing of non-standard telemetry message: {Message}", message?.ToString());
                skipped++;
                continue;
            }

            var processMessage = transformService.ProcessMessage(message, out var skippedMessages).ToList();
            processedMessages.AddRange(processMessage);
            processed += processMessage.Count;
            skipped += skippedMessages;
        }

        if (processedMessages.Count != 0)
        {
            try
            {
                await sender.SendAsync(processedMessages, cancellationToken);

                logger.LogDebug("Processed {Count} messages", processedMessages.Count);

                return (processed, 0, skipped);
            }
            catch (PipelineException)
            {
                healthCheckTelemetryProcessor.Current = HealthCheckTelemetryProcessor.FailedToSend;
                return (0, processed, skipped);
            }
        }

        return (0, 0, skipped);
    }
}
