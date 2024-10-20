namespace Willow.CognianTelemetryAdapter.Services;

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using Willow.CognianTelemetryAdapter.Models;
using Willow.CognianTelemetryAdapter.Options;
using Willow.LiveData.Pipeline;

/// <summary>
/// Service to transform incoming IoTHub message to Unified EventHub message format.
/// </summary>
internal class TransformService(IOptions<CognianAdapterOption> cognianAdapterSettings) : ITransformService
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        Converters = { new NullableDoubleConverter(), new DictionaryConverter() },
    };

    public IEnumerable<Telemetry> ProcessMessages(IEnumerable<CognianTelemetryMessage> inputMessages) =>
        inputMessages.SelectMany(this.ProcessMessage).ToList();

    public IEnumerable<Telemetry> ProcessMessage(CognianTelemetryMessage inputMessage)
    {
        const string luxDictKey = "lux";
        const string presenceDictKey = "presence";
        const string bleTagIdDictKey = "BLETagId";

        var values = new List<PointValue>();

        if (inputMessage is not { Topic: not null, Origin: not null })
        {
            return new List<Telemetry>();
        }

        if (inputMessage.Telemetry != null)
        {
            // Handling presence data
            if (inputMessage.Topic.Contains("/presence") && inputMessage.Telemetry.TryGetValue(presenceDictKey, out var presenceValue))
            {
                if (bool.TryParse(presenceValue.ToString(), out var presence))
                {
                    values.AddRange(Values($"{inputMessage.Origin.Device.DeviceId}-presence", TicksToDate(inputMessage.Timestamp), presence));
                }
            }

            // Handling lux data
            if (inputMessage.Topic.Contains("/lux") && inputMessage.Telemetry.TryGetValue(luxDictKey, out var luxValue))
            {
                if (int.TryParse(luxValue.ToString(), out var lux))
                {
                    values.AddRange(Values($"{inputMessage.Origin.Device.DeviceId}-Lux", TicksToDate(inputMessage.Timestamp), lux));
                }
            }

            // Handling BLETag presence
            if (inputMessage.Topic.Contains("/BLETag") && inputMessage.Telemetry.TryGetValue(bleTagIdDictKey, out var bleTagValue))
            {
                var bleTagIdJson = bleTagValue.ToString();
                if (bleTagIdJson != null)
                {
                    var bleTagId = JsonSerializer.Deserialize<CognianBLETag>(bleTagIdJson, JsonSerializerOptions);
                    if (bleTagId != null)
                    {
                        values.AddRange(Values($"{bleTagId.Uuid}-{bleTagId.Major}-{bleTagId.Minor}-presence", TicksToDate(inputMessage.Timestamp), 1));
                    }
                }
            }
        }

        // Handling multi-sensor data
        if (TopicMatchesPattern(inputMessage.Topic, pattern: "*/sensors/*/multi") && inputMessage.Telemetry != null)
        {
            values.AddRange(ToTrendDataMulti(inputMessage.Origin.Device.DeviceId, TicksToDate(inputMessage.Timestamp), inputMessage.Telemetry));
        }

        if (values.Count != 0)
        {
            return values.Select(
                value => new Telemetry
                {
                    EnqueuedTimestamp = DateTime.UtcNow,
                    SourceTimestamp = TicksToDate(inputMessage.Timestamp),
                    ConnectorId = cognianAdapterSettings.Value.ConnectorId,
                    ScalarValue = value.Value,
                    ExternalId = value.PointExternalId,
                });
        }

        return new List<Telemetry>();
    }

    private static bool TopicMatchesPattern(string topic, string pattern)
    {
        string regexPattern = "^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$";
        return Regex.IsMatch(topic, regexPattern, RegexOptions.IgnoreCase);
    }

    private static DateTime TicksToDate(long timestamp)
    {
        return DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime;
    }

    private static List<PointValue> Values(string pointExternalId, DateTime timestamp, object? value)
    {
        return
        [
            new PointValue { PointExternalId = pointExternalId, Timestamp = timestamp, Value = value }
        ];
    }

    private static List<PointValue> ToTrendDataMulti(string deviceId, DateTime timestamp, Dictionary<string, object> telemetry)
    {
        var propertiesToTrend = new HashSet<string>
        {
            "doorCount", "peopleCount", "autoReset", "pm25ug", "co2Ppm",
            "temperatureC", "humidityRelative", "detectedPersons",
            "detectionZonesPresent", "detectedPersonsZone",
        };

        var output = new List<PointValue>();

        foreach (var kvp in telemetry.Where(kvp => propertiesToTrend.Contains(kvp.Key)))
        {
            if (kvp is { Key: "detectedPersonsZone", Value: JsonElement { ValueKind: JsonValueKind.Array } zoneValues })
            {
                var i = 0;
                output.AddRange(from zoneValue in zoneValues.EnumerateArray() let externalId = $"{deviceId}-{kvp.Key}[{++i}]" select new PointValue { PointExternalId = externalId, Timestamp = timestamp, Value = zoneValue });
            }
            else
            {
                var externalId = $"{deviceId}-{kvp.Key}";
                output.Add(new PointValue { PointExternalId = externalId, Timestamp = timestamp, Value = kvp.Value });
            }
        }

        return output;
    }
}
