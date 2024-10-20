namespace Willow.LiveData.Pipeline.EventHub;

using System;
using System.Diagnostics.Metrics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Core;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Primitives;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Willow.Extensions.Logging;
using Willow.Telemetry;

internal class BatchEventProcessor<TTelemetry>
    : PluggableCheckpointStoreEventProcessor<EventProcessorPartition>
{
    private const string IoTHubServiceConnectionString = "IoTHubService:ConnectionString";

    // Observability Metrics
    private readonly Histogram<long> checkpointUpdateDuration;
    private readonly Counter<long> messagesProcessedCounter;

    private readonly ILogger<BatchEventProcessor<TTelemetry>> logger;
    private readonly ITelemetryProcessor<TTelemetry> telemetryProcessor;
    private readonly IEnumerable<IProcessorFilter> processorFilters;
    private readonly MetricsAttributesHelper attributesHelper;
    private readonly HealthCheckTelemetryProcessor healthCheck;
    private readonly bool isCompressedData;

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
    };

    public BatchEventProcessor(
    ITelemetryProcessor<TTelemetry> telemetryProcessor,
    IEnumerable<IProcessorFilter> processorFilters,
    ILogger<BatchEventProcessor<TTelemetry>> logger,
    BlobContainerClient storageClient,
    Meter meter,
    MetricsAttributesHelper attributesHelper,
    HealthCheckTelemetryProcessor healthCheck,
    IOptions<Configuration.EventHubConfig> eventHubConfig,
    IConfiguration configuration)
    : base(new BlobCheckpointStore(storageClient), eventHubConfig.Value.Source!.MaxBatchSize, eventHubConfig.Value.Source.ConsumerGroup, configuration[IoTHubServiceConnectionString], eventHubConfig.Value.Source.Name, eventHubConfig.Value.Source.EventProcessorOptions)
    {
        this.telemetryProcessor = telemetryProcessor;
        this.logger = logger;
        this.attributesHelper = attributesHelper;
        this.healthCheck = healthCheck;
        this.processorFilters = processorFilters;
        this.isCompressedData = eventHubConfig.Value.Source.CompressionEnabled;

        messagesProcessedCounter = meter.CreateCounter<long>(Metrics.MessagesProcessed);
        checkpointUpdateDuration = meter.CreateHistogram<long>(Metrics.CheckpointUpdateDuration);
    }

    public BatchEventProcessor(
        ITelemetryProcessor<TTelemetry> telemetryProcessor,
        IEnumerable<IProcessorFilter> processorFilters,
        ILogger<BatchEventProcessor<TTelemetry>> logger,
        BlobContainerClient storageClient,
        Meter meter,
        MetricsAttributesHelper attributesHelper,
        HealthCheckTelemetryProcessor healthCheck,
        IOptions<Configuration.EventHubConfig> eventHubConfig,
        TokenCredential credential)
    : base(new BlobCheckpointStore(storageClient), eventHubConfig.Value.Source!.MaxBatchSize, eventHubConfig.Value.Source.ConsumerGroup, eventHubConfig.Value.Source.FullyQualifiedNamespace, eventHubConfig.Value.Source.Name, credential, eventHubConfig.Value.Source.EventProcessorOptions)
    {
        this.telemetryProcessor = telemetryProcessor;
        this.logger = logger;
        this.attributesHelper = attributesHelper;
        this.healthCheck = healthCheck;
        this.processorFilters = processorFilters;
        this.isCompressedData = eventHubConfig.Value.Source.CompressionEnabled;

        messagesProcessedCounter = meter.CreateCounter<long>(Metrics.MessagesProcessed);
        checkpointUpdateDuration = meter.CreateHistogram<long>(Metrics.CheckpointUpdateDuration);
    }

    protected override async Task OnProcessingEventBatchAsync(IEnumerable<EventData?> events, EventProcessorPartition partition, CancellationToken cancellationToken)
    {
        var eventArray = events as EventData[] ?? events.ToArray();
        try
        {
            List<TTelemetry> eventBodies = [];
            var filteredEvents = eventArray.Where(c => c is not null && processorFilters.All(filter => filter.OnDataReceived(c))).ToList();
            var skippedEvents = eventArray.Length - filteredEvents.Count;

            // Get the event bodies
            foreach (var eventData in filteredEvents)
            {
                var (parsed, eventBody) = await TryParse(eventData!.EventBody, isCompressedData);
                if (parsed)
                {
                    eventBodies.Add(eventBody);
                }
            }

            // Batch process the events
            var (succeeded, failed, skipped) = await telemetryProcessor.ProcessAsync(eventBodies, cancellationToken);

            skipped += skippedEvents;

            messagesProcessedCounter.Add(succeeded, attributesHelper.GetValues(new KeyValuePair<string, object?>(Metrics.StatusDimensionName, Metrics.SuccessStatus)));
            messagesProcessedCounter.Add(skipped, attributesHelper.GetValues(new KeyValuePair<string, object?>(Metrics.StatusDimensionName, Metrics.SkippedStatus)));
            messagesProcessedCounter.Add(failed, attributesHelper.GetValues(new KeyValuePair<string, object?>(Metrics.StatusDimensionName, Metrics.FailedStatus)));

            // Infer the number of parse failures
            messagesProcessedCounter.Add(eventArray.Length - skippedEvents - eventBodies.Count, attributesHelper.GetValues(new KeyValuePair<string, object?>(Metrics.StatusDimensionName, Metrics.ParseErrorStatus)));

            var lastEvent = eventArray.Where(e => e is not null).LastOrDefault();
            if (lastEvent is not null)
            {
                await MeasureExecutionTime.ExecuteTimed(
                                                    async () =>
                                                    {
                                                        await UpdateCheckpointAsync(partition.PartitionId, lastEvent.Offset, lastEvent.SequenceNumber, cancellationToken);
                                                        return true;
                                                    },
                                                    (_, ms) =>
                                                    {
                                                        checkpointUpdateDuration.Record(ms);
                                                        logger.LogDebug("Updating checkpoint storage took {Time} milliseconds", ms);
                                                    });
            }

            // Batch processed successfully, app is healthy.
            healthCheck.Current = HealthCheckTelemetryProcessor.Healthy;
        }
        catch (Exception e)
        {
            logger.LogDebug(e, "Error processing incoming batch: Error: {ErrorMessage}", e.Message);
            healthCheck.Current = HealthCheckTelemetryProcessor.FailedToProcess;
            messagesProcessedCounter.Add(
                                              eventArray.Length,
                                              attributesHelper.GetValues(new KeyValuePair<string, object?>(Metrics.StatusDimensionName, Metrics.FailedStatus)));
        }
    }

    protected override async Task OnProcessingErrorAsync(Exception exception, EventProcessorPartition partition, string operationDescription, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "An unexpected error occurred while processing on {@Partition}: {ExceptionMessage}. Operation description: {Description}", partition, exception.Message, operationDescription);
        messagesProcessedCounter.Add(
                                          1,
                                          attributesHelper.GetValues(new KeyValuePair<string, object?>(Metrics.StatusDimensionName, Metrics.FailedStatus)));

        //Temp line to avoid warning for not having an await in the method
        await Task.FromResult(true);
    }

    private static async Task<(bool Success, TTelemetry Telemetry)> TryParse(BinaryData eventBody, bool isCompressedData)
    {
        TTelemetry telemetry;
        try
        {
            if (isCompressedData)
            {
                telemetry = await MessageProcessingHelper.DecompressAndDeserializeMessage<TTelemetry>(eventBody.ToArray());
            }
            else
            {
                telemetry = eventBody.ToObjectFromJson<TTelemetry>(JsonSerializerOptions);
            }

            return (true, telemetry);
        }
        catch (JsonException)
        {
            telemetry = default!;
            return (false, telemetry);
        }
    }
}
