namespace Willow.LiveData.Core.Features.Connectors.DTOs;

using System;
using System.Collections.Generic;

/// <summary>
/// Missing Trends Detail.
/// </summary>
public class MissingTrendsDetail
{
    /// <summary>
    /// Gets or sets the connector identifier.
    /// </summary>
    public Guid ConnectorId { get; set; }

    /// <summary>
    /// Gets or sets the list of capability details.
    /// </summary>
    public List<CapabilityDetail> Details { get; set; }
}
