using PlatformPortalXL.Models;
using System.Collections.Generic;
using System;

namespace PlatformPortalXL.ServicesApi.InsightApi;

public class GetInsightFilterApiRequest
{
    public List<Guid> SiteIds { get; set; }
    public List<InsightStatus> StatusList { get; set; }
}
