namespace Willow.MappedTelemetryAdaptor;

using System.Collections.Generic;
using System.Text.Json;
using Willow.LiveData.Pipeline;
using Willow.MappedTelemetryAdaptor.Models;
using Willow.MappedTelemetryAdaptor.Services;

internal class TelemetryProcessor(
    ISender sender,
    HealthCheckTelemetryProcessor healthCheckTelemetryProcessor,
    ILogger<TelemetryProcessor> logger,
    IIdMapCacheService idMapCacheService)
    : ITelemetryProcessor<MappedInput>
{
    public Task<(int Succeeded, int Failed, int Skipped)> ProcessAsync(MappedInput telemetry, CancellationToken cancellationToken = default) => ProcessAsync([telemetry], cancellationToken);

    public async Task<(int Succeeded, int Failed, int Skipped)> ProcessAsync(IEnumerable<MappedInput> batch, CancellationToken cancellationToken = default)
    {
        List<Telemetry> outputBatch = [];
        var skipped = 0;
        var processed = 0;
        var failed = 0;

        foreach (var message in batch)
        {
            if (string.IsNullOrWhiteSpace(message.Value?.ToString()))
            {
                logger.LogWarning("Skipping processing of empty value in telemetry message for {PointId}, Timestamp: {Timestamp}", message.PointId, message.Timestamp);
                skipped++;
                continue;
            }

            try
            {
                outputBatch.Add(this.ProcessMappedTelemetry(message));
                processed++;
            }
            catch (Exception e)
            {
                logger.LogWarning("Failed to process telemetry message for {PointId}, Timestamp: {Timestamp}. Error: {Error}", message.PointId, message.Timestamp, e.Message);
                failed++;
            }
        }

        try
        {
            await sender.SendAsync(outputBatch, cancellationToken);
            return (processed, failed, skipped);
        }
        catch (PipelineException)
        {
            healthCheckTelemetryProcessor.Current = HealthCheckTelemetryProcessor.FailedToSend;
            return (0, batch.Count(), 0);
        }
    }

    public Telemetry ProcessMappedTelemetry(MappedInput message)
    {
        var value = message.Value;
        dynamic? properties = null;

        if (CheckJsonPayload(message.Value))
        {
            properties = message.Value;
            value = 1;
        }

        var connectorId = idMapCacheService.GetConnectorId(message.PointId);

        var telemetry = new Telemetry
        {
            ConnectorId = connectorId,
            ExternalId = message.PointId,
            SourceTimestamp = message.Timestamp,
            ScalarValue = value,
            EnqueuedTimestamp = DateTime.UtcNow,
            Properties = properties,
        };

        return telemetry;
    }

    private static bool CheckJsonPayload(dynamic? value)
    {
        //Check if the value is a valid JSON object
        try
        {
            var json = JsonSerializer.Deserialize<JsonElement>(value);
            return json.ValueKind == JsonValueKind.Object;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
