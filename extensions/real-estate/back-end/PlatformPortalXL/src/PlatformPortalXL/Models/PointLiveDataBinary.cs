using System.Collections.Generic;

namespace PlatformPortalXL.Models
{
    public class PointLiveDataBinary : PointLiveDataBase
    {
        public List<TimeSeriesBinaryData> TimeSeriesData { get; set; }
    }
}
