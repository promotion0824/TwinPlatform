using System;

namespace WorkflowCore.Services.MappedIntegration.Dtos.Requests;

public class MappedGetTicketsRequest
{
    /// <summary>
    /// Ticket external id
    /// </summary>
    public string ExternalId { get; set; }
    /// <summary>
    /// Ticket source id
    /// sourceId is equivalent to app id
    /// </summary>
    public Guid? SourceId { get; set; }
    /// <summary>
    /// Ticket created after UTC date time
    /// </summary>
    public DateTime? CreatedAfter { get; set; }
}

