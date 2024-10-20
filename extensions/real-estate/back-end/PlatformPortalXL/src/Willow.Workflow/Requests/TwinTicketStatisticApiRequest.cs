
using System.Collections.Generic;

namespace Willow.Workflow.Requests;

public class TwinTicketStatisticApiRequest
{
    public List<string> TwinIds { get; set; }
    public List<TicketSourceType> SourceTypes { get; set; }
}
