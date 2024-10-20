using System;
using System.Collections.Generic;
using PlatformPortalXL.Models;

namespace PlatformPortalXL.Features.Insights;

public class GetInsightFilterRequest
{
    public List<Guid> SiteIds { get; set; }
    public List<InsightStatus> StatusList { get; set; }
    public string ScopeId { get; set; }
}
