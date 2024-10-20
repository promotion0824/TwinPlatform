using PlatformPortalXL.Models;
using System.Collections.Generic;
using System;

namespace PlatformPortalXL.Features.Insights
{
	public class UpdateOldInsightsStatusRequest
	{
		public List<Guid> Ids { get; set; }
		public OldInsightStatus Status { get; set; }
	}
}
