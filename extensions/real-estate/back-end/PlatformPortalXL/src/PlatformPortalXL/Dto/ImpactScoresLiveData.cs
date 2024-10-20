using PlatformPortalXL.Models;
using System.Collections.Generic;

namespace PlatformPortalXL.Dto
{
    public class ImpactScoresLiveData
    {
        public string ExternalId { get; set; }
        public List<TimeSeriesAnalogData> TimeSeriesData { get; set; }
    }
}
