using System;
using System.Collections.Generic;
using InsightCore.Models;

namespace InsightCore.Infrastructure.Extensions;

public static class InsightStatusExtension
{
	public static InsightStatus Convert(this OldInsightStatus oldStatus)
	{
		switch (oldStatus)
		{
			case OldInsightStatus.Acknowledged:
				return InsightStatus.Ignored;
			case OldInsightStatus.Closed:
				return InsightStatus.Resolved;
			case OldInsightStatus.InProgress:
				return InsightStatus.InProgress;
			case OldInsightStatus.Open:
				return InsightStatus.Open;
			default:
				throw new Exception($"the old insight status ({oldStatus}) is not valid");
		}
	}
	public static List<InsightStatus> Convert(this IList<OldInsightStatus> oldStatuses)
	{
		var result = new List<InsightStatus>();
		foreach (var oldStatus in oldStatuses)
		{
			switch (oldStatus)
			{
				case OldInsightStatus.Acknowledged:
					result.Add(InsightStatus.Ignored);
					break;
				case OldInsightStatus.Closed:
					result.Add(InsightStatus.Resolved);
					break;
				case OldInsightStatus.InProgress:
					result.Add(InsightStatus.InProgress);
					break;
				case OldInsightStatus.Open:
				{
					result.Add(InsightStatus.Open);
					result.Add(InsightStatus.New);
				}
					break;
				default:
					throw new Exception($"the old insight status ({oldStatus}) is not valid");
			}
		}
		

		return result;
	}
	public static OldInsightStatus Convert(this InsightStatus status)
	{
		switch (status)
		{
			case InsightStatus.Ignored:
            case InsightStatus.Deleted:
                return OldInsightStatus.Acknowledged;
            case InsightStatus.Resolved:
				return OldInsightStatus.Closed;
			case InsightStatus.InProgress:
            case InsightStatus.ReadyToResolve:
				return OldInsightStatus.InProgress;
			case InsightStatus.Open:
			case InsightStatus.New:
				return OldInsightStatus.Open;
			default:
				throw new Exception($"the insight status ({status}) is not valid");
		}
	}
}
