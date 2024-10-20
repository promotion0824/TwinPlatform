using PlatformPortalXL.Models;
using System.Collections.Generic;
using Willow.Batch;

namespace PlatformPortalXL.Dto
{
    public class InsightsCardsDto
    {
        public BatchDto<InsightCard> Cards { get; set; }
        public List<ImpactScore> ImpactScoreSummary { get; set; }
    }
}
