using System;
using System.Collections.Generic;

namespace PlatformPortalXL.Dto;

public class TwinInsightStatisticsResponseDto
{
    public Guid? UniqueId { get; set; }
    public Guid? GeometryViewerId { get; set; }
    public string TwinId { get; set; }
    public int InsightCount { get; set; }
    public int HighestPriority { get; set; }
    public List<string> RuleIds { get; set; }
}

