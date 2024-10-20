using MobileXL.Models.Enums;

namespace MobileXL.Models
{
    public class InspectionZoneStatistics
    {
        public int CompletedCheckCount { get; set; }
        public int WorkableCheckCount { get; set; }
        public CheckRecordStatus? WorkableCheckSummaryStatus { get; set; }
    }
}
