namespace Willow.LiveData.Core.Features.Connectors.DTOs;

using System;
using Newtonsoft.Json;

/// <summary>
/// Capability Detail.
/// </summary>
public class CapabilityDetail
{
    /// <summary>
    /// Gets or sets the connector identifier.
    /// </summary>
    [JsonIgnore]
    public Guid ConnectorId { get; set; }

    /// <summary>
    /// Gets or sets the trend identifier.
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public Guid TrendId { get; set; }

    /// <summary>
    /// Gets or sets the twin identifier.
    /// </summary>
    public string TwinId { get; set; }

    /// <summary>
    /// Gets or sets the external identifier.
    /// </summary>
    public string ExternalId { get; set; }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the model.
    /// </summary>
    public string Model { get; set; }

    /// <summary>
    /// Gets or sets the ID of the twin that this capability is a capability of.
    /// </summary>
    public string IsCapabilityOf { get; set; }

    /// <summary>
    /// Gets or sets the ID of the twin that hosts this capability.
    /// </summary>
    public string IsHostedBy { get; set; }
}
