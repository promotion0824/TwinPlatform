namespace Willow.Msm.Connector.Models
{
    /// <summary>
    /// Represents a single data point in a time series.
    /// </summary>
    public class DataPoint
    {
        /// <summary>
        /// Gets or sets the timestamp for the data point.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the value of the data point.
        /// </summary>
        public decimal Value { get; set; }
    }
}
