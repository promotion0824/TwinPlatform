namespace Willow.LiveData.Pipeline;

using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Processor;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Willow.LiveData.Pipeline.EventHub;
using Willow.Telemetry;

/// <summary>
/// Listens to Event Hub for telemetry of the specified type and forwards it to the configured processor.
/// </summary>
/// <typeparam name="TTelemetry">The type of telemetry to receive.</typeparam>
internal class TelemetryListener<TTelemetry>(ILogger<TelemetryListener<TTelemetry>> logger,
    EventHubClientFactory eventProcessorClientFactory,
    ITelemetryProcessor<TTelemetry> telemetryProcessor,
    IOptions<Configuration.EventHubConfig> eventHubConfig,
    Meter meter,
    MetricsAttributesHelper metricsAttributesHelper)
    : BackgroundService
{
    private readonly Configuration.EventHubSource eventHubConfig = eventHubConfig.Value.Source ?? throw new ArgumentNullException(nameof(eventHubConfig.Value.Source));

    private readonly Counter<long> counter = meter.CreateCounter<long>("Messages", description: "The number of messages processed by the listener");
    private readonly Histogram<long> histogram = meter.CreateHistogram<long>("ProcessDuration", "milliseconds", "The time taken to process a message to completion");

    private readonly EventProcessorClient eventProcessorClient = eventProcessorClientFactory.CreateEventProcessorClient();

    private readonly JsonSerializerOptions jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        PropertyNamingPolicy = null,
        WriteIndented = true,
        AllowTrailingCommas = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        eventProcessorClient.ProcessEventAsync += ProcessEventHandler;
        eventProcessorClient.ProcessErrorAsync += ProcessErrorHandler;
        await eventProcessorClient.StartProcessingAsync(stoppingToken);
    }

    private async Task ProcessEventHandler(ProcessEventArgs args)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            TTelemetry message;
            if (eventHubConfig.CompressionEnabled)
            {
                message = await MessageProcessingHelper.DecompressAndDeserializeMessage<TTelemetry>(args.Data.EventBody.ToArray());
            }
            else
            {
                message = args.Data.EventBody.ToObjectFromJson<TTelemetry>(this.jsonSerializerOptions);
            }

            await telemetryProcessor.ProcessAsync(message);
            await args.UpdateCheckpointAsync();
            logger.LogDebug("Processed incoming message");
            counter.Add(1, metricsAttributesHelper.GetValues(new(Metrics.Action, Metrics.Processed),
                                                              new(Metrics.Status, Metrics.Succeeded)));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error processing incoming message: {eventBody}. Error: {message}", args.Data.EventBody, e.Message);
            counter.Add(1, metricsAttributesHelper.GetValues(new(Metrics.Action, Metrics.Processed),
                                                              new(Metrics.Status, Metrics.Failed)));
        }
        finally
        {
            stopwatch.Stop();
            histogram.Record(stopwatch.ElapsedMilliseconds, metricsAttributesHelper.GetValues(new KeyValuePair<string, object?>(Metrics.Action, Metrics.Processed)));
        }
    }

    /// <summary>
    /// Process errors that occur within the client itself, rather than the processing code.
    /// </summary>
    /// <param name="args">The error arguments.</param>
    /// <returns>A task.</returns>
    private Task ProcessErrorHandler(ProcessErrorEventArgs args)
    {
        counter.Add(1, metricsAttributesHelper.GetValues(new(Metrics.Action, Metrics.Processed),
                                                          new(Metrics.Status, Metrics.ClientError)));
        logger.LogError(args.Exception, "Error processing incoming message: {message}", args.Exception.Message);
        return Task.CompletedTask;
    }
}
