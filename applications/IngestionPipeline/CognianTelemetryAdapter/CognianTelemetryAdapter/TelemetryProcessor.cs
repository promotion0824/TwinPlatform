namespace Willow.CognianTelemetryAdapter;

using System.Collections.Generic;
using Willow.CognianTelemetryAdapter.Metrics;
using Willow.CognianTelemetryAdapter.Models;
using Willow.CognianTelemetryAdapter.Services;
using Willow.LiveData.Pipeline;

internal class TelemetryProcessor(ILogger<TelemetryProcessor> logger,
                          ITransformService transformService,
                          IMetricsCollector metricsCollector,
                          ISender sender,
                          HealthCheckTelemetryProcessor healthCheckTelemetryProcessor)
    : ITelemetryProcessor<CognianTelemetryMessage>
{
    public Task<(int Succeeded, int Failed, int Skipped)> ProcessAsync(CognianTelemetryMessage telemetry, CancellationToken cancellationToken = default) => ProcessAsync([telemetry], cancellationToken);

    public async Task<(int Succeeded, int Failed, int Skipped)> ProcessAsync(IEnumerable<CognianTelemetryMessage> batch, CancellationToken cancellationToken = default)
    {
        int skipped = 0;
        int processed = 0;
        List<Telemetry> processedMessages = [];

        foreach (var message in batch)
        {
            var processedMessage = transformService.ProcessMessage(message).ToList();
            if (processedMessage.Count > 0)
            {
                processedMessages.AddRange(processedMessage);
                processed++;
            }
            else
            {
                logger.LogDebug("Skipped processing message {Message}", message.ToString());
                skipped++;
            }
        }

        if (processedMessages.Count == 0)
        {
            return (0, 0, skipped);
        }

        try
        {
            await sender.SendAsync(processedMessages, cancellationToken);
            metricsCollector.TrackMessagesIngested(processedMessages.Count, null);
            return (processed, 0, skipped);
        }
        catch (PipelineException)
        {
            healthCheckTelemetryProcessor.Current = HealthCheckTelemetryProcessor.FailedToSend;
            return (0, batch.Count() - skipped, skipped);
        }
    }
}
