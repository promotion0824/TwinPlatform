namespace Willow.LiveData.Core.Features.Connectors.DTOs;

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

/// <summary>
/// Missing Trends Result.
/// </summary>
public class MissingTrendsResult
{
    /// <summary>
    /// Gets or sets the missing trends data.
    /// </summary>
    public IEnumerable<MissingTrendsDetail> Data { get; set; } = Enumerable.Empty<MissingTrendsDetail>();

    /// <summary>
    /// Gets or sets the error list.
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public ErrorData ErrorList { get; set; }
}
