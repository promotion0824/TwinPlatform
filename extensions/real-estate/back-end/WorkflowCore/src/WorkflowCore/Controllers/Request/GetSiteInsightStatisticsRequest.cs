using System;
using System.Collections.Generic;

namespace WorkflowCore.Controllers.Request
{
    public class GetSiteInsightStatisticsRequest
    {
        public IList<Guid> SiteIds { get; set; }
		public IList<int> Statuses { get; set; }
		public bool? Scheduled { get; set; }
	}
}
