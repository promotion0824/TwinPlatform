namespace Willow.LiveData.Core.Features.GetLiveData.GetPointLiveData.Adx;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Willow.Infrastructure.Exceptions;
using Willow.LiveData.Core.Common;
using Willow.LiveData.Core.Domain;
using Willow.LiveData.Core.Features.Connectors.DTOs;
using Willow.LiveData.Core.Features.GetLiveData.GetPointLiveData.Extensions;
using Willow.LiveData.Core.Features.GetLiveData.GetPointLiveData.Models;
using Willow.LiveData.Core.Infrastructure.Extensions;

/// <inheritdoc/>
internal class AdxLiveDataService : IAdxLiveDataService
{
    private readonly IAdxLiveDataRepository adxLiveDataRepository;
    private readonly IAdxContinuationTokenProvider<string, int> continuationTokenProvider;
    private readonly IDateTimeIntervalService dateTimeIntervalService;

    public AdxLiveDataService(
        IDateTimeIntervalService dateTimeIntervalService,
        IAdxLiveDataRepository adxLiveDataRepository,
        IAdxContinuationTokenProvider<string, int> continuationTokenProvider)
    {
        this.adxLiveDataRepository = adxLiveDataRepository;
        this.continuationTokenProvider = continuationTokenProvider;
        this.dateTimeIntervalService = dateTimeIntervalService;
    }

    public async Task<IEnumerable<TimeSeriesData>> GetTimeSeriesDataByTrendIdAsync(
        Guid? clientId,
        DateTime start,
        DateTime end,
        string type,
        Guid pointEntityId,
        TimeSpan? selectedInterval)
    {
        var interval = dateTimeIntervalService.GetDateTimeInterval(start, end, selectedInterval);

        var items = type switch
        {
            Constants.Binary => await adxLiveDataRepository.GetTimeSeriesBinaryDataByPointEntityIdAsync(
                clientId,
                start,
                end,
                interval.Name,
                pointEntityId),
            Constants.Analog => await adxLiveDataRepository.GetTimeSeriesAnalogDataByPointEntityIdAsync(clientId,
                start,
                end,
                interval.Name,
                pointEntityId),
            Constants.MultiState => await adxLiveDataRepository.GetTimeSeriesMultiStateDataByPointEntityIdAsync(clientId,
                start,
                end,
                interval.Name,
                pointEntityId),
            Constants.Sum => await adxLiveDataRepository.GetTimeSeriesSumDataByPointEntityIdAsync(
                clientId,
                start,
                end,
                interval.Name,
                pointEntityId),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, $"Time Series Data Type {type} is not available."),
        };

        return items;
    }

    public Task<Dictionary<Guid, List<TimeSeriesData>>> GetTimeSeriesDataByTrendIdAsync(
        Guid? clientId,
        DateTime start,
        DateTime end,
        string type,
        IEnumerable<Guid> pointEntityIds,
        TimeSpan? selectedInterval)
    {
        var interval = dateTimeIntervalService.GetDateTimeInterval(start, end, selectedInterval);

        var getTimeSeriesData = type switch
        {
            Constants.Binary =>
                adxLiveDataRepository.GetTimeSeriesBinaryDataByPointEntityIdAsync(
                    clientId,
                    start,
                    end,
                    interval.Name,
                    pointEntityIds),
            Constants.Analog =>
                adxLiveDataRepository.GetTimeSeriesAnalogDataByPointEntityIdAsync(
                    clientId,
                    start,
                    end,
                    interval.Name,
                    pointEntityIds),
            Constants.MultiState =>
                adxLiveDataRepository.GetTimeSeriesMultiStateDataByPointEntityIdAsync(
                    clientId,
                    start,
                    end,
                    interval.Name,
                    pointEntityIds),
            Constants.Sum =>
                adxLiveDataRepository.GetTimeSeriesSumDataByPointEntityIdAsync(
                    clientId,
                    start,
                    end,
                    interval.Name,
                    pointEntityIds),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, $"Time Series Data Type {type} is not available."),
        };

        return GetTimeSeriesDataByPointEntityIdInternalAsync();

        async Task<Dictionary<Guid, List<TimeSeriesData>>> GetTimeSeriesDataByPointEntityIdInternalAsync()
        {
            var items = await getTimeSeriesData;

            return items.GroupBy(x => x.Id, x => x.TimeSeriesData)
                .ToDictionary(x => x.Key, x => x.ToList());
        }
    }

    public Task<Dictionary<Guid, List<TimeSeriesData>>> GetTimeSeriesDataByTrendIdAsync(
        Guid? clientId,
        DateTime start,
        DateTime end,
        Guid[] pointEntityIds,
        string[] types,
        TimeSpan? selectedInterval)
    {
        var interval = dateTimeIntervalService.GetDateTimeInterval(start, end, selectedInterval);
        var analogPointEntityIds = new List<Guid>(pointEntityIds.Length);
        var multiStatePointEntityIds = new List<Guid>(pointEntityIds.Length);
        var binaryPointEntityIds = new List<Guid>(pointEntityIds.Length);
        var sumPointEntityIds = new List<Guid>(pointEntityIds.Length);
        for (var i = 0; i < pointEntityIds.Length; i++)
        {
            if (types == null || types[i] == null)
            {
                throw new BadRequestException("Point type cannot be empty");
            }

            var type = types[i].ToLower();
            switch (type)
            {
                case Constants.Analog:
                    analogPointEntityIds.Add(pointEntityIds[i]);
                    break;
                case Constants.Binary:
                    binaryPointEntityIds.Add(pointEntityIds[i]);
                    break;
                case Constants.MultiState:
                    multiStatePointEntityIds.Add(pointEntityIds[i]);
                    break;
                case Constants.Sum:
                    sumPointEntityIds.Add(pointEntityIds[i]);
                    break;
                default:
                    throw new BadRequestException($"Point Type {type} is not available.");
            }
        }

        return GetTimeSeriesDataByPointEntityIdInternalAsync();

        async Task<Dictionary<Guid, List<TimeSeriesData>>> GetTimeSeriesDataByPointEntityIdInternalAsync()
        {
            var items = new List<(Guid Id, TimeSeriesData Data)>();
            var result = new Dictionary<Guid, List<TimeSeriesData>>();
            if (analogPointEntityIds.Any())
            {
                items.AddRange(await adxLiveDataRepository.GetTimeSeriesAnalogDataByPointEntityIdAsync(
                    clientId,
                    start,
                    end,
                    interval.Name,
                    analogPointEntityIds));
            }

            if (binaryPointEntityIds.Any())
            {
                items.AddRange(await adxLiveDataRepository.GetTimeSeriesBinaryDataByPointEntityIdAsync(
                    clientId,
                    start,
                    end,
                    interval.Name,
                    binaryPointEntityIds));
            }

            if (multiStatePointEntityIds.Any())
            {
                items.AddRange(await adxLiveDataRepository.GetTimeSeriesMultiStateDataByPointEntityIdAsync(
                    clientId,
                    start,
                    end,
                    interval.Name,
                    multiStatePointEntityIds));
            }

            if (sumPointEntityIds.Any())
            {
                items.AddRange(await adxLiveDataRepository.GetTimeSeriesSumDataByPointEntityIdAsync(
                    clientId,
                    start,
                    end,
                    interval.Name,
                    sumPointEntityIds));
            }

            foreach (var groupedItem in items.GroupBy(x => x.Id, x => x.Data))
            {
                result[groupedItem.Key] = groupedItem.ToList();
            }

            return result;
        }
    }

    public async Task<GetTrendlogResult> GetTimeSeriesRawDataByTrendIdAsync(
        Guid? clientId,
        Guid pointEntityId,
        DateTime start,
        DateTime end,
        string continuationToken,
        int? pageSize)
    {
        var items = await adxLiveDataRepository.GetTimeSeriesRawDataByPointEntityIdAsync(
            clientId,
            pointEntityId,
            start,
            end);

        // Pagination is not implemented to be feature parity with existing service for PostgresDB
        // Server side pagination is preferred over client-side to avoid memory issues with large amount of data
        // This should be implemented as part of next version of API to better utilise ADX
        return new GetTrendlogResult
        {
            ContinuationToken = null,
            Data = items.ToList(),
        };
    }

    public async Task<IEnumerable<GetTrendlogsResultItem>> GetTimeSeriesRawDataBySiteIdAsync(Guid? clientId,
        Guid siteId,
        DateTime start,
        DateTime end,
        List<Guid> pointIds = null)
    {
        var items = await adxLiveDataRepository.GetTimeSeriesRawDataBySiteIdAsync(
            clientId,
            siteId,
            start,
            end,
            pointIds);

        var grouped = items.GroupBy(q => q.PointEntityId);

        var result = new List<GetTrendlogsResultItem>();
        foreach (var group in grouped)
        {
            var resultItem = new GetTrendlogsResultItem
            {
                PointEntityId = group.Key,
                Data = group.Select(q => new TimeSeriesRawData
                {
                    Value = q.Value,
                    Timestamp = q.Timestamp,
                }).ToList(),
            };

            result.Add(resultItem);
        }

        return result;
    }

    public async Task<IEnumerable<PointTimeSeriesRawData>> GetLastTimeSeriesRawDataBySiteIdAsync(
        Guid? clientId,
        Guid siteId,
        List<Guid> pointIds = null)
    {
        return await adxLiveDataRepository.GetLastTimeSeriesRawDataBySiteIdAsync(clientId, siteId, pointIds);
    }

    public Task<List<PointStatsData>> GetPointStatsAsync(Guid? clientId, DateTime start)
    {
        return adxLiveDataRepository.GetPointStatsAsync(clientId, start);
    }

    public Task<int> GetPointsStatsCountByIdsListAsync(
        Guid? clientId,
        DateTime start,
        IEnumerable<Guid> pointEntityIds)
    {
        return adxLiveDataRepository.GetPointsStatsCountByIdsListAsync(clientId, start, pointEntityIds);
    }

    public Task<IReadOnlyList<PointTimeSeriesRawData>> GetHistoricalLastTimeSeriesRawDataAsync(
        Guid? clientId,
        DateTime start,
        DateTime end,
        List<Guid> pointIds = null)
    {
        return adxLiveDataRepository.GetHistoricalLastTimeSeriesRawDataAsync(clientId, start, end, pointIds);
    }

    public async Task<IReadOnlyDictionary<DateTime, IReadOnlyDictionary<Guid, decimal?>>> GetAggregatedValuesInsideTimeIntervalsAsync(
        Guid? clientId,
        DateTime start,
        DateTime end,
        TimeSpan interval,
        List<Guid> pointEntityIds,
        AggregationType aggregationType)
    {
        var intervalStr = interval.ToString("c");

        IReadOnlyList<TimeSeriesDataPoint> dataPoints = aggregationType switch
        {
            AggregationType.First =>
                await adxLiveDataRepository.GetFirstValuesInsideTimeIntervalsAsync(
                    clientId, start, end, intervalStr, pointEntityIds),
            var x when
                x == AggregationType.Avg ||
                x == AggregationType.Sum =>
                await adxLiveDataRepository.GetAggregatedValuesInsideTimeIntervalsAsync(
                    clientId, start, end, intervalStr, pointEntityIds, aggregationType),
            _ => throw new NotSupportedException("Unsupported aggregation type"),
        };

        // TODO Create dictionary in Kusto query and parse the result as a dictionary directly instead
        // Reference: https://docs.microsoft.com/en-us/azure/data-explorer/kusto/query/make-bag-aggfunction
        return dataPoints
                .GroupBy(x => x.Timestamp)
                .ToDictionary(
                    x => x.Key,
                    x => x.GroupBy(y => y.PointEntityId)
                          .ToDictionary(
                              y => y.Key,
                              y => y.Average(z => z.Value)) as IReadOnlyDictionary<Guid, decimal?>);
    }

    public async Task<IReadOnlyCollection<CumulativeTimeSeriesDataPoint>> GetCumulativeTrendAsync(
        Guid? clientId,
        DateTime start,
        DateTime end,
        TimeSpan interval,
        List<Guid> pointIds,
        double valueMultiplier)
    {
        var intervalStr = interval.ToString("c");

        return await adxLiveDataRepository.GetCumulativeTrendAsync(
            clientId: clientId,
            start: start,
            end: end,
            interval: intervalStr,
            pointIds: pointIds,
            valueMultiplier: valueMultiplier);
    }

    public async Task<IReadOnlyCollection<TimeSeriesDataPoint>> GetCumulativeSumAsync(
        Guid? clientId,
        DateTime start,
        DateTime end,
        TimeSpan interval,
        List<Guid> pointIds,
        double valueMultiplier)
    {
        var intervalStr = interval.ToString("c");

        return await adxLiveDataRepository.GetCumulativeSumAsync(
            clientId: clientId,
            start: start,
            end: end,
            interval: intervalStr,
            pointIds: pointIds,
            valueMultiplier: valueMultiplier);
    }

    public Task<GetTelemetryResult> GetTelemetryAsync(
        Guid? clientId,
        TelemetryRequestBody telemetryPayload,
        DateTime start,
        DateTime end,
        int pageSize,
        string continuationToken)
    {
        if (telemetryPayload == null)
        {
            throw new ArgumentNullException(nameof(telemetryPayload));
        }

        return GetTelemetryInternalAsync(clientId, telemetryPayload, start, end, pageSize, continuationToken);
    }

    public async Task<IEnumerable<TimeSeriesAnalogData>> GetTimeSeriesDataByExternalIdAsync(
        Guid connectorId,
        string externalId,
        Guid? clientId,
        DateTime startUtc,
        DateTime endUtc,
        TimeSpan? selectedInterval)
    {
        var interval = dateTimeIntervalService.GetDateTimeInterval(startUtc, endUtc, selectedInterval);

        var items = await adxLiveDataRepository.GetTimeSeriesDataByExternalIdAsync(connectorId, externalId, clientId, startUtc, endUtc, interval.Name);

        return items;
    }

    private async Task<GetTelemetryResult> GetTelemetryInternalAsync(
        Guid? clientId,
        TelemetryRequestBody telemetryPayload,
        DateTime start,
        DateTime end,
        int pageSize,
        string continuationToken)
    {
        (string storedQueryNameResult, int rowNumber) = continuationTokenProvider.ParseToken(continuationToken);
        var validTrendIds = telemetryPayload.TrendIds.GetValidGuids();

        var pagedTelemetry = await adxLiveDataRepository.GetTelemetryAsync(new GetTelemetryRequest(
            clientId,
            telemetryPayload.ConnectorId,
            start,
            end,
            pageSize,
            storedQueryNameResult,
            telemetryPayload.DtIds,
            validTrendIds,
            rowNumber));

        int? lastRowNumber = pagedTelemetry.Telemetry.MaxBy(x => x.RowNumber)?.RowNumber;
        continuationToken = continuationTokenProvider.GetToken(pagedTelemetry.ContinuationToken, lastRowNumber ?? 0);

        var invalidGuids = telemetryPayload.TrendIds.GetInvalidGuids();
        ErrorData errList = null;
        if (invalidGuids != null && invalidGuids.Count > 0)
        {
            invalidGuids.RemoveAll(x => x.Length == 0);
            errList = new ErrorData
            {
                Ids = invalidGuids,
                Message = telemetryPayload.TrendIds.GetErrorMessageOnInvalidGuids("Trend Ids"),
            };
        }

        return new GetTelemetryResult
        {
            ContinuationToken = pagedTelemetry.TotalRowsCount > lastRowNumber ? continuationToken : string.Empty,
            Data = pagedTelemetry.Telemetry?.ToList().MapTo(),
            ErrorList = errList,
        };
    }
}
