using Microsoft.AspNetCore.Mvc.ModelBinding;
using PlatformPortalXL.Features.Insights;
using PlatformPortalXL.Models;
using System;

namespace PlatformPortalXL.Requests.InsightCore
{
	/// <summary>
	/// Request to get insights for all sites
	/// </summary>
    public class GetInsightsV2Request
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
		/// Insight creation date
		/// </summary>
		public DateTime? CreatedDateFrom { get; set; }
		/// <summary>
		/// Optional flag to indicate whether to include the equipmentName of insight,
		/// if true, get the asset name of insights with valid equipment id
		/// if false or not set then return empty string for equipmentName 
		/// </summary>
		public bool IncludeEquipmentName { get; set; }
    }
}
