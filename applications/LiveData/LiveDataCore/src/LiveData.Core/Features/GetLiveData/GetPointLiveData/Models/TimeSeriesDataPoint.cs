namespace Willow.LiveData.Core.Features.GetLiveData.GetPointLiveData
{
    using System;
    using Willow.LiveData.Core.Domain;

    /// <summary>
    /// Time Series Data Point.
    /// </summary>
    public class TimeSeriesDataPoint : TimeSeriesData
    {
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public decimal? Value { get; set; }

        /// <summary>
        /// Gets or sets the point entity identifier.
        /// </summary>
        public Guid PointEntityId { get; set; }
    }
}
