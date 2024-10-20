using System;

namespace InsightCore.Models
{
	[Obsolete]
    public enum OldInsightStatus
    {
        Open = 0,
        Acknowledged = 10,
        InProgress = 20,
        Closed = 30,
        Deleted = 50
    }
}
