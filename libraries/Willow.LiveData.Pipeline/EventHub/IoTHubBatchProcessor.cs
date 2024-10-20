namespace Willow.LiveData.Pipeline.EventHub;

using System.Diagnostics.Metrics;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Willow.Telemetry;

internal class IoTHubBatchProcessor<TTelemetry>(
    ITelemetryProcessor<TTelemetry> telemetryProcessor,
    IEnumerable<IProcessorFilter> processorFilters,
    ILogger<BatchEventProcessor<TTelemetry>> logger,
    BlobContainerClient storageClient,
    Meter meter,
    MetricsAttributesHelper attributesHelper,
    HealthCheckTelemetryProcessor healthCheck,
    IOptions<Configuration.EventHubConfig> eventHubConfig,
    IConfiguration configuration)
    : IBatchProcessor
{
    private readonly BatchEventProcessor<TTelemetry> batchProcessor = new(telemetryProcessor, processorFilters, logger, storageClient, meter, attributesHelper, healthCheck, eventHubConfig, configuration);

    public Task StartProcessingAsync(CancellationToken cancellationToken) =>
        batchProcessor.StartProcessingAsync(cancellationToken);

    public Task StopProcessingAsync(CancellationToken cancellationToken) =>
        batchProcessor.StopProcessingAsync(cancellationToken);
}
