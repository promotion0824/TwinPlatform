using System;
using System.Collections.Generic;
using System.Linq;

namespace Willow.IoTService.Monitoring.Dtos.LiveDataCore;

public class MissingTrendsResult
{
    public IEnumerable<MissingTrendsDto>? Data { get; init; }
}

public class MissingTrendsDto
{
    public Guid ConnectorId { get; set; }

    public IEnumerable<MissingTrendsDetailDto>? Details { get; set; }
}

public class MissingTrendsDetailDto
{
    public Guid TrendId { get; set; }

    public string TwinId { get; set; } = string.Empty;

    public string ExternalId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public string IsCapabilityOf { get; set; } = string.Empty;

    public string IsCapabilityOfSecond { get; set; } = string.Empty;

    public string IsHostedBy { get; set; } = string.Empty;

    public string IsHostedBySecond { get; set; } = string.Empty;

    public override string ToString()
    {
        /// <summary>
        /// Take the first two subfields
        /// </summary>
        /// <param name="field">A comma separated string</param>
        /// <returns>The first two subfields</returns>
        (string? first, string? second) GetSubFields(string field)       // Comma separated list of subfields
        {
            var parts = field.Split(',');       // Split the field into subfields

            // Get the first two elements of the subfields array
            var first = parts.FirstOrDefault();
            var second = parts.Skip(1).FirstOrDefault();

            return (first, second);
        }

        // Get the subfields of IsCapabilityOf and IsHostedBy
        var (isCapabilityOfFirst, isCapabilityOfSecond) = GetSubFields(IsCapabilityOf);
        var (isHostedByFirst, isHostedBySecond) = GetSubFields(IsHostedBy);

        return ($"\"{TrendId}\",\"{TwinId}\",\"{ExternalId}\",\"{Name}\",\"{Model}\",\"{isCapabilityOfFirst}\",\"{isCapabilityOfSecond}\",\"{isHostedByFirst}\",\"{isHostedBySecond}\"");
    }

    // Build a comma separated list of props
    public static string GetProps()
    {
        var props = typeof(MissingTrendsDetailDto).GetProperties().Select(x => x.Name);
        var csv = string.Join(",", props);

        return csv;
    }
}
