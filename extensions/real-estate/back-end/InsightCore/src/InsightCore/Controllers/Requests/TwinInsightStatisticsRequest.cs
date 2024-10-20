using System.Collections.Generic;

namespace InsightCore.Controllers.Requests
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
