using System;

namespace DigitalTwinCore.Features.TwinsSearch.Dtos;

public class BulkQueryRequest
{
    /// <summary>
    /// List of site ids to search
    /// </summary>
    public Guid[] SiteIds { get; set; }

    /// <summary>
    /// Stored query on ADX for paging
    /// </summary>
    public string QueryId { get; set; }

    /// <summary>
    /// List of Site and Twin ids pair
    /// </summary>
    public SiteTwinPair[] Twins { get; set; } = Array.Empty<SiteTwinPair>();
}