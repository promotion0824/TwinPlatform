namespace Willow.LiveData.Core.Features.GetLiveData.GetPointLiveData
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Willow.LiveData.Core.Domain;

    /// <summary>
    /// Represents a service for retrieving live data from ADX.
    /// </summary>
    public interface IAdxLiveDataService
    {
        /// <summary>
        /// Retrieves time series data for a specific TrendId.
        /// </summary>
        /// <param name="clientId">The client ID.</param>
        /// <param name="start">The start date and time of the time series data.</param>
        /// <param name="end">The end date and time of the time series data.</param>
        /// <param name="type">The type of time series data.</param>
        /// <param name="trendId">The trendId of the point.</param>
        /// <param name="selectedInterval">The selected time interval for the time series data.</param>
        /// <returns>A collection of time series data for the specified trendId.</returns>
        Task<IEnumerable<TimeSeriesData>> GetTimeSeriesDataByTrendIdAsync(
            Guid? clientId,
            DateTime start,
            DateTime end,
            string type,
            Guid trendId,
            TimeSpan? selectedInterval);

        /// <summary>
        /// Retrieves time series data for an array of TrendIds.
        /// </summary>
        /// <param name="clientId">The client ID.</param>
        /// <param name="start">The start date and time of the time series data.</param>
        /// <param name="end">The end date and time of the time series data.</param>
        /// <param name="type">The type of time series data.</param>
        /// <param name="trendIds">An array of trendIds.</param>
        /// <param name="selectedInterval">The selected time interval for the time series data.</param>
        /// <returns>A dictionary with trendIds as keys and a list of time series data as values.</returns>
        Task<Dictionary<Guid, List<TimeSeriesData>>> GetTimeSeriesDataByTrendIdAsync(
            Guid? clientId,
            DateTime start,
            DateTime end,
            string type,
            IEnumerable<Guid> trendIds,
            TimeSpan? selectedInterval);

        /// <summary>
        /// Retrieves time series data for an array of TrendIds.
        /// </summary>
        /// <param name="clientId">The client ID.</param>
        /// <param name="start">The start date and time of the time series data.</param>
        /// <param name="end">The end date and time of the time series data.</param>
        /// <param name="trendIds">An array of point entity IDs.</param>
        /// <param name="types">An array of point types.</param>
        /// <param name="selectedInterval">The selected time interval for the time series data.</param>
        /// <returns>A dictionary containing point entity IDs as keys and lists of time series data as values.</returns>
        /// <remarks>
        /// The returned dictionary maps each point entity ID to a list of time series data for that point.
        /// </remarks>
        Task<Dictionary<Guid, List<TimeSeriesData>>> GetTimeSeriesDataByTrendIdAsync(
            Guid? clientId,
            DateTime start,
            DateTime end,
            Guid[] trendIds,
            string[] types,
            TimeSpan? selectedInterval);

        /// <summary>
        /// Retrieves raw time series data for a specific TrendId.
        /// </summary>
        /// <param name="clientId">The client ID.</param>
        /// <param name="trendId">The trendId of the point.</param>
        /// <param name="start">The start date and time of the time series data.</param>
        /// <param name="end">The end date and time of the time series data.</param>
        /// <param name="continuationToken">The continuation token for paging through the data.</param>
        /// <param name="pageSize">The maximum number of data points to retrieve per page.</param>
        /// <returns>The result of retrieving the raw time series data.</returns>
        Task<GetTrendlogResult> GetTimeSeriesRawDataByTrendIdAsync(
            Guid? clientId,
            Guid trendId,
            DateTime start,
            DateTime end,
            string continuationToken,
            int? pageSize);

        /// <summary>
        /// Retrieves time series raw data for a specific siteId.
        /// </summary>
        /// <param name="clientId">The client ID.</param>
        /// <param name="siteId">The siteId of the point.</param>
        /// <param name="start">The start date and time of the time series data.</param>
        /// <param name="end">The end date and time of the time series data.</param>
        /// <param name="trendIds">A list of trendIds to filter the time series data.</param>
        /// <returns>A collection of time series raw data for the specified siteId.</returns>
        Task<IEnumerable<GetTrendlogsResultItem>> GetTimeSeriesRawDataBySiteIdAsync(
            Guid? clientId,
            Guid siteId,
            DateTime start,
            DateTime end,
            List<Guid> trendIds = null);

        /// <summary>
        /// Retrieves the last time series raw data for a specific site ID.
        /// </summary>
        /// <param name="clientId">The client ID.</param>
        /// <param name="siteId">The site ID.</param>
        /// <param name="trendIds">The list of trend IDs. If not provided, all trend IDs will be considered.</param>
        /// <returns>A collection of the last time series raw data for the specified site ID.</returns>
        Task<IEnumerable<PointTimeSeriesRawData>> GetLastTimeSeriesRawDataBySiteIdAsync(
            Guid? clientId,
            Guid siteId,
            List<Guid> trendIds = null);

        /// <summary>
        /// Point stats data for a specific client ID.
        /// </summary>
        /// <param name="clientId">The client ID.</param>
        /// <param name="start">The start date and time of the time series data.</param>
        /// <returns>A list of points stats data.</returns>
        Task<List<PointStatsData>> GetPointStatsAsync(Guid? clientId, DateTime start);

        /// <summary>
        /// Gets the point stats data for a specific client ID and a list of trend IDs.
        /// </summary>
        /// <param name="clientId">The client ID.</param>
        /// <param name="start">The start date and time of the time series data.</param>
        /// <param name="trendIds">The trend IDs.</param>
        /// <returns>The count of point stats.</returns>
        Task<int> GetPointsStatsCountByIdsListAsync(Guid? clientId, DateTime start, IEnumerable<Guid> trendIds);

        /// <summary>
        /// Retrieves the historical last time series raw data for the specified TrendIds within a specified time range.
        /// </summary>
        /// <param name="clientId">The client ID.</param>
        /// <param name="start">The start date and time of the time series data.</param>
        /// <param name="end">The end date and time of the time series data.</param>
        /// <param name="trendIds">The list of TrendIds for which to retrieve the data. If null, retrieves data for all points.</param>
        /// <returns>A read-only list of PointTimeSeriesRawData objects containing the historical last time series raw data for the specified point IDs.</returns>
        Task<IReadOnlyList<PointTimeSeriesRawData>> GetHistoricalLastTimeSeriesRawDataAsync(
            Guid? clientId,
            DateTime start,
            DateTime end,
            List<Guid> trendIds = null);

        /// <summary>
        /// Retrieves the aggregated values for the specified TrendIds within a specified time range.
        /// </summary>
        /// <param name="clientId">The client ID.</param>
        /// <param name="start">The start date and time of the time series data.</param>
        /// <param name="end">The end date and time of the time series data.</param>
        /// <param name="interval">The interval.</param>
        /// <param name="trendIds">The list of TrendIds for which to retrieve the data. If null, retrieves data for all points.</param>
        /// <param name="aggregationType">The type of aggregation.</param>
        /// <returns>A read-only dictionary containing the aggregated values for the specified TrendIds within the specified time range.</returns>
        Task<IReadOnlyDictionary<DateTime, IReadOnlyDictionary<Guid, decimal?>>> GetAggregatedValuesInsideTimeIntervalsAsync(
            Guid? clientId,
            DateTime start,
            DateTime end,
            TimeSpan interval,
            List<Guid> trendIds,
            AggregationType aggregationType);

        /// <summary>
        /// Retrieves the cumulative time series data for the specified TrendIds within a specified time range.
        /// </summary>
        /// <param name="clientId">The client ID.</param>
        /// <param name="start">The start date and time of the time series data.</param>
        /// <param name="end">The end date and time of the time series data.</param>
        /// <param name="interval">The interval.</param>
        /// <param name="trendIds">The list of TrendIds for which to retrieve the data. If null, retrieves data for all points.</param>
        /// <param name="valueMultiplier">The value multiplier.</param>
        /// <returns>A read-only collection of CumulativeTimeSeriesDataPoint objects containing the cumulative time series data for the specified TrendIds within the specified time range.</returns>
        Task<IReadOnlyCollection<CumulativeTimeSeriesDataPoint>> GetCumulativeTrendAsync(
            Guid? clientId,
            DateTime start,
            DateTime end,
            TimeSpan interval,
            List<Guid> trendIds,
            double valueMultiplier);

        /// <summary>
        /// Retrieves the cumulative sum of the time series data for the specified TrendIds within a specified time range.
        /// </summary>
        /// <param name="clientId">The client ID.</param>
        /// <param name="start">The start date and time of the time series data.</param>
        /// <param name="end">The end date and time of the time series data.</param>
        /// <param name="interval">The interval.</param>
        /// <param name="trendIds">The list of TrendIds for which to retrieve the data. If null, retrieves data for all points.</param>
        /// <param name="valueMultiplier">The value multiplier.</param>
        /// <returns>A read-only collection of TimeSeriesDataPoint objects containing the cumulative sum of the time series data for the specified TrendIds within the specified time range.</returns>
        Task<IReadOnlyCollection<TimeSeriesDataPoint>> GetCumulativeSumAsync(
            Guid? clientId,
            DateTime start,
            DateTime end,
            TimeSpan interval,
            List<Guid> trendIds,
            double valueMultiplier);

        /// <summary>
        /// Retrieves telemetry data within a specified time range.
        /// </summary>
        /// <param name="clientId">The client ID.</param>
        /// <param name="telemetryPayload">The telemetry payload containing the required parameters.</param>
        /// <param name="start">The start date and time of the telemetry data.</param>
        /// <param name="end">The end date and time of the telemetry data.</param>
        /// <param name="pageSize">The number of telemetry data to retrieve per page.</param>
        /// <param name="continuationToken">The token to continue retrieving telemetry data from a previous request.</param>
        /// <returns>An <see cref="GetTelemetryResult"/> object containing the retrieved telemetry data.</returns>
        Task<GetTelemetryResult> GetTelemetryAsync(
            Guid? clientId,
            TelemetryRequestBody telemetryPayload,
            DateTime start,
            DateTime end,
            int pageSize,
            string continuationToken);

        /// <summary>
        /// Retrieves time series data for a specific external ID.
        /// </summary>
        /// <param name="connectorId">The connector ID.</param>
        /// <param name="externalId">The external ID.</param>
        /// <param name="clientId">The client ID.</param>
        /// <param name="start">The start date and time of the time series data.</param>
        /// <param name="end">The end date and time of the time series data.</param>
        /// <param name="selectedInterval">The selected time interval for the time series data.</param>
        /// <returns>A collection of time series data for the specified external ID.</returns>
        Task<IEnumerable<TimeSeriesAnalogData>> GetTimeSeriesDataByExternalIdAsync(
        Guid connectorId,
        string externalId,
        Guid? clientId,
        DateTime start,
        DateTime end,
        TimeSpan? selectedInterval);
    }
}
