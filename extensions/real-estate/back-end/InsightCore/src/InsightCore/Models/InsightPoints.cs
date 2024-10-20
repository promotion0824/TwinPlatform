using System.Collections.Generic;

namespace InsightCore.Models;
public class InsightPoints
{
    public InsightPoints()
    {
        ImpactScores = new List<ImpactScore>();
    }
    public string PointsJson { get; set; }
    public List<ImpactScore> ImpactScores { get; set; }
}

