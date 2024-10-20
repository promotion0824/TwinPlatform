namespace Willow.LiveData.TelemetryStreaming;

using System.Text;
using MQTTnet;
using MQTTnet.Extensions.ManagedClient;
using Willow.LiveData.Pipeline;
using Willow.LiveData.TelemetryStreaming.Metrics;
using Willow.LiveData.TelemetryStreaming.Models;
using Willow.LiveData.TelemetryStreaming.Services;

/// <summary>
/// Processes incoming telemetry and forwards to MQTT.
/// </summary>
/// <remarks>
/// 1. Get all subscriptions for the given connector and external ID.
/// 2. Transform the telemetry into the streaming format, adding any subscription-specific metadata.
/// 3. Publish to the correct MQTT topic for the subscription.
/// </remarks>
internal class TelemetryProcessor(ISubscriptionService subscriptionService, IManagedMqttClient mqttClient, IMetricsCollector metricsCollector, ILogger<TelemetryProcessor> logger) : ITelemetryProcessor
{
    public async Task<(int Succeeded, int Failed, int Skipped)> ProcessAsync(Pipeline.Telemetry telemetry, CancellationToken cancellationToken = default)
    {
        // Do not proceed if we have not matched a twin.
        if (string.IsNullOrEmpty(telemetry.ConnectorId?.ToString()) || string.IsNullOrEmpty(telemetry.ExternalId))
        {
            logger.LogDebug("Connector ID \"{cid}\" or external ID \"{eid}\" missing", telemetry.ConnectorId, telemetry.ExternalId);
            return (0, 0, Skipped: 1);
        }

        // Do not proceed if the scalar value is not a double.
        if (!double.TryParse(telemetry.ScalarValue?.ToString(), out double value))
        {
            logger.LogDebug("Scalar value is not a double. Unable to process");
            return (0, 0, Skipped: 1);
        }

        // Get all subscriptions for this twin.
        var subscriptions = await subscriptionService.GetSubscriptions(telemetry.ConnectorId.ToString()!, telemetry.ExternalId);

        logger.LogDebug("Processor: Found {count} matching subscriptions for connector {cid} and external ID {eid}", subscriptions.Length, telemetry.ConnectorId, telemetry.ExternalId);

        // Loop through all subscriptions and publish telemetry.
        await Parallel.ForEachAsync(subscriptions, cancellationToken, async (subscription, cancellationToken) =>
        {
            // Convert the raw telemetry to the streaming format.
            OutputTelemetry outputTelemetry = new()
            {
                ConnectorId = telemetry.ConnectorId.ToString()!,
                EnqueuedTimestamp = telemetry.EnqueuedTimestamp,
                ExternalId = telemetry.ExternalId,
                SourceTimestamp = telemetry.SourceTimestamp,
                Value = value,
                Metadata = subscription.Metadata,
            };

            var message = new MqttApplicationMessageBuilder()
                .WithTopic($"telemetry/{subscription.SubscriberId}/{telemetry.ConnectorId}/{telemetry.ExternalId}")
                .WithPayload(Encoding.ASCII.GetBytes(outputTelemetry.ToString()))
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            logger.LogDebug("Processor: Publishing telemetry to topic {topic}", message.Topic);
            await mqttClient.EnqueueAsync(message);

            var latencyMs = (DateTime.UtcNow - DateTime.SpecifyKind(telemetry.SourceTimestamp, DateTimeKind.Utc)).TotalMilliseconds;
            metricsCollector.TrackMqttTelemetryLatency(latencyMs, new Dictionary<string, string>
                   {
                       { "ConnectorId", telemetry.ConnectorId.ToString()! },
                       { "ExternalId", telemetry.ExternalId },
                   });
        });

        if (subscriptions.Length > 0)
        {
            return (Succeeded: 1, 0, 0);
        }

        return (0, 0, Skipped: 1);
    }

    public async Task<(int Succeeded, int Failed, int Skipped)> ProcessAsync(IEnumerable<Telemetry> batch, CancellationToken cancellationToken = default)
    {
        int succeeded = 0, failed = 0, skipped = 0;

        foreach (var telemetry in batch)
        {
            var res = await ProcessAsync(telemetry, cancellationToken);

            succeeded += res.Succeeded;
            failed += res.Failed;
            skipped += res.Skipped;
        }

        return (succeeded, failed, skipped);
    }
}
