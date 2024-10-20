namespace PlatformPortalXL.Models
{
    public class TimeSeriesAnalogData : TimeSeriesData
    {
        public decimal? Average { get; set; }
        public decimal? Minimum { get; set; }
        public decimal? Maximum { get; set; }
    }
}
