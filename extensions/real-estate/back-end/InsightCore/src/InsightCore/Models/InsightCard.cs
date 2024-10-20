using System;
using System.Collections.Generic;

namespace InsightCore.Models;

public class InsightCard
{
    public string RuleId { get; set; }
    public string RuleName { get; set; }
    public InsightType? InsightType { get; set; }  // insighttype - one per ruleid
    public int Priority { get; set; }
    public Guid? SourceId { get; set; }
    public string PrimaryModelId { get; set; }
    public string Recommendation { get; set; }
    public int InsightCount { get; set; }
    public DateTime LastOccurredDate { get; set; }
    public List<ImpactScore> ImpactScores { get; set; }
}
