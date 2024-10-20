using System.Collections.Generic;

namespace PlatformPortalXL.Models
{
    public class PointLiveDataAnalog : PointLiveDataBase
    {
        public List<TimeSeriesAnalogData> TimeSeriesData { get; set; }
    }
}
