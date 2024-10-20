namespace Willow.LiveData.Core.Features.GetLiveData.GetPointLiveData.Adx;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Willow.LiveData.Core.Domain;
using Willow.LiveData.Core.Features.GetLiveData.GetPointLiveData.Model;
using Willow.LiveData.Core.Features.GetLiveData.GetPointLiveData.Models;

internal interface IAdxLiveDataRepository
{
    Task<IReadOnlyCollection<TimeSeriesData>> GetTimeSeriesAnalogDataByPointEntityIdAsync(
        Guid? clientId,
        DateTime start,
        DateTime end,
        string interval,
        Guid pointEntityId);

    Task<IReadOnlyCollection<(Guid Id, TimeSeriesData TimeSeriesData)>> GetTimeSeriesAnalogDataByPointEntityIdAsync(
        Guid? clientId,
        DateTime start,
        DateTime end,
        string interval,
        IEnumerable<Guid> pointEntityIds);

    Task<IReadOnlyCollection<TimeSeriesData>> GetTimeSeriesBinaryDataByPointEntityIdAsync(
        Guid? clientId,
        DateTime start,
        DateTime end,
        string interval,
        Guid pointEntityId);

    Task<IReadOnlyCollection<(Guid Id, TimeSeriesData TimeSeriesData)>> GetTimeSeriesBinaryDataByPointEntityIdAsync(
        Guid? clientId,
        DateTime start,
        DateTime end,
        string interval,
        IEnumerable<Guid> pointEntityIds);

    Task<IReadOnlyCollection<TimeSeriesData>> GetTimeSeriesMultiStateDataByPointEntityIdAsync(
        Guid? clientId,
        DateTime start,
        DateTime end,
        string interval,
        Guid pointEntityId);

    Task<IReadOnlyCollection<(Guid Id, TimeSeriesData TimeSeriesData)>> GetTimeSeriesMultiStateDataByPointEntityIdAsync(
        Guid? clientId,
        DateTime start,
        DateTime end,
        string interval,
        IEnumerable<Guid> pointEntityIds);

    Task<IReadOnlyCollection<TimeSeriesData>> GetTimeSeriesSumDataByPointEntityIdAsync(
        Guid? clientId,
        DateTime start,
        DateTime end,
        string interval,
        Guid pointEntityId);

    Task<IReadOnlyCollection<(Guid Id, TimeSeriesData TimeSeriesData)>> GetTimeSeriesSumDataByPointEntityIdAsync(
        Guid? clientId,
        DateTime start,
        DateTime end,
        string interval,
        IEnumerable<Guid> pointEntityIds);

    Task<IReadOnlyCollection<TimeSeriesRawData>> GetTimeSeriesRawDataByPointEntityIdAsync(
       Guid? clientId,
       Guid pointEntityId,
       DateTime start,
       DateTime end);

    Task<IReadOnlyCollection<PointTimeSeriesRawData>> GetTimeSeriesRawDataBySiteIdAsync(
       Guid? clientId,
       Guid siteId,
       DateTime start,
       DateTime end,
       List<Guid> pointIds = null);

    Task<IReadOnlyCollection<PointTimeSeriesRawData>> GetLastTimeSeriesRawDataBySiteIdAsync(
       Guid? clientId,
       Guid siteId,
       List<Guid> pointIds = null);

    Task<List<PointStatsData>> GetPointStatsAsync(Guid? clientId, DateTime start);

    Task<int> GetPointsStatsCountByIdsListAsync(Guid? clientId, DateTime start, IEnumerable<Guid> pointIds);

    Task<IReadOnlyList<PointTimeSeriesRawData>> GetHistoricalLastTimeSeriesRawDataAsync(
        Guid? clientId,
        DateTime start,
        DateTime end,
        List<Guid> pointIds = null);

    Task<IReadOnlyList<TimeSeriesDataPoint>> GetFirstValuesInsideTimeIntervalsAsync(
        Guid? clientId,
        DateTime start,
        DateTime end,
        string interval,
        List<Guid> pointIds = null);

    Task<IReadOnlyList<CumulativeTimeSeriesDataPoint>> GetCumulativeTrendAsync(
        Guid? clientId, DateTime start, DateTime end, string interval, List<Guid> pointIds, double valueMultiplier);

    Task<IReadOnlyList<TimeSeriesDataPoint>> GetCumulativeSumAsync(
        Guid? clientId, DateTime start, DateTime end, string interval, List<Guid> pointIds, double valueMultiplier);

    Task<IReadOnlyList<TimeSeriesDataPoint>> GetAggregatedValuesInsideTimeIntervalsAsync(
        Guid? clientId,
        DateTime start,
        DateTime end,
        string interval,
        List<Guid> pointIds,
        AggregationType aggregationType);

    Task<PagedTelemetry> GetTelemetryAsync(GetTelemetryRequest request);

    Task<IReadOnlyCollection<TimeSeriesAnalogData>> GetTimeSeriesDataByExternalIdAsync(
       Guid connectorId,
       string externalId,
       Guid? clientId,
       DateTime start,
       DateTime end,
       string interval);
}
