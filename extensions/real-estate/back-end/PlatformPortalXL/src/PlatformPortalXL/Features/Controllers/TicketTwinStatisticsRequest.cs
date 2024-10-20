using System.Collections.Generic;
using Willow.Workflow;

namespace PlatformPortalXL.Features.Controllers;

public class TicketTwinStatisticsRequest: TwinStatisticsRequest
{
    public List<string>? DtIds { get; set; }
    public List<TicketSourceType> SourceTypes { get; set; }
}
