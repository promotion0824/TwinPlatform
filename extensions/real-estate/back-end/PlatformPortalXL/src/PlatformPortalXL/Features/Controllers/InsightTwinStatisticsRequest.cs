using System.Collections.Generic;

namespace PlatformPortalXL.Features.Controllers;

public class InsightTwinStatisticsRequest: TwinStatisticsRequest
{
    public List<string> DtIds { get; set; }
    /// <summary>
    /// A temporary filter for Walmart 3dViewer Demo
    /// </summary>
    public string IncludeRuleId { get; set; }
    /// <summary>
    /// A temporary filter for Walmart 3dViewer Demo
    /// </summary>
    public string ExcludeRuleId { get; set; }
}
