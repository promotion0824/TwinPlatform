using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Willow.Workflow.Models;

public class TicketCategoricalData
{
    /// <summary>
    /// Ticket status list
    /// </summary>
    public List<string> TicketStatus { get; set; }
    /// <summary>
    /// Ticket sub status list
    /// </summary>
    public List<string> TicketSubStatus { get; set; }
    /// <summary>
    /// Ticket priorities list
    /// </summary>
    public List<string> Priorities { get; set; }
    /// <summary>
    /// Ticket  assignee types list
    /// </summary>
    public List<string> AssigneeTypes { get; set; }
    /// <summary>
    ///  Ticket job types list
    /// </summary>
    public List<TicketJobType> JobTypes { get; set; }

    /// <summary>
    /// Service needed list for each space twin
    /// </summary>
    public List<TicketSpaceTwinServiceNeeded> ServicesNeeded { get; set; }

    /// <summary>
    ///  Ticket request types list
    ///  Request type is equivalent to ticket categories
    /// </summary>
    public List<string> RequestTypes { get; set; }
}

public record TicketSpaceTwinServiceNeeded(string SpaceTwinId, List<TicketServiceNeeded> ServiceNeededList);
public record TicketServiceNeeded(Guid Id, string Name, Guid? CategoryId);
public record TicketJobType(Guid Id, string Name);

