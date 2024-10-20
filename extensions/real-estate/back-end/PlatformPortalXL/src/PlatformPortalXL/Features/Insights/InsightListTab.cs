using PlatformPortalXL.Models;
using System;
using System.Collections.Generic;
using Willow.Common;

namespace PlatformPortalXL.Features.Insights
{
    public enum InsightListTab
    {
        All,
        Open,
        Acknowledged,
        Closed
    }

	public static class InsightListTabExtenstions
	{
		public static IEnumerable<InsightStatus> MapTo(this InsightListTab tab)
		{
			return tab switch
			{
				InsightListTab.Open => InsightStatusGroups.Active,
				InsightListTab.Acknowledged => InsightStatusGroups.Ignored,
				InsightListTab.Closed => InsightStatusGroups.Resolved,
				InsightListTab.All => InsightStatusGroups.All,
				_ => throw new ArgumentException().WithData(new { tab }),
			};
		}
	}
}
