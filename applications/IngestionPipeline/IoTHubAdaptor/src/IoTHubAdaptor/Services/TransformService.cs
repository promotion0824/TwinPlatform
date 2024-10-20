namespace Willow.LiveData.IoTHubAdaptor.Services;

using System.Text.Json;
using System.Text.Json.Serialization;
using Willow.LiveData.IoTHubAdaptor.Models;

/// <summary>
/// Service to transform incoming IoTHub message to Unified EventHub message format.
/// </summary>
internal class TransformService(ILogger<TransformService> logger) : ITransformService
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        Converters = { new NullableDoubleConverter() },
    };

    public IEnumerable<Pipeline.Telemetry> ProcessMessage(UnifiedTelemetryMessage inputMessage, out int skipped)
    {
        string? connectorId = null;
        skipped = 0;

        if (inputMessage.ConnectorId is not null)
        {
            connectorId = inputMessage.ConnectorId;
        }

        List<Pipeline.Telemetry> unifiedTelemetryPointValues = [];

        switch (inputMessage.Version)
        {
            case SupportedTelemetryVersions.Version1:
                var values = JsonSerializer.Deserialize<List<PointValue>>(inputMessage.Values.ToString() ?? string.Empty, JsonSerializerOptions);

                if (values is null)
                {
                    break;
                }

                unifiedTelemetryPointValues.AddRange(values
                    .Where(p => p.Value is not null &&
                          (connectorId is not null || p.ConnectorId is not null) &&
                          (p.PointId is null || Guid.TryParse(p.PointId, out var _)))
                    .Select(value =>
                    {
                        var pointConnectorId = connectorId;

                        if (pointConnectorId is null && value.ConnectorId is not null)
                        {
                            pointConnectorId = value.ConnectorId;
                        }

#pragma warning disable CS0618 // Type or member is obsolete
                        return new Pipeline.Telemetry
                        {
                            ConnectorId = pointConnectorId,

                            // This is obsolete, but we need to keep it for now to support the old version of the pipeline.
                            TrendId = value.PointId is not null ? Guid.Parse(value.PointId) : null,
                            ScalarValue = value.Value,
                            ExternalId = value.PointExternalId,
                            SourceTimestamp = value.Timestamp,
                            EnqueuedTimestamp = DateTime.UtcNow,
                        };
#pragma warning restore CS0618 // Type or member is obsolete
                    }));

                skipped = unifiedTelemetryPointValues.Count - values.Count;
                if (skipped > 0)
                {
                    logger.LogWarning("Missing point values or ids in the collection. These will be ignored. Message: {InputMessage}", JsonSerializer.Serialize(inputMessage));
                }

                break;
            case SupportedTelemetryVersions.Version2:
                var unifiedValues = JsonSerializer.Deserialize<IEnumerable<Pipeline.Telemetry>>(inputMessage.Values.ToString() ?? string.Empty, JsonSerializerOptions);

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
                throw new NotSupportedException($"Provided version format {inputMessage.Version} not supported");
        }

        return unifiedTelemetryPointValues.Where(unifiedMsg => unifiedMsg.ConnectorId is not null);
    }
}
