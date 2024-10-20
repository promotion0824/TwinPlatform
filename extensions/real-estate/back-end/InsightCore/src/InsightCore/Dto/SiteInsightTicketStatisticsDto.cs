using System;

namespace InsightCore.Dto;

public class SiteInsightTicketStatisticsDto
{
    public Guid Id { get; set; }
    public int OverdueCount { get; set; }
    public int ScheduledCount { get; set; }
    public int TotalCount { get; set; }
}

