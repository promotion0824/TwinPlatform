namespace Willow.LiveData.Core.Features.Connectors.DTOs;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

/// <summary>
/// Connector status result.
/// </summary>
[ExcludeFromCodeCoverage]
public class ConnectorStatusResult
{
    /// <summary>
    /// Gets or sets the connector stats.
    /// </summary>
    public IEnumerable<ConnectorStatsDto> Data { get; set; }

    /// <summary>
    /// Gets or sets a list of errors.
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public ErrorData ErrorList { get; set; }
}
