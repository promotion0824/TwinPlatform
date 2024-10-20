using Microsoft.AspNetCore.Mvc.ModelBinding;
using PlatformPortalXL.Features.Insights;
using PlatformPortalXL.Models;
using System;

namespace PlatformPortalXL.Requests.InsightCore
{
	/// <summary>
	/// Request to get insights for all sites
	/// </summary>
    public class GetInsightsRequest
    {
		/// <summary>
		/// Required insight status
		/// </summary>
        [BindRequired]
        public InsightListTab Tab { get; set; }
		/// <summary>
		/// The source of the insight
		/// </summary>
        public InsightSourceType? SourceType { get; set; }
		/// <summary>
		/// Insight last occurred date
		/// </summary>
		public DateTime? LastOccurredDateFrom { get; set; }
		/// <summary>
		/// Insight creation date
		/// </summary>
		public DateTime? CreatedDateFrom { get; set; }
		/// <summary>
		/// The insights' site Id
		/// </summary>
		public Guid? SiteId { get; set; }
	}
}
