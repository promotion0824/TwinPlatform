namespace Willow.HealthChecks;

#pragma warning disable CS8618 // Nullable fields in DTO

using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

/// <summary>
/// Standard health check sent from healthz endpoint and between services.
/// </summary>
/// <remarks>
/// Tree-shaped result with subsystem health too.
/// </remarks>
public class HealthCheckDto
{
    private static readonly HealthCheckDto missing = new HealthCheckDto("missing", HealthStatus.Degraded, "Missing value", string.Empty, new Dictionary<string, HealthCheckDto>());

    private static readonly string version = System.Reflection.Assembly.GetEntryAssembly()?.GetName()?.Version?.ToString() ?? "not set";

    /// <summary>
    /// Initializes a new instance of the <see cref="HealthCheckDto"/> class.
    /// </summary>
    public HealthCheckDto(string key, HealthStatus status, string description, string version, IReadOnlyDictionary<string, HealthCheckDto>? entries)
    {
        this.Key = key;
        this.Status = status;
        this.Description = description;
        this.Version = version;
        this.Entries = entries;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HealthCheckDto"/> class.
    /// </summary>
    public HealthCheckDto(string key, string description, HealthReport report)
    {
        this.Key = key;
        this.Status = report.Status;
        this.Description = description;
        this.Version = version;
        this.Entries = !report.Entries.Any() ? null :
            report.Entries.ToDictionary(e => e.Key, e =>
                new HealthCheckDto(e.Key, e.Value.Status, e.Value.Description ?? string.Empty, e.Value.Data));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HealthCheckDto"/> class.
    /// Creates a new health check dto from an internal one where we have a dictionary of objects.
    /// </summary>
    public HealthCheckDto(string key, HealthStatus status, string description, IReadOnlyDictionary<string, object> entries)
    {
        this.Key = key;
        this.Status = status;
        this.Description = description;
        this.Version = entries.TryGetValue("Version", out var v) ? $"{v}" : null;
        this.Entries =
            !entries.Any() ? null :
            entries
                .Where(x => x.Key != "Version")
                .ToDictionary(x => x.Key, x => x.Value as HealthCheckDto ?? missing);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HealthCheckDto"/> class.
    /// Json constructor.
    /// </summary>
    public HealthCheckDto()
    {
    }

    /// <summary>
    /// Gets or sets unique key name for this health check result, can be used to recreate graph.
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// Gets or sets status.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))] // also allows integer values
    public HealthStatus Status { get; set; }

    /// <summary>
    /// Gets or sets description of the health status.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets assembly version of this component.
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// Gets or sets individual subsystem health states or null if no subsystems.
    /// </summary>
    public IReadOnlyDictionary<string, HealthCheckDto>? Entries { get; set; }

    /// <summary>
    /// Gets the entries as a dictionary of objects with any additional payload properties like version.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyDictionary<string, object>? EntriesWithPayload =>
                Entries is null ?
                    string.IsNullOrEmpty(Version) ? null // no entries and no version
                    :
                    new Dictionary<string, object>() { { "Version", this.Version } } // no entries, version
                    :
                    string.IsNullOrEmpty(Version) ? Entries.ToDictionary(x => x.Key, x => (object)x.Value) // entries, no version
                    :
                    Entries!.Select(x => (x.Key, (object)x.Value)) // entries and version
                        .Append(("Version", this.Version))
                        .ToDictionary(x => x.Item1, x => x.Item2);

    /// <summary>
    /// Visit every node in the tree.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<HealthCheckDto> SelfAndDescendants(string root)
    {
        yield return this;

        if (this.Entries is not null)
        {
            foreach ((var key, var descendant) in this.Entries)
            {
                foreach (var item in descendant.SelfAndDescendants(key))
                {
                    yield return item;
                }
            }
        }
    }
}

#pragma warning restore CS8618 // Nullable fields in DTO
