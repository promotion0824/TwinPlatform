namespace Willow.LiveData.Core.Domain;

using System;
using System.Collections.Generic;

/// <summary>
/// Get trendlogs result item.
/// </summary>
public class GetTrendlogsResultItem
{
    /// <summary>
    /// Gets or sets the point entity id.
    /// </summary>
    public Guid PointEntityId { get; set; }

    /// <summary>
    /// Gets or sets the data.
    /// </summary>
    public List<TimeSeriesRawData> Data { get; set; }
}
