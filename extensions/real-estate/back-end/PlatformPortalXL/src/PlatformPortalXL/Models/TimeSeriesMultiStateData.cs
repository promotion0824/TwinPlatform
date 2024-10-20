using System.Collections.Generic;

namespace PlatformPortalXL.Models
{
    public class TimeSeriesMulitStateData : TimeSeriesData
    {
        public Dictionary<string, int> State { get; init; }
    }
}
