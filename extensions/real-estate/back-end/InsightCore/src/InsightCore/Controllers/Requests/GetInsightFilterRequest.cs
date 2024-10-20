using System;
using System.Collections.Generic;
using InsightCore.Models;

namespace InsightCore.Controllers.Requests;

public class GetInsightFilterRequest
{
    public List<Guid> SiteIds { get; set; }
    public List<InsightStatus> StatusList { get; set; }
}