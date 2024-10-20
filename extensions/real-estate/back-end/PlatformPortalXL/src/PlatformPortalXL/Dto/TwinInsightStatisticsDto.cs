using System;
using System.Collections.Generic;

namespace PlatformPortalXL.Dto;

public class PriorityCounts
{
    public int OpenCount { get; set; }
    public int UrgentCount { get; set; }
    public int HighCount { get; set; }
    public int MediumCount { get; set; }
    public int LowCount { get; set; }
}

public class TwinInsightStatisticsDto
{
    public string TwinId { get; set; }
    public int InsightCount { get; set; }
    public int HighestPriority { get; set; }
    public List<string> RuleIds { get; set; }
    public PriorityCounts PriorityCounts { get; set; }
}

