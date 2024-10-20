using PlatformPortalXL.Models;
using System.Collections.Generic;
using Willow.Batch;

namespace PlatformPortalXL.Dto
{
    public class InsightsDto
    {
        public BatchDto<InsightSimpleDto> Insights { get; set; }
        public List<ImpactScore> ImpactScoreSummary { get; set; }
    }
}
