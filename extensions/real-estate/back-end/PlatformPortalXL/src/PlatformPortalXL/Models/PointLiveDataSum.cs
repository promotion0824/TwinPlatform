using System.Collections.Generic;

namespace PlatformPortalXL.Models
{
    public class PointLiveDataSum : PointLiveDataBase
    {
        public List<TimeSeriesSumData> TimeSeriesData { get; set; }
    }
}
