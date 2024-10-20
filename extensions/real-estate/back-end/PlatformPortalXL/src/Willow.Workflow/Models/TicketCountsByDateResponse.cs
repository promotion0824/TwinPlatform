using System;
using System.Collections.Generic;

namespace Willow.Workflow.Models;

/// <summary>
/// Represents the count of the created tickets for each day within a specified date range.
/// </summary>
public class TicketCountsByDateResponse
{

    public TicketCountsByDateResponse()
    {
        Counts = [];
    }

    /// <summary>
    /// Gets the count of created tickets for each day within a specified date range.
    /// </summary>
    public Dictionary<DateTime, int> Counts { get; set; }
}

