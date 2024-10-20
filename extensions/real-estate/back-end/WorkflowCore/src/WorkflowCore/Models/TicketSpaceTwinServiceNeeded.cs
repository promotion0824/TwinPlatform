using System.Collections.Generic;

namespace WorkflowCore.Models;

public class TicketSpaceTwinServiceNeeded
{
    public string SpaceTwinId { get; set; }
    public List<TicketServiceNeeded> ServiceNeededList { get; set; }
}

