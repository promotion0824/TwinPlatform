using System.Collections.Generic;

namespace PlatformPortalXL.ServicesApi.InsightApi
{
    public class TwinInsightStatisticsRequest
    {
        public List<string> TwinIds { get; set; }
        /// <summary>
        /// A temporary filter for Walmart 3dViewer Demo
        /// </summary>
        public string IncludeRuleId { get; set; }
        /// <summary>
        /// A temporary filter for Walmart 3dViewer Demo
        /// </summary>
        public string ExcludeRuleId { get; set; }

        public bool IncludePriorityCounts { get; set; }
    }
}
