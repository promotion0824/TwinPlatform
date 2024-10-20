using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Willow.HealthChecks;

#pragma warning disable CS8618 // Nullable fields in DTO

/// <summary>
/// A single report
/// </summary>
public struct Entry
{
    /// <summary>
    /// Gets or sets Extra data
    /// </summary>
    public Dictionary<string, object> Data { get; set; }

    /// <summary>
    /// Gets or sets Description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets Duration
    /// </summary>
    public string Duration { get; set; }

    /// <summary>
    /// Gets or sets Status
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))] // also allows integer values
    public HealthStatus Status { get; set; }

    /// <summary>
    /// Gets or sets Tags
    /// </summary>
    public List<object> Tags { get; set; }

    /// <summary>
    /// Converts an old Entry to a new HealthCheckDto
    /// </summary>
    /// <param name="key">Name of the section</param>
    /// <returns>A new healthcheckdto</returns>
    public HealthCheckDto AsHealthCheckDto(string key)
    {
        var report = new HealthReport(new Dictionary<string, HealthReportEntry>(), this.Status, TimeSpan.FromSeconds(1));
        return new HealthCheckDto(key, this.Description, report);
    }
}

/// <summary>
/// Health check returned by older /healthcheck endpoints
/// </summary>
public class HealthCheckOldStyle
{
    /// <summary>
    /// Gets or sets the overall status
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))] // also allows integer values
    public HealthStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the total duration
    /// </summary>
    public string TotalDuration { get; set; }

    /// <summary>
    /// Gets or sets the sub entries
    /// </summary>
    public Dictionary<string, Entry> Entries { get; set; }

    /// <summary>
    /// Gets the entries formatted for serialization
    /// </summary>
    [JsonIgnore]
    public Dictionary<string, HealthCheckDto> EntriesWithPayload => this.Entries.Where(x => x.Key != "Assembly Version")
        .ToDictionary(x => x.Key, x => x.Value.AsHealthCheckDto(x.Key));

    /// <summary>
    /// Gets the version from a dictionary entry
    /// </summary>
    [JsonIgnore]
    public string Version
    {
        get
        {
            if (Entries.TryGetValue("Assembly Version", out Entry version))
            {
                return version.Description!;
            }
            else
            {
                return "0.0.0.0";
            }
        }
    }

    /// <summary>
    /// Get the new format from the old format
    /// </summary>
    public HealthCheckDto AsHealthCheckDto(string key)
    {
        return new HealthCheckDto(key, this.Status, "No description", this.Version, this.EntriesWithPayload);
    }
}
