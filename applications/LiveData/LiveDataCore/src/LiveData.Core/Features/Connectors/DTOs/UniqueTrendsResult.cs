namespace Willow.LiveData.Core.Features.Connectors.DTOs;

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

/// <summary>
/// Unique Trends Result.
/// </summary>
public class UniqueTrendsResult
{
    /// <summary>
    /// Gets or sets the unique trends data.
    /// </summary>
    public IEnumerable<UniqueTrends> Data { get; set; } = Enumerable.Empty<UniqueTrends>();

    /// <summary>
    /// Gets or sets the error list.
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public ErrorData ErrorList { get; set; }
}
