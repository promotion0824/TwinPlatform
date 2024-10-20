namespace Willow.LiveData.Core.Domain;

using System;

/// <summary>
/// Represents statistics data for points.
/// </summary>
public class PointStatsData
{
    /// <summary>
    /// Gets or sets the SiteId.
    /// </summary>
    public Guid SiteId { get; set; }

    /// <summary>
    /// Gets or sets the Point count.
    /// </summary>
    public int PointsCount { get; set; }
}
