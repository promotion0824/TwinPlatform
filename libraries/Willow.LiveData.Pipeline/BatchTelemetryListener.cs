namespace Willow.LiveData.Pipeline;

using Microsoft.Extensions.Hosting;
using Willow.LiveData.Pipeline.EventHub;

/// <summary>
/// Listens to Event Hub for telemetry of the specified type and forwards it to the configured processor.
/// </summary>
/// <typeparam name="TTelemetry">The type of telemetry to receive.</typeparam>
internal class BatchTelemetryListener<TTelemetry>(IBatchProcessor batchProcessor)
    : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
        batchProcessor.StartProcessingAsync(stoppingToken);

    /// <inheritdoc />
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await batchProcessor.StopProcessingAsync(cancellationToken);

        await base.StopAsync(cancellationToken);
    }
}
