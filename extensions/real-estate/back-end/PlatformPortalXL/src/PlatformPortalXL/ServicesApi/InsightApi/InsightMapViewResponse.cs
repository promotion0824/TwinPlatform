using System.Collections.Generic;
using PlatformPortalXL.Models;

namespace PlatformPortalXL.ServicesApi.InsightApi;

public class InsightMapViewResponse:Insight
{
    public IEnumerable<InsightOccurrence> Occurrences { get; set; }
}
