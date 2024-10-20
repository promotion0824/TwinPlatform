using PlatformPortalXL.Models;
using PlatformPortalXL.ServicesApi.InsightApi;
using System.Collections.Generic;
using System.Linq;

namespace PlatformPortalXL.Extensions
{
	public static class InsightExtensions
	{
		public static bool GetEcmDependency(this Insight insight, List<Insight> insights)
		{
			return insight.Type == InsightType.Fault && insights.Any(c =>
				c.PrimaryModelId == insight.PrimaryModelId && c.Type == InsightType.Energy);

		}

        public static bool GetEcmDependency(this Insight insight, List<InsightMapViewResponse> insights)
        {
            return insight.Type == InsightType.Fault && insights.Any(c =>
                c.PrimaryModelId == insight.PrimaryModelId && c.Type == InsightType.Energy);

        }
}
}
