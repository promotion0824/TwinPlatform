using System;
using System.Collections.Generic;

namespace WorkflowCore.Controllers.Request
{
    public class GetInsightStatisticsRequest
    {
        public IList<Guid> InsightIds { get; set; }
		public IList<int> Statuses { get; set; }
		public bool? Scheduled { get; set; }
	}
}
