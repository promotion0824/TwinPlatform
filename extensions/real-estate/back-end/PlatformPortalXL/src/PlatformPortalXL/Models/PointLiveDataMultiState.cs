using System.Collections.Generic;

namespace PlatformPortalXL.Models
{
    public class PointLiveDataMultiState : PointLiveDataBase
    {
        public List<TimeSeriesMulitStateData> TimeSeriesData { get; set; }
        public object ValueMap { get; set; }
    }
}
