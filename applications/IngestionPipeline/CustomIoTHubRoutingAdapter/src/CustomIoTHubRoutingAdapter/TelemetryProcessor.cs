namespace Willow.CustomIoTHubRoutingAdapter;

using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Willow.CustomIoTHubRoutingAdapter.Models;
using Willow.CustomIoTHubRoutingAdapter.Options;
using Willow.LiveData.Pipeline;

/// <summary>
/// Base worker class for processing telemetry.
/// </summary>
internal class TelemetryProcessor(
    ILogger<TelemetryProcessor> logger,
    IOptions<ConnectorIdOption> connectorIdOption,
    ISender sender,
    HealthCheckTelemetryProcessor healthCheckTelemetryProcessor)
    : ITelemetryProcessor<UnifiedTelemetryMessage>
{
    private readonly List<string> connectorIdList = connectorIdOption.Value.ConnectorIdList.ToList();

    public Task<(int Succeeded, int Failed, int Skipped)> ProcessAsync(UnifiedTelemetryMessage telemetry, CancellationToken cancellationToken = default) => ProcessAsync([telemetry], cancellationToken);

    public async Task<(int Succeeded, int Failed, int Skipped)> ProcessAsync(IEnumerable<UnifiedTelemetryMessage> batch, CancellationToken cancellationToken = default)
    {
        var skipped = 0;
        var processed = 0;
        List<Telemetry> processedMessages = [];

        foreach (var message in batch)
        {
            if (string.IsNullOrEmpty(message.Version))
            {
                //Skip processing non-standard telemetry - for example external events sent direct to IoTHub
                skipped++;
                continue;
            }

            if (!connectorIdList.Contains(message.ConnectorId ?? string.Empty))
            {
                skipped++;
            }

            var processMessage = ProcessMessage(message, out var skippedMessages).ToList();
            processedMessages.AddRange(processMessage);
            processed += processMessage.Count;
            skipped += skippedMessages;
        }

        if (processedMessages.Count == 0)
        {
            return (0, 0, skipped);
        }

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

    private IEnumerable<Telemetry> ProcessMessage(UnifiedTelemetryMessage message, out int skipped)
    {
        skipped = 0;

        var connectorId = message.ConnectorId;

        List<Telemetry> unifiedTelemetryPointValues = [];

        switch (message.Version)
        {
            case SupportedTelemetryVersions.Version1:
                var values = JsonSerializer.Deserialize<List<PointValue>>(message.Values.ToString() ?? string.Empty);

                if (values is null)
                {
                    break;
                }

                unifiedTelemetryPointValues.AddRange(values
                    .Where(p => p.Value is not null &&
                          (connectorId is not null || p.ConnectorId is not null) &&
                          (p.PointId is null || Guid.TryParse(p.PointId, out _)))
                    .Select(value =>
                    {
                        var pointConnectorId = connectorId;

                        if (pointConnectorId is null && value.ConnectorId is not null)
                        {
                            pointConnectorId = value.ConnectorId;
                        }

                        return new Telemetry
                        {
                            ConnectorId = pointConnectorId,
                            ScalarValue = value.Value,
                            ExternalId = value.PointExternalId,
                            SourceTimestamp = value.Timestamp,
                            EnqueuedTimestamp = DateTime.UtcNow,
                        };
                    }));

                skipped = unifiedTelemetryPointValues.Count - values.Count;
                if (skipped > 0)
                {
                    logger.LogWarning("Missing point values or ids in the collection. These will be ignored. Message: {InputMessage}", JsonSerializer.Serialize(message));
                }

                break;
            case SupportedTelemetryVersions.Version2:
                var unifiedValues = JsonSerializer.Deserialize<IEnumerable<Telemetry>>(message.Values.ToString() ?? string.Empty);

                if (unifiedValues is null)
                {
                    skipped += 1;
                    break;
                }

                unifiedTelemetryPointValues.AddRange(unifiedValues.Select(value =>
                {
                    value.EnqueuedTimestamp = DateTime.UtcNow;
                    return value;
                }));

                break;
            default:
                // non-supported versions
                throw new NotSupportedException($"Provided version format {message.Version} not supported");
        }

        return unifiedTelemetryPointValues.Where(unifiedMsg => unifiedMsg.ConnectorId is not null);
    }
}
