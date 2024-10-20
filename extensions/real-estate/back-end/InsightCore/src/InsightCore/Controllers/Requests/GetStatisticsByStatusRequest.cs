using System;
using System.Collections.Generic;

namespace InsightCore.Controllers.Requests;

public class GetStatisticsByStatusRequest
{
    public List<Guid> SiteIds { get; set; }
}
