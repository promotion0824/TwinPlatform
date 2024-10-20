namespace Willow.LiveData.Core.Features.GetLiveData.GetPointLiveData
{
    using System.Collections.Generic;
    using Willow.LiveData.Core.Domain;

    /// <summary>
    /// Get Trendlog Result.
    /// </summary>
    public class GetTrendlogResult
    {
        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        public List<TimeSeriesRawData> Data { get; set; }

        /// <summary>
        /// Gets or sets the continuation token.
        /// </summary>
        public string ContinuationToken { get; set; }
    }
}
