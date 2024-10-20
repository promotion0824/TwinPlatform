using System.Collections.Generic;
using System;

namespace PlatformPortalXL.ServicesApi.InsightApi;
public class InsightOccurrencesCountByDateResponse
{
    public List<InsightOccurrencesCountDto> Counts { get; set; }
    public int AverageDuration { get; set; }
}

public class InsightOccurrencesCountDto
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
}
