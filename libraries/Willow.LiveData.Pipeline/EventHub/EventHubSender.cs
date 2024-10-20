namespace Willow.LiveData.Pipeline.EventHub;

using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Azure.Messaging.EventHubs;
using Microsoft.Extensions.Logging;

/// <summary>
/// Sends telemetry to an Event Hub.
/// </summary>
/// <typeparam name="TTelemetry">The type of telemetry message this processor supports.</typeparam>
internal class EventHubSender<TTelemetry>(ILogger<EventHubSender<TTelemetry>> logger, EventHubClientFactory eventHubClientFactory)
    : ISender<TTelemetry>
{
    /// <inheritdoc />
    public Task SendAsync(TTelemetry telemetry, CancellationToken cancellationToken = default) =>
        SendAsync([telemetry], cancellationToken);

    /// <inheritdoc />
    public async Task SendAsync(IEnumerable<TTelemetry> batch, CancellationToken cancellationToken = default)
    {
        var currentBatch = new List<EventData>();
        var currentBatchSize = 0;
        const int sizeLimit = 1024 * 1024; // 1 MB https://learn.microsoft.com/en-us/azure/event-hubs/event-hubs-quotas

        foreach (var telemetry in batch)
        {
            var eventData = new EventData(JsonSerializer.Serialize(telemetry));

            // Estimate event size (considering a small overhead for metadata)
            var eventSize = Encoding.UTF8.GetByteCount(eventData.EventBody.ToString()) + 50;
            if (currentBatchSize + eventSize > sizeLimit)
            {
                await SendBatchAsync(currentBatch, cancellationToken);
                currentBatch.Clear();
                currentBatchSize = 0;
            }

            // Add event to the current batch
            currentBatch.Add(eventData);
            currentBatchSize += eventSize;
        }

        if (currentBatch.Count > 0)
        {
            await SendBatchAsync(currentBatch, cancellationToken);
        }
    }

    private async Task SendBatchAsync(IEnumerable<EventData> batch, CancellationToken cancellationToken)
    {
        try
        {
            var targetEventHubClient = eventHubClientFactory.CreateEventProducerClient();
            await targetEventHubClient.SendAsync(batch.ToArray(), cancellationToken);
            logger.LogDebug("Sent Telemetry to event hub");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending telemetry to Event Hub");
            throw new PipelineException("Error sending telemetry to Event Hub", ex);
        }
    }
}

/// <summary>
/// Sends default telemetry type to an Event Hub.
/// </summary>
internal class EventHubSender(ILogger<EventHubSender> logger, EventHubClientFactory eventHubClientFactory)
    : EventHubSender<Telemetry>(logger, eventHubClientFactory), ISender;
