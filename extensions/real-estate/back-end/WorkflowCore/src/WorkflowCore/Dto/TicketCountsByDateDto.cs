using System;
using System.Collections.Generic;

namespace WorkflowCore.Dto;

/// <summary>
/// Represents the count of the created tickets for each day within a specified date range.
/// </summary>
public class TicketCountsByDateDto
{
    
    public TicketCountsByDateDto()
    {
        Counts = [];
    }

    /// <summary>
    /// Gets or sets the count of created tickets for each day within a specified date range.
    /// </summary>
    public Dictionary<DateTime, int> Counts { get; set; }
}


