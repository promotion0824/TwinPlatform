namespace Willow.LiveData.Core.Features.GetLiveData.GetPointLiveData.Adx;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Kusto.Data.Exceptions;
using Willow.LiveData.Core.Common;
using Willow.LiveData.Core.Domain;
using Willow.LiveData.Core.Features.GetLiveData.GetPointLiveData.Model;
using Willow.LiveData.Core.Features.GetLiveData.GetPointLiveData.Models;
using Willow.LiveData.Core.Infrastructure.Database.Adx;

internal class AdxLiveDataRepository : AdxBaseRepository, IAdxLiveDataRepository
{
    private readonly IAdxQueryRunner adxQueryRunner;

    private const string FilterDuplicates = @"| summarize arg_max(EnqueuedTimestamp, *) by TrendId, SourceTimestamp";

    private const string EmptyTrendId = @"where (isempty(TrendId) or TrendId == ""00000000-0000-0000-0000-000000000000"") and isnotempty(ExternalId)";

    private const string SkipIllegalValues = @" ScalarValue !in (""NaN"", ""N/A"", ""n/a"", ""NA"", ""na"", ""NAN"", ""nan"", ""Infinity"")";

    /// <summary>
    /// Initializes a new instance of the <see cref="AdxLiveDataRepository"/> class.
    ///  This service executes KQL queries against ADX table.
    /// </summary>
    /// <param name="adxQueryRunner">Service that executes the Adx query.</param>
    /// <param name="storedQueryResultTokenProvider">Service for stored query token used for paging.</param>
    public AdxLiveDataRepository(
        IAdxQueryRunner adxQueryRunner,
        IContinuationTokenProvider<string, string> storedQueryResultTokenProvider)
        : base(adxQueryRunner, storedQueryResultTokenProvider)
    {
        this.adxQueryRunner = adxQueryRunner;
    }

    private static string GetQueryPrefix(DateTime start, DateTime end, IEnumerable<Guid> pointEntityIds = null, bool returnTableRef = false)
    {
        var trendIds = (pointEntityIds ?? Array.Empty<Guid>()).ToList();
        var trendIdClause = GetTrendIdsClause(trendIds);
        var trendIdString = "where isnotempty(TrendId)";
        var referenceTable = "ActiveTwins";
        var queryPrefix = string.Empty;

        if (trendIds.Any())
        {
            trendIdString = $@"{trendIdString} {trendIdClause}";
            referenceTable = "ReferenceTwins";
            queryPrefix =
                $@"let ExternalIds = ActiveTwins
                {trendIdClause}
                and isnotempty(ExternalId)
                | summarize ExternalIdTwins = make_list(ExternalId);
                let TrendIds = ActiveTwins
                {trendIdClause}
                | summarize TrendIdTwins = make_list(TrendId);
                let ReferenceTwins = ActiveTwins
                | where TrendId in (TrendIds)
                | project Id, ConnectorId = tostring(ConnectorId), TrendId = tostring(TrendId), ExternalId;"
                ;
        }

        var telemetryTrendId = GetTelemetryJoinQuery(referenceTable, trendIdString, start, end, "TrendId");
        var telemetryExternalId = GetTelemetryJoinQuery(referenceTable, EmptyTrendId, start, end, "ExternalId");

        var unionQuery = $@"(union {telemetryExternalId}, {telemetryTrendId})";

        var finalQuery = returnTableRef ? $@"{queryPrefix} let TelemetryTable = {unionQuery};"
                             : $@"{queryPrefix} {unionQuery}";

        return finalQuery;
    }

    private static string GetTelemetryJoinQuery(string referenceTable, string filterString, DateTime start, DateTime end, string joinField)
    {
        var query =
            $@"({referenceTable}
            | project ConnectorId = tostring(ConnectorId), TrendId = tostring(TrendId), ExternalId, Id
            | join kind=inner (Telemetry | {filterString} and SourceTimestamp between (datetime({start:s}) .. datetime({end:s})) and {SkipIllegalValues})
            on $left.{joinField} == $right.{joinField}
            | project ConnectorId, DtId = Id, ExternalId, TrendId, SourceTimestamp, EnqueuedTimestamp, ScalarValue, Latitude, Longitude, Altitude, Properties)"
            ;

        return query;
    }

    public async Task<IReadOnlyCollection<TimeSeriesData>> GetTimeSeriesAnalogDataByPointEntityIdAsync(
        Guid? clientId,
        DateTime start,
        DateTime end,
        string interval,
        Guid pointEntityId)
    {
        IEnumerable<Guid> trendIds = new[] { pointEntityId };
        var queryPrefix = GetQueryPrefix(start, end, trendIds);

        var kqlQuery = $@"{queryPrefix}
                            {FilterDuplicates}
                            | summarize Average = avg(todouble(ScalarValue)),
                                Minimum = min(todouble(ScalarValue)),
                                Maximum = max(todouble(ScalarValue))
                                by bin(TimeStamp=SourceTimestamp, time({interval})), PointEntityId = toguid(TrendId)
                            | order by TimeStamp asc";

        using var reader = await adxQueryRunner.QueryAsync(clientId, kqlQuery);
        var result = reader.Parse<TimeSeriesAnalogData>();

        return result.Select(x => (TimeSeriesData)x).ToList();
    }

    public async Task<IReadOnlyCollection<TimeSeriesData>> GetTimeSeriesMultiStateDataByPointEntityIdAsync(
        Guid? clientId,
        DateTime start,
        DateTime end,
        string interval,
        Guid pointEntityId)
    {
        IEnumerable<Guid> trendIds = new[] { pointEntityId };
        var queryPrefix = GetQueryPrefix(start, end, trendIds);

        var kqlQuery = $@"{queryPrefix}
                            {FilterDuplicates}
                            | summarize StateCountValue = count() by toint(ScalarValue), bin(TimeStamp=SourceTimestamp, time({interval})), PointEntityId = toguid(TrendId)
                            | summarize State = make_bag(bag_pack(tostring(ScalarValue), StateCountValue)) by TimeStamp, PointEntityId
                            | order by TimeStamp asc";

        using var reader = await adxQueryRunner.QueryAsync(clientId, kqlQuery);
        var result = reader.Parse<TimeSeriesDataWithIds>();

        return result.Select(x => (TimeSeriesData)new TimeSeriesMultiStateData
        {
            Timestamp = x.Timestamp,
            State = x.State.ToObject<Dictionary<string, int>>(),
        }).ToList();
    }

    public async Task<IReadOnlyCollection<(Guid, TimeSeriesData)>> GetTimeSeriesMultiStateDataByPointEntityIdAsync(
        Guid? clientId,
        DateTime start,
        DateTime end,
        string interval,
        IEnumerable<Guid> pointEntityIds)
    {
        var queryPrefix = GetQueryPrefix(start, end, pointEntityIds);
        var kqlQuery = $@"{queryPrefix}
                            {FilterDuplicates}
                            | summarize StateCountValue = count() by toint(ScalarValue), bin(TimeStamp=SourceTimestamp, time({interval})), PointEntityId = toguid(TrendId)
                            | summarize State = make_bag(bag_pack(tostring(ScalarValue), StateCountValue)) by TimeStamp, PointEntityId
                            | order by TimeStamp asc";

        using var reader = await adxQueryRunner.QueryAsync(clientId, kqlQuery);
        var result = reader.Parse<TimeSeriesDataWithIds>();

        return result.Select(x => (x.PointEntityId,
                (TimeSeriesData)new TimeSeriesMultiStateData
                {
                    Timestamp = x.Timestamp,
                    State = x.State.ToObject<Dictionary<string, int>>(),
                }))
            .ToList();
    }

    public async Task<IReadOnlyCollection<(Guid, TimeSeriesData)>> GetTimeSeriesAnalogDataByPointEntityIdAsync(
        Guid? clientId,
        DateTime start,
        DateTime end,
        string interval,
        IEnumerable<Guid> pointEntityIds)
    {
        var queryPrefix = GetQueryPrefix(start, end, pointEntityIds);
        var kqlQuery = $@"{queryPrefix}
                            {FilterDuplicates}
                            | summarize Average = avg(todouble(ScalarValue)),
                                Minimum = min(todouble(ScalarValue)),
                                Maximum = max(todouble(ScalarValue))
                                by bin(TimeStamp=SourceTimestamp, time({interval})), PointEntityId = toguid(TrendId)
                            | order by TimeStamp asc";

        using var reader = await adxQueryRunner.QueryAsync(clientId, kqlQuery);
        var result = reader.Parse<TimeSeriesDataWithIds>();

        return result.Select(x => (x.PointEntityId,
                                 (TimeSeriesData)new TimeSeriesAnalogData
                                 {
                                     Timestamp = x.Timestamp,
                                     Average = x.Average,
                                     Maximum = x.Maximum,
                                     Minimum = x.Minimum,
                                 }))
                     .ToList();
    }

    public async Task<IReadOnlyCollection<TimeSeriesData>> GetTimeSeriesBinaryDataByPointEntityIdAsync(
        Guid? clientId,
        DateTime start,
        DateTime end,
        string interval,
        Guid pointEntityId)
    {
        IEnumerable<Guid> trendIds = new[] { pointEntityId };
        var queryPrefix = GetQueryPrefix(start, end, trendIds);

        var kqlQuery = $@"{queryPrefix}
                            {FilterDuplicates}
                            | summarize OnCount = countif(tobool(ScalarValue) == true),
                                OffCount = countif(tobool(ScalarValue) == false)
                                by bin(TimeStamp=SourceTimestamp, time({interval})), PointEntityId = toguid(TrendId)
                            | order by TimeStamp asc";

        using var reader = await adxQueryRunner.QueryAsync(clientId, kqlQuery);
        var result = reader.Parse<TimeSeriesBinaryData>();

        return result.Select(x => (TimeSeriesData)x).ToList();
    }

    public async Task<IReadOnlyCollection<(Guid, TimeSeriesData)>> GetTimeSeriesBinaryDataByPointEntityIdAsync(
        Guid? clientId,
        DateTime start,
        DateTime end,
        string interval,
        IEnumerable<Guid> pointEntityIds)
    {
        var queryPrefix = GetQueryPrefix(start, end, pointEntityIds);
        var kqlQuery = $@"{queryPrefix}
                            {FilterDuplicates}
                            | summarize OnCount = countif(tobool(ScalarValue) == true),
                                OffCount = countif(tobool(ScalarValue) == false)
                                by bin(TimeStamp=SourceTimestamp, time({interval})), PointEntityId = toguid(TrendId)
                            | order by TimeStamp asc";

        using var reader = await adxQueryRunner.QueryAsync(clientId, kqlQuery);
        var result = reader.Parse<TimeSeriesDataWithIds>();

        return result.Select(x => (x.PointEntityId,
                                 (TimeSeriesData)new TimeSeriesBinaryData
                                 {
                                     Timestamp = x.Timestamp,
                                     OffCount = x.OffCount,
                                     OnCount = x.OnCount,
                                 }))
                     .ToList();
    }

    public async Task<IReadOnlyCollection<TimeSeriesData>> GetTimeSeriesSumDataByPointEntityIdAsync(
        Guid? clientId,
        DateTime start,
        DateTime end,
        string interval,
        Guid pointEntityId)
    {
        IEnumerable<Guid> trendIds = new[] { pointEntityId };
        var queryPrefix = GetQueryPrefix(start, end, trendIds);

        var kqlQuery = $@"{queryPrefix}
                            {FilterDuplicates}
                            | summarize Average = sum(todouble(ScalarValue))
                                by bin(TimeStamp=SourceTimestamp, time({interval})), PointEntityId = toguid(TrendId)
                            | order by TimeStamp asc";

        using var reader = await adxQueryRunner.QueryAsync(clientId, kqlQuery);
        var result = reader.Parse<TimeSeriesSumData>();

        return result.Select(x => (TimeSeriesData)x).ToList();
    }

    public async Task<IReadOnlyCollection<(Guid, TimeSeriesData)>> GetTimeSeriesSumDataByPointEntityIdAsync(
        Guid? clientId,
        DateTime start,
        DateTime end,
        string interval,
        IEnumerable<Guid> pointEntityIds)
    {
        var queryPrefix = GetQueryPrefix(start, end, pointEntityIds);
        var kqlQuery = $@"{queryPrefix}
                            {FilterDuplicates}
                            | summarize Average = sum(todouble(ScalarValue))
                                by bin(TimeStamp=SourceTimestamp, time({interval})), PointEntityId = toguid(TrendId)
                            | order by TimeStamp asc";

        using var reader = await adxQueryRunner.QueryAsync(clientId, kqlQuery);
        var result = reader.Parse<TimeSeriesDataWithIds>();

        return result.Select(x => (x.PointEntityId,
                                 (TimeSeriesData)new TimeSeriesSumData
                                 {
                                     Timestamp = x.Timestamp,
                                     Average = x.Average,
                                 }))
                     .ToList();
    }

    public async Task<IReadOnlyCollection<TimeSeriesRawData>> GetTimeSeriesRawDataByPointEntityIdAsync(
        Guid? clientId,
        Guid pointEntityId,
        DateTime start,
        DateTime end)
    {
        IEnumerable<Guid> trendIds = new[] { pointEntityId };
        var queryPrefix = GetQueryPrefix(start, end, trendIds);

        var kqlQuery = $@"{queryPrefix}
                            {FilterDuplicates}
                            | project Timestamp = SourceTimestamp, Value = ScalarValue
                            | order by Timestamp asc ";

        using var reader = await adxQueryRunner.QueryAsync(clientId, kqlQuery);
        var result = reader.Parse<TimeSeriesRawData>();

        return result.ToList();
    }

    public async Task<IReadOnlyCollection<PointTimeSeriesRawData>> GetTimeSeriesRawDataBySiteIdAsync(
        Guid? clientId,
        Guid siteId,
        DateTime start,
        DateTime end,
        List<Guid> pointIds = null)
    {
        var filterByPoints = string.Empty;

        if (pointIds != null)
        {
            filterByPoints = $@" and (TrendId in ({string.Join(", ", pointIds.Select(q => $"'{q}'"))}))";
        }

        var siteIdQuery = $@" (SiteId == ""{siteId}"") {filterByPoints} ";

        var queryPrefix = GetQueryPrefix(start, end, pointIds);
        var kqlQuery = $@"{queryPrefix}
                            | project PointEntityId = toguid(TrendId), SourceTimestamp, EnqueuedTimestamp, Value = ScalarValue
                            | join kind=inner ActiveTwins on $left.PointEntityId == $right.TrendId
                            | where ({siteIdQuery})
                            {FilterDuplicates}
                            | project Timestamp = SourceTimestamp, Value, PointEntityId
                            | order by PointEntityId, Timestamp asc";

        using var reader = await adxQueryRunner.QueryAsync(clientId, kqlQuery);
        var result = reader.Parse<PointTimeSeriesRawData>();

        return result.ToList();
    }

    public async Task<IReadOnlyCollection<PointTimeSeriesRawData>> GetLastTimeSeriesRawDataBySiteIdAsync(
        Guid? clientId,
        Guid siteId,
        List<Guid> pointIds = null)
    {
        var filterByPoints = string.Empty;
        if (pointIds != null)
        {
            filterByPoints = $@" and (TrendId in ({string.Join(", ", pointIds.Select(q => $"'{q}'"))}))";
        }

        var siteIdQuery = $@" (SiteId == ""{siteId}"") {filterByPoints} ";

        var queryPrefix = GetQueryPrefix(DateTime.UtcNow - TimeSpan.FromHours(1), DateTime.UtcNow, pointIds, true);

        // For feature parity with Postgres version, assumes that ScalarValue contains double data type only
        // Needs to be implemented differently to allow truly dynamic values
        var kqlQuery = $@"{queryPrefix}
                        ActiveTwins
                            | join kind = inner (
                                TelemetryTable
                                | summarize MaxSourceTimestamp = max(SourceTimestamp) by TrendId
                                | join kind=inner (TelemetryTable) on $left.MaxSourceTimestamp == $right.SourceTimestamp
                                    and $left.TrendId == $right.TrendId
                                | distinct TrendId, MaxSourceTimestamp, todouble(ScalarValue)
                                | project PointEntityId = toguid(TrendId), MaxSourceTimestamp, ScalarValue)
                                on $left.TrendId == $right.PointEntityId
                            | where MaxSourceTimestamp > now(-1h) and ({siteIdQuery})
                            | project Timestamp = MaxSourceTimestamp, Value = ScalarValue, PointEntityId
                            | order by PointEntityId, Timestamp asc";

        using var reader = await adxQueryRunner.QueryAsync(clientId, kqlQuery);
        var result = reader.Parse<PointTimeSeriesRawData>();

        return result.ToList();
    }

    public async Task<IReadOnlyList<PointTimeSeriesRawData>> GetHistoricalLastTimeSeriesRawDataAsync(
        Guid? clientId,
        DateTime start,
        DateTime end,
        List<Guid> pointIds = null)
    {
        var filterByPoints = string.Empty;
        if (pointIds != null)
        {
            filterByPoints = $@" and (TrendId in ({string.Join(", ", pointIds.Select(q => $"'{q}'"))}))";
        }

        // For feature parity with Postgres version, assumes that ScalarValue contains double data type only
        // Needs to be implemented differently to allow truly dynamic values
        var kqlQuery =
            $@"
            let dateTimeStart = datetime({start:s});
            let dateTimeEnd = datetime({end:s});
            Telemetry
            | where SourceTimestamp between (dateTimeStart .. dateTimeEnd)
                and isnotempty(TrendId)
                {filterByPoints}
            | summarize
                MaxSourceTimestamp = max(SourceTimestamp),
                arg_max(SourceTimestamp, ScalarValue) by TrendId
            | project
                PointEntityId = toguid(TrendId),
                Timestamp = MaxSourceTimestamp,
                Value = ScalarValue
            | order by PointEntityId, Timestamp asc";

        using var reader = await adxQueryRunner.QueryAsync(clientId, kqlQuery);
        var result = reader.Parse<PointTimeSeriesRawData>();

        return result.ToList();
    }

    public async Task<IReadOnlyList<TimeSeriesDataPoint>> GetFirstValuesInsideTimeIntervalsAsync(
        Guid? clientId,
        DateTime start,
        DateTime end,
        string interval,
        List<Guid> pointIds = null)
    {
        var filterByPoints = string.Empty;
        if (pointIds != null)
        {
            filterByPoints = $@" and (TrendId in ({string.Join(", ", pointIds.Select(q => $"'{q}'"))}))";
        }

        // For feature parity with Postgres version, assumes that ScalarValue contains double data type only
        // Needs to be implemented differently to allow truly dynamic values
        // make-series is used after summarize to get all the time buckets for all TrendIds
        var kqlQuery =
            $@"
            let dateTimeStart = datetime({start:s});
            let dateTimeEnd = datetime({end:s});
            let interval = time({interval});
            Telemetry
            | where isfinite(todouble(ScalarValue))
                and isnotnull(SourceTimestamp)
                and SourceTimestamp between (dateTimeStart .. dateTimeEnd)
                and isnotempty(TrendId)
                {filterByPoints}
            | summarize
                arg_min(SourceTimestamp, ScalarValue) by TrendId,
                bin(SourceTimestamp, interval)
            | make-series
                Values = take_any(todouble(ScalarValue)) default = double(null)
                    on SourceTimestamp
                    from dateTimeStart to dateTimeEnd
                    step interval
                    by TrendId
            | mv-expand
                Timestamp = SourceTimestamp,
                Value = Values
            | project
                todatetime(Timestamp),
                PointEntityId = toguid(TrendId),
                todouble(Value)
            | order by PointEntityId, Timestamp asc";

        using var reader = await adxQueryRunner.QueryAsync(clientId, kqlQuery);
        var result = reader.Parse<TimeSeriesDataPoint>();
        return result.ToList();
    }

    public async Task<IReadOnlyList<CumulativeTimeSeriesDataPoint>> GetCumulativeTrendAsync(
        Guid? clientId,
        DateTime start,
        DateTime end,
        string interval,
        List<Guid> pointIds,
        double valueMultiplier)
    {
        var filterByPoints = string.Empty;
        if (pointIds?.Any() == true)
        {
            filterByPoints = $@" and (TrendId in ({string.Join(", ", pointIds.Select(q => $"'{q}'"))}))";
        }

        // For feature parity with Postgres version, assumes that ScalarValue contains double data type only
        // Needs to be implemented differently to allow truly dynamic values
        // make-series is used after summarize to get all the time buckets for all TrendIds
        var kqlQuery =
            $@"
            let dateTimeStart = datetime({start:s});
            let dateTimeEnd = datetime({end:s});
            let interval = time({interval});
            let valueMultiplier = {valueMultiplier};
            Telemetry
            | where isfinite(todouble(ScalarValue))
                and isnotnull(SourceTimestamp)
                and SourceTimestamp >= dateTimeStart and SourceTimestamp < dateTimeEnd
                and isnotempty(TrendId)
                {filterByPoints}
            | summarize
                TotalizedValue = avg(todouble(ScalarValue)) * valueMultiplier by TrendId,
                SourceTimestamp = bin_at(SourceTimestamp, interval, dateTimeStart)
            | make-series
                TotalizedValues = max(todouble(TotalizedValue)) default = double(null),
                IsInterpolated = bool(false) default = bool(true)
                    on todatetime(SourceTimestamp)
                    from dateTimeStart to dateTimeEnd
                    step interval
                    by TrendId
            | mv-expand
                SourceTimestamp,
                IsInterpolated,
                TotalizedValue = TotalizedValues
            | order by TrendId, todatetime(SourceTimestamp) asc
            | serialize
                CumulativeValue = row_cumsum(todouble(TotalizedValue), TrendId != prev(TrendId)),
                IsInterpolated
            | project
                Timestamp = todatetime(SourceTimestamp),
                PointEntityId = toguid(TrendId),
                Value = todouble(CumulativeValue),
                IsInterpolated = tobool(IsInterpolated)";

        using var reader = await adxQueryRunner.QueryAsync(clientId, kqlQuery);
        var result = reader.Parse<CumulativeTimeSeriesDataPoint>();
        return result.ToList();
    }

    public async Task<IReadOnlyList<TimeSeriesDataPoint>> GetCumulativeSumAsync(
        Guid? clientId,
        DateTime start,
        DateTime end,
        string interval,
        List<Guid> pointIds,
        double valueMultiplier)
    {
        var filterByPoints = string.Empty;
        if (pointIds?.Any() == true)
        {
            filterByPoints = $@" and (TrendId in ({string.Join(", ", pointIds.Select(q => $"'{q}'"))}))";
        }

        // For feature parity with Postgres version, assumes that ScalarValue contains double data type only
        // Needs to be implemented differently to allow truly dynamic values
        // make-series is used after summarize to get all the time buckets for all TrendIds
        var kqlQuery =
            $@"
            let dateTimeStart = datetime({start:s});
            let dateTimeEnd = datetime({end:s});
            let interval = time({interval});
            let valueMultiplier = {valueMultiplier};
            Telemetry
            | where isfinite(todouble(ScalarValue))
                and isnotnull(SourceTimestamp)
                and SourceTimestamp >= dateTimeStart and SourceTimestamp < dateTimeEnd
                and isnotempty(TrendId)
                {filterByPoints}
            | summarize
                Value = avg(todouble(ScalarValue)) * valueMultiplier by TrendId,
                SourceTimestamp = bin_at(SourceTimestamp, interval, dateTimeStart)
            | summarize
                CumulativeValue = sum(Value),
                SourceTimestamp = max(SourceTimestamp) by TrendId
            | project
                Timestamp = SourceTimestamp,
                PointEntityId = toguid(TrendId),
                Value = CumulativeValue";

        using var reader = await adxQueryRunner.QueryAsync(clientId, kqlQuery);
        var result = reader.Parse<TimeSeriesDataPoint>();
        return result.ToList();
    }

    public async Task<IReadOnlyList<TimeSeriesDataPoint>> GetAggregatedValuesInsideTimeIntervalsAsync(
        Guid? clientId,
        DateTime start,
        DateTime end,
        string interval,
        List<Guid> pointIds,
        AggregationType aggregationType)
    {
        var filterByPoints = string.Empty;
        if (pointIds != null)
        {
            filterByPoints = $@" and (TrendId in ({string.Join(", ", pointIds.Select(q => $"'{q}'"))}))";
        }

        string aggregationTypeQuery = aggregationType switch
        {
            AggregationType.Avg => "avg",
            AggregationType.Sum => "sum",
            _ => throw new NotSupportedException("Unsupported aggregation type."),
        };

        // For feature parity with Postgres version, assumes that ScalarValue contains double data type only
        // Needs to be implemented differently to allow truly dynamic values
        var kqlQuery =
            $@"
            let dateTimeStart = datetime({start:s});
            let dateTimeEnd = datetime({end:s});
            let interval = time({interval});
            Telemetry
            | where isfinite(todouble(ScalarValue))
                and isnotnull(SourceTimestamp)
                and SourceTimestamp between (dateTimeStart .. dateTimeEnd)
                and isnotempty(TrendId)
                {filterByPoints}
            | make-series
                Values = {aggregationTypeQuery}(todouble(ScalarValue)) default = double(null)
                    on todatetime(SourceTimestamp)
                    from dateTimeStart to dateTimeEnd
                    step interval
                    by TrendId
            | mv-expand
                Timestamp = SourceTimestamp,
                Value = Values
            | project
                todatetime(Timestamp),
                PointEntityId = toguid(TrendId),
                todouble(Value)
            | order by PointEntityId, Timestamp asc";

        using var reader = await adxQueryRunner.QueryAsync(clientId, kqlQuery);
        var result = reader.Parse<TimeSeriesDataPoint>();
        return result.ToList();
    }

    public Task<List<PointStatsData>> GetPointStatsAsync(Guid? clientId, DateTime start)
    {
        throw new NotSupportedException("This is not yet supported for ADX Telemetry table");
    }

    public Task<int> GetPointsStatsCountByIdsListAsync(Guid? clientId, DateTime start, IEnumerable<Guid> pointIds)
    {
        throw new NotSupportedException("This is not yet supported for ADX Telemetry table");
    }

    public Task<PagedTelemetry> GetTelemetryAsync(GetTelemetryRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        return GetTelemetryInternalAsync();

        async Task<PagedTelemetry> GetTelemetryInternalAsync()
        {
            string kqlQuery = GetTelemetryStoredQueryResult(
                request.ConnectorId,
                request.Start,
                request.End,
                request.DtdIds,
                request.TrendIds);
            (string pagedKqlQuery, string continuationToken, int totalRowsCount) = await CreatePagedQueryAsync(
                request.ClientId,
                kqlQuery,
                request.PageSize,
                request.LastRowNumber,
                request.ContinuationToken);

            try
            {
                using var reader = await adxQueryRunner.QueryAsync(request.ClientId, pagedKqlQuery);
                var result = reader.Parse<Telemetry>();
                return new PagedTelemetry
                {
                    Telemetry = result.ToList(),
                    ContinuationToken = continuationToken,
                    TotalRowsCount = totalRowsCount,
                };
            }
            catch (KustoException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new KustoClientException("Error executing the query", ex);
            }
        }
    }

    public string GetTelemetryStoredQueryResult(
        Guid connectorId,
        DateTime start,
        DateTime end,
        List<string> dtdIds,
        List<Guid> trendIds)
    {
        return @$"{GetQueryPrefix(start, end, trendIds)}
            {GetConnectorIdClause(connectorId)}
            {GetdtIdsClause(dtdIds)}
            {FilterDuplicates}
            | order by SourceTimestamp asc, ExternalId asc
        ";
    }

    /// <summary>
    /// Retrieves time series data for a given external ID.
    /// </summary>
    /// <param name="connectorId">The ID of the connector.</param>
    /// <param name="externalId">The external ID.</param>
    /// <param name="clientId">The client ID.</param>
    /// <param name="startUtc">The start date and time in UTC.</param>
    /// <param name="endUtc">The end date and time in UTC.</param>
    /// <param name="interval">The time interval for aggregation.</param>
    /// <returns>A collection of <see cref="TimeSeriesData"/> objects.</returns>
    public async Task<IReadOnlyCollection<TimeSeriesAnalogData>> GetTimeSeriesDataByExternalIdAsync(
       Guid connectorId,
       string externalId,
       Guid? clientId,
       DateTime startUtc,
       DateTime endUtc,
       string interval)
    {
        externalId = externalId.Replace("\\", "\\\\").Replace("'", "\\'").Replace("\"", "\\\"");
        var queryPrefix = @$"Telemetry | where ExternalId == '{externalId}'
                           {GetConnectorIdClause(connectorId)}
                           and SourceTimestamp between (datetime({startUtc:s}) .. datetime({endUtc:s}))";

        var filterDuplicates = @"| summarize arg_max(EnqueuedTimestamp, *) by ExternalId, SourceTimestamp, ConnectorId ";

        var kqlQuery = $@"{queryPrefix} {filterDuplicates}
                            | summarize Average = avg(todouble(ScalarValue)),
                                Minimum = min(todouble(ScalarValue)),
                                Maximum = max(todouble(ScalarValue))
                                by bin(TimeStamp=SourceTimestamp, time({interval})), ExternalId, ConnectorId
                            | order by TimeStamp asc";

        using var reader = await adxQueryRunner.QueryAsync(clientId, kqlQuery);
        var result = reader.Parse<TimeSeriesAnalogData>();

        return result.ToList();
    }
}
