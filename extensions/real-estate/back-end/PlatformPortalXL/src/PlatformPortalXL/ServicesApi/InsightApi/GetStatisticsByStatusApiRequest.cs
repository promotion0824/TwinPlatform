using System.Collections.Generic;
using System;

namespace PlatformPortalXL.ServicesApi.InsightApi;

public class GetStatisticsByStatusApiRequest
{
    public List<Guid> SiteIds { get; set; }
}
