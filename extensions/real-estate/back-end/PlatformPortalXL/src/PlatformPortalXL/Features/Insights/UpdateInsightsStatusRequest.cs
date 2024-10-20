using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PlatformPortalXL.Models;

namespace PlatformPortalXL.Features.Insights
{
    public class UpdateInsightStatusRequest
    {
		public List<Guid> Ids { get; set; }

		[Required(ErrorMessage = "the insight status is required")]
		public InsightStatus? Status { get; set; }

		public string Reason { get; set; }
        public string ScopeId { get; set; }
	}
}
