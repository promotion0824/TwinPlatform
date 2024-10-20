namespace Willow.LiveData.Core.Features.GetLiveData.GetPointLiveData;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Willow.LiveData.Core.Domain;
using Willow.LiveData.Core.Features.Connectors.DTOs;

/// <summary>
/// Get telemetry result.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetTelemetryResult
{
    /// <summary>
    /// Gets or sets the data.
    /// </summary>
    public List<TelemetryData> Data { get; set; }

    /// <summary>
    /// Gets or sets the continuation token.
    /// </summary>
    public string ContinuationToken { get; set; }

    /// <summary>
    /// Gets or sets the error list.
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public ErrorData ErrorList { get; set; }
}
