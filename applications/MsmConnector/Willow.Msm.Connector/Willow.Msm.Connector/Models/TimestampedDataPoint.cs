namespace Willow.Msm.Connector.Models
{
    /// <summary>
    /// Represents a data point with an associated time range and value, typically used for time-series data analysis.
    /// </summary>
    public class TimestampedDataPoint
    {
        /// <summary>
        /// Gets or sets the start date and time of the period covered by this data point.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date and time of the period covered by this data point.
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Gets or sets the numeric value associated with this data point. This value can be null if the data is not available.
        /// </summary>
        public decimal? Value { get; set; }
    }
}
