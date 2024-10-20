using System.Collections.Generic;

namespace PlatformPortalXL.Models;

public enum InsightStatus
{

	Open = 0,
	Ignored = 10,
	InProgress = 20,
	Resolved = 30,
	New = 40,
	Deleted = 50,
    ReadyToResolve = 60
}

public static class InsightStatusGroups
{
	public static readonly List<InsightStatus> Active = new()
	{
		InsightStatus.New,
		InsightStatus.Open,
		InsightStatus.InProgress,
        InsightStatus.ReadyToResolve
	};

	public static readonly List<InsightStatus> Ignored = new()
	{
		InsightStatus.Ignored
	};

	public static readonly List<InsightStatus> Resolved = new()
	{
		InsightStatus.Resolved
	};

	public static readonly List<InsightStatus> All = new()
	{
		InsightStatus.New,
		InsightStatus.Open,
		InsightStatus.InProgress,
        InsightStatus.ReadyToResolve,
		InsightStatus.Ignored,
		InsightStatus.Resolved
	};
}
