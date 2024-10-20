using System.Collections.Generic;
using WorkflowCore.Models;

namespace WorkflowCore.Controllers.Request;

public class TwinStatisticsRequest
{
    public List<string> TwinIds { get; set; }
    public List<SourceType> SourceTypes { get; set; }
}
