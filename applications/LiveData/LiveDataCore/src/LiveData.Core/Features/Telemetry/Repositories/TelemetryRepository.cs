namespace Willow.LiveData.Core.Features.Telemetry.Repositories;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Kusto.Data.Exceptions;
using Newtonsoft.Json.Linq;
using Willow.LiveData.Core.Common;
using Willow.LiveData.Core.Features.GetLiveData.GetPointLiveData.Extensions;
using Willow.LiveData.Core.Features.GetLiveData.GetPointLiveData.Model;
using Willow.LiveData.Core.Features.Telemetry.DTOs;
using Willow.LiveData.Core.Features.Telemetry.Helpers;
using Willow.LiveData.Core.Features.Telemetry.Interfaces;
using Willow.LiveData.Core.Infrastructure.Database.Adx;

internal class TelemetryRepository : AdxBaseRepository, ITelemetryRepository
{
    private readonly IAdxQueryRunner adxQueryRunner;
    private readonly IAdxQueryHelper adxQueryHelper;
    private readonly IConfiguration configuration;

    public TelemetryRepository(
        IAdxQueryRunner adxQueryRunner,
        IContinuationTokenProvider<string, string> continuationTokenProvider,
        IAdxQueryHelper adxQueryHelper,
        IConfiguration configuration)
        : base(adxQueryRunner, continuationTokenProvider)
    {
        this.adxQueryRunner = adxQueryRunner;
        this.adxQueryHelper = adxQueryHelper;
        this.configuration = configuration;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<TelemetryAnalogResponseData>> GetTelemetryAnalogDataByTwinIdAsync(
        Guid? clientId,
        DateTime start,
        DateTime end,
        string interval,
        string twinId)
    {
        var twinIds = new List<string> { twinId };
        var (telemetryQuery, _) = await GetTelemetryQuery(clientId, start, end, twinIds);
        if (telemetryQuery is null)
        {
            return null;
        }

        var kqlQuery = adxQueryHelper.SummarizeAnalogValues(telemetryQuery, interval);

        using var telemetryReader = await adxQueryRunner.QueryAsync(clientId, kqlQuery.GetQuery());
        var telemetryResult = telemetryReader.Parse<TelemetryAnalogResponseData>().ToList();

        if (telemetryResult.Count == 0)
        {
            return [];
        }

        var dataQualityResult = await GetDataQuality(clientId, start, end, interval, twinIds);

        return telemetryResult
            .GroupJoin(
                dataQualityResult,
                telemetry => telemetry.Timestamp,
                dataQuality => dataQuality.LastValidationUpdatedAt,
                (telemetry, dataQuality) => new { telemetry, dataQuality = dataQuality.DefaultIfEmpty() })
            .SelectMany(group => group.dataQuality, (group, dataQuality) => new TelemetryAnalogResponseData
            {
                IsInvalid = dataQuality?.IsInvalid,
                IsValueOutOfRange = dataQuality?.IsValueOutOfRange,
                Timestamp = group.telemetry.Timestamp,
                Minimum = group.telemetry.Minimum,
                Maximum = group.telemetry.Maximum,
                Average = group.telemetry.Average,
            })
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<(string Id, TelemetrySummaryData TelemetrySummaryData)>>
        GetTelemetryAnalogDataByTwinIdAsync(Guid? clientId,
            DateTime start,
            DateTime end,
            string interval,
            IReadOnlyCollection<string> twinIds)
    {
        var (telemetryQuery, twinResult) = await GetTelemetryQuery(clientId, start, end, twinIds);
        if (telemetryQuery is null)
        {
            return null;
        }

        var kqlQuery = adxQueryHelper.SummarizeAnalogValues(telemetryQuery, interval);

        using var telemetryReader = await adxQueryRunner.QueryAsync(clientId, kqlQuery.GetQuery());
        var telemetryResult = telemetryReader
                             .Parse<TelemetryAnalogResponseData>()
                             .MapTwinId<TelemetryAnalogResponseData>(twinResult).ToList();

        if (telemetryResult.Count == 0)
        {
            return [];
        }

        var dataQualityResult = await GetDataQuality(clientId, start, end, interval, twinIds);

        return telemetryResult
            .GroupJoin(
                dataQualityResult,
                telemetry => telemetry.Timestamp,
                dataQuality => dataQuality.LastValidationUpdatedAt,
                (telemetry, dataQuality) => new { telemetry, dataQuality = dataQuality.DefaultIfEmpty() })
            .SelectMany(group => group.dataQuality, (group, dataQuality) =>
                (group.telemetry.Id, (TelemetrySummaryData)new TelemetryAnalogResponseData
                {
                    IsInvalid = dataQuality?.IsInvalid,
                    IsValueOutOfRange = dataQuality?.IsValueOutOfRange,
                    Timestamp = group.telemetry.Timestamp,
                    Minimum = group.telemetry.Minimum,
                    Maximum = group.telemetry.Maximum,
                    Average = group.telemetry.Average,
                }))
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<TelemetryMultiStateResponseData>> GetTelemetryMultiStateDataByTwinIdAsync(
        Guid? clientId,
        DateTime start,
        DateTime end,
        string interval,
        string twinId)
    {
        var twinIds = new List<string> { twinId };
        var (telemetryQuery, _) = await GetTelemetryQuery(clientId, start, end, twinIds);
        if (telemetryQuery is null)
        {
            return null;
        }

        var kqlQuery = adxQueryHelper.SummarizeMultiStateValues(telemetryQuery, interval);

        using var telemetryReader = await adxQueryRunner.QueryAsync(clientId, kqlQuery.GetQuery());
        var telemetryResult = telemetryReader.Parse<TelemetryRawMultiStateResponseData>().ToList();

        if (telemetryResult.Count == 0)
        {
            return [];
        }

        var dataQualityResult = await GetDataQuality(clientId, start, end, interval, twinIds);

        return telemetryResult
            .GroupJoin(
                dataQualityResult,
                telemetry => telemetry.Timestamp,
                dataQuality => dataQuality.LastValidationUpdatedAt,
                (telemetry, dataQuality) => new { telemetry, dataQuality = dataQuality.DefaultIfEmpty() })
            .SelectMany(group => group.dataQuality, (group, dataQuality) => new TelemetryMultiStateResponseData
            {
                IsInvalid = dataQuality?.IsInvalid,
                IsValueOutOfRange = dataQuality?.IsValueOutOfRange,
                Timestamp = group.telemetry.Timestamp,
                State = group.telemetry.State.ToObject<Dictionary<string, int>>(),
            })
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<(string Id, TelemetrySummaryData TelemetrySummaryData)>>
        GetTelemetryMultiStateDataByTwinIdAsync(Guid? clientId,
            DateTime start,
            DateTime end,
            string interval,
            IReadOnlyCollection<string> twinIds)
    {
        var (telemetryQuery, twinResult) = await GetTelemetryQuery(clientId, start, end, twinIds);
        if (telemetryQuery is null)
        {
            return null;
        }

        var kqlQuery = adxQueryHelper.SummarizeMultiStateValues(telemetryQuery, interval);

        using var telemetryReader = await adxQueryRunner.QueryAsync(clientId, kqlQuery.GetQuery());
        var telemetryResult = telemetryReader
            .Parse<TelemetryRawMultiStateResponseData>()
            .MapTwinId<TelemetryRawMultiStateResponseData>(twinResult).ToList();

        if (telemetryResult.Count == 0)
        {
            return [];
        }

        var dataQualityResult = await GetDataQuality(clientId, start, end, interval, twinIds);

        return telemetryResult
            .GroupJoin(
                dataQualityResult,
                telemetry => telemetry.Timestamp,
                dataQuality => dataQuality.LastValidationUpdatedAt,
                (telemetry, dataQuality) => new { telemetry, dataQuality = dataQuality.DefaultIfEmpty() })
            .SelectMany(group => group.dataQuality, (group, dataQuality) =>
                (group.telemetry.Id, (TelemetrySummaryData)new TelemetryMultiStateResponseData
                {
                    IsInvalid = dataQuality?.IsInvalid,
                    IsValueOutOfRange = dataQuality?.IsValueOutOfRange,
                    Timestamp = group.telemetry.Timestamp,
                    State = group.telemetry.State.ToObject<Dictionary<string, int>>(),
                }))
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<TelemetryBinaryResponseData>> GetTelemetryBinaryDataByTwinIdAsync(
        Guid? clientId,
        DateTime start,
        DateTime end,
        string interval,
        string twinId)
    {
        var twinIds = new List<string> { twinId };
        var (telemetryQuery, _) = await GetTelemetryQuery(clientId, start, end, twinIds);
        if (telemetryQuery is null)
        {
            return null;
        }

        var kqlQuery = adxQueryHelper.SummarizeBinaryValues(telemetryQuery, interval);

        using var telemetryReader = await adxQueryRunner.QueryAsync(clientId, kqlQuery.GetQuery());
        var telemetryResult = telemetryReader.Parse<TelemetryBinaryResponseData>().ToList();

        if (telemetryResult.Count == 0)
        {
            return [];
        }

        var dataQualityResult = await GetDataQuality(clientId, start, end, interval, twinIds);

        return telemetryResult
            .GroupJoin(
                dataQualityResult,
                telemetry => telemetry.Timestamp,
                dataQuality => dataQuality.LastValidationUpdatedAt,
                (telemetry, dataQuality) => new { telemetry, dataQuality = dataQuality.DefaultIfEmpty() })
            .SelectMany(group => group.dataQuality, (group, dataQuality) => new TelemetryBinaryResponseData
            {
                IsInvalid = dataQuality?.IsInvalid,
                IsValueOutOfRange = dataQuality?.IsValueOutOfRange,
                Timestamp = group.telemetry.Timestamp,
                OffCount = group.telemetry.OffCount,
                OnCount = group.telemetry.OnCount,
            })
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<(string Id, TelemetrySummaryData TelemetrySummaryData)>>
        GetTelemetryBinaryDataByTwinIdAsync(Guid? clientId,
            DateTime start,
            DateTime end,
            string interval,
            IReadOnlyCollection<string> twinIds)
    {
        var (telemetryQuery, twinResult) = await GetTelemetryQuery(clientId, start, end, twinIds);
        if (telemetryQuery is null)
        {
            return null;
        }

        var kqlQuery = adxQueryHelper.SummarizeBinaryValues(telemetryQuery, interval);

        using var telemetryReader = await adxQueryRunner.QueryAsync(clientId, kqlQuery.GetQuery());
        var telemetryResult = telemetryReader
                             .Parse<TelemetryBinaryResponseData>()
                             .MapTwinId<TelemetryBinaryResponseData>(twinResult).ToList();

        if (telemetryResult.Count == 0)
        {
            return [];
        }

        var dataQualityResult = await GetDataQuality(clientId, start, end, interval, twinIds);

        return telemetryResult
            .GroupJoin(
                dataQualityResult,
                telemetry => telemetry.Timestamp,
                dataQuality => dataQuality.LastValidationUpdatedAt,
                (telemetry, dataQuality) => new { telemetry, dataQuality = dataQuality.DefaultIfEmpty() })
            .SelectMany(group => group.dataQuality, (group, dataQuality) =>
                (group.telemetry.Id, (TelemetrySummaryData)new TelemetryBinaryResponseData
                {
                    IsInvalid = dataQuality?.IsInvalid,
                    IsValueOutOfRange = dataQuality?.IsValueOutOfRange,
                    Timestamp = group.telemetry.Timestamp,
                    OnCount = group.telemetry.OnCount,
                    OffCount = group.telemetry.OffCount,
                }))
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<TelemetrySumResponseData>> GetTelemetrySumDataByTwinIdAsync(
        Guid? clientId,
        DateTime start,
        DateTime end,
        string interval,
        string twinId)
    {
        var twinIds = new List<string> { twinId };
        var (telemetryQuery, _) = await GetTelemetryQuery(clientId, start, end, twinIds);
        if (telemetryQuery is null)
        {
            return null;
        }

        var kqlQuery = adxQueryHelper.SummarizeSumValues(telemetryQuery, interval);

        using var telemetryReader = await adxQueryRunner.QueryAsync(clientId, kqlQuery.GetQuery());
        var telemetryResult = telemetryReader.Parse<TelemetrySumResponseData>().ToList();

        if (telemetryResult.Count == 0)
        {
            return [];
        }

        var dataQualityResult = await GetDataQuality(clientId, start, end, interval, twinIds);

        return telemetryResult
            .GroupJoin(
                dataQualityResult,
                telemetry => telemetry.Timestamp,
                dataQuality => dataQuality.LastValidationUpdatedAt,
                (telemetry, dataQuality) => new { telemetry, dataQuality = dataQuality.DefaultIfEmpty() })
            .SelectMany(group => group.dataQuality, (group, dataQuality) => new TelemetrySumResponseData
            {
                IsInvalid = dataQuality?.IsInvalid,
                IsValueOutOfRange = dataQuality?.IsValueOutOfRange,
                Timestamp = group.telemetry.Timestamp,
                Sum = group.telemetry.Sum,
            })
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<(string Id, TelemetrySummaryData TelemetrySummaryData)>>
        GetTelemetrySumDataByTwinIdAsync(Guid? clientId,
            DateTime start,
            DateTime end,
            string interval,
            IReadOnlyCollection<string> twinIds)
    {
        var (telemetryQuery, twinResult) = await GetTelemetryQuery(clientId, start, end, twinIds);
        if (telemetryQuery is null)
        {
            return null;
        }

        var kqlQuery = adxQueryHelper.SummarizeSumValues(telemetryQuery, interval);

        using var telemetryReader = await adxQueryRunner.QueryAsync(clientId, kqlQuery.GetQuery());
        var telemetryResult = telemetryReader
                             .Parse<TelemetrySumResponseData>()
                             .MapTwinId<TelemetrySumResponseData>(twinResult).ToList();

        if (telemetryResult.Count == 0)
        {
            return [];
        }

        var dataQualityResult = await GetDataQuality(clientId, start, end, interval, twinIds);

        return telemetryResult
            .GroupJoin(
                dataQualityResult,
                telemetry => telemetry.Timestamp,
                dataQuality => dataQuality.LastValidationUpdatedAt,
                (telemetry, dataQuality) => new { telemetry, dataQuality = dataQuality.DefaultIfEmpty() })
            .SelectMany(group => group.dataQuality, (group, dataQuality) =>
                (group.telemetry.Id, (TelemetrySummaryData)new TelemetrySumResponseData
                {
                    IsInvalid = dataQuality?.IsInvalid,
                    IsValueOutOfRange = dataQuality?.IsValueOutOfRange,
                    Timestamp = group.telemetry.Timestamp,
                    Sum = group.telemetry.Sum,
                }))
            .ToList();
    }

    /// <inheritdoc/>
    [Obsolete("Use GetTelemetryRawDataAsync instead")]
    public async Task<IReadOnlyCollection<TelemetryRawData>> GetTelemetryRawDataBySiteIdAsync(Guid? clientId,
        Guid siteId,
        DateTime start,
        DateTime end,
        IEnumerable<string> twinIds)
    {
        var (telemetryQuery, twinResult) = await GetTelemetryQuery(clientId, start, end, twinIds, siteId);
        if (telemetryQuery is null)
        {
            return null;
        }

        var kqlQuery = ((telemetryQuery.Project($"Timestamp = {TelemetryTable.SourceTimestamp}, " +
                                                $"Value = {TelemetryTable.TelemetryField}, " +
                                                $"{TelemetryTable.ExternalId}, " +
                                                $"{TelemetryTable.TrendId}")
                             as IAdxQueryFilterGroup)!
                       .Order("Timestamp")
                        as IAdxQuerySelector)!;

        using var reader = await adxQueryRunner.QueryAsync(clientId, kqlQuery.GetQuery());
        var telemetryResult = reader
                             .Parse<TelemetryRawData>()
                             .MapTwinId<TelemetryRawData>(twinResult)
                             .ToList();

        return telemetryResult;
    }

    /// <inheritdoc/>
    [Obsolete("Use GetLastTelemetryRawDataAsync instead")]
    public async Task<IReadOnlyCollection<TelemetryRawData>> GetLastTelemetryRawDataBySiteIdAsync(Guid? clientId,
                                                                                                  Guid siteId,
                                                                                                  IEnumerable<string> twinIds = null)
    {
        var startDate = DateTime.UtcNow - TimeSpan.FromHours(1);
        var endDate = DateTime.UtcNow;

        var (telemetryQuery, twinResult) = await GetTelemetryQuery(
                                                                   clientId,
                                                                   startDate,
                                                                   endDate,
                                                                   twinIds,
                                                                   siteId,
                                                                   true);
        if (telemetryQuery is null)
        {
            return null;
        }

        var kqlQuery = ((telemetryQuery.Project($"Timestamp = {TelemetryTable.SourceTimestamp}, " +
                                                $"Value = {TelemetryTable.TelemetryField}, " +
                                                $"{TelemetryTable.ExternalId}, " +
                                                $"{TelemetryTable.TrendId}")
                             as IAdxQueryFilterGroup)!
                        .Order("Timestamp")
                        as IAdxQuerySelector)!;

        using var reader = await adxQueryRunner.QueryAsync(clientId, kqlQuery.GetQuery());
        var telemetryResult = reader
                             .Parse<TelemetryRawData>()
                             .MapTwinId<TelemetryRawData>(twinResult).ToList();

        return telemetryResult.Select(x => new TelemetryRawData
        {
            Id = x.Id,
            Timestamp = x.Timestamp,
            Value = x.Value,
        }).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<TelemetryRawData>> GetTelemetryRawDataAsync(
        Guid? clientId,
        DateTime start,
        DateTime end,
        IEnumerable<string> twinIds)
    {
        var (telemetryQuery, twinResult) = await GetTelemetryQuery(clientId, start, end, twinIds);
        if (telemetryQuery is null)
        {
            return null;
        }

        var kqlQuery = ((telemetryQuery.Project($"Timestamp = {TelemetryTable.SourceTimestamp}, " +
                                                $"Value = iff(tolower({TelemetryTable.TelemetryField}) == 'true' or tolower({TelemetryTable.TelemetryField}) == 'false', iff(tolower({TelemetryTable.TelemetryField}) == 'true', 1, 0), {TelemetryTable.TelemetryField}), " +
                                                $"{TelemetryTable.ExternalId}, " +
                                                $"{TelemetryTable.TrendId}")
                             as IAdxQueryFilterGroup)!
                       .Order("Timestamp")
                        as IAdxQuerySelector)!;

        using var reader = await adxQueryRunner.QueryAsync(clientId, kqlQuery.GetQuery());
        var telemetryResult = reader
                             .Parse<TelemetryRawData>()
                             .MapTwinId<TelemetryRawData>(twinResult)
                             .ToList();

        return telemetryResult;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<TelemetryRawData>> GetLastTelemetryRawDataAsync(Guid? clientId,
                                                                                                  IEnumerable<string> twinIds = null)
    {
        var startDate = DateTime.UtcNow - TimeSpan.FromHours(1);
        var endDate = DateTime.UtcNow;

        var (telemetryQuery, twinResult) = await GetTelemetryQuery(
                                                                   clientId,
                                                                   startDate,
                                                                   endDate,
                                                                   twinIds,
                                                                   latestOnly: true);
        if (telemetryQuery is null)
        {
            return null;
        }

        var kqlQuery = ((telemetryQuery.Project($"Timestamp = {TelemetryTable.SourceTimestamp}, " +
                                                $"Value = iff(tolower({TelemetryTable.TelemetryField}) == 'true' or tolower({TelemetryTable.TelemetryField}) == 'false', iff(tolower({TelemetryTable.TelemetryField}) == 'true', 1, 0), {TelemetryTable.TelemetryField}), " +
                                                $"{TelemetryTable.ExternalId}, " +
                                                $"{TelemetryTable.TrendId}")
                as IAdxQueryFilterGroup)!
            .Order("Timestamp")
            as IAdxQuerySelector)!;

        using var reader = await adxQueryRunner.QueryAsync(clientId, kqlQuery.GetQuery());
        var result = reader
                             .Parse<TelemetryRawData>()
                             .MapTwinId<TelemetryRawData>(twinResult).ToList();

        return result.Select(x => new TelemetryRawData
        {
            Id = x.Id,
            Timestamp = x.Timestamp,
            Value = x.Value,
        }).ToList();
    }

    public async Task<IReadOnlyCollection<Telemetry>> GetLastTelemetryAsync(IEnumerable<string> twinIds)
    {
        var start = DateTime.UtcNow - TimeSpan.FromHours(1);
        var end = DateTime.UtcNow;

        var (telemetryQuery, twinResult) = await GetTelemetryQuery(null, start, end, twinIds, latestOnly: true);
        if (telemetryQuery is null)
        {
            return null;
        }

        var kqlQuery = ((telemetryQuery as IAdxQueryFilterGroup)!
                       .Order(TelemetryTable.SourceTimestamp)
                       as IAdxQuerySelector)!;

        try
        {
            using var reader = await adxQueryRunner.QueryAsync(null, kqlQuery.GetQuery());
            var result = reader.Parse<Telemetry>().ToList();

            foreach (var telemetry in result)
            {
                telemetry.DtId = string.IsNullOrEmpty(telemetry.TrendId)
                                     ? twinResult.FirstOrDefault(x => x.ExternalId == telemetry.ExternalId)?.Id
                                     : twinResult.FirstOrDefault(x => x.TrendId.ToString() == telemetry.TrendId)?.Id;
            }

            return result;
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

    /// <inheritdoc/>
    public async Task<PagedTelemetry> GetTelemetryAsync(Guid? clientId,
                                                  DateTime start,
                                                  DateTime end,
                                                  int pageSize,
                                                  string continuationToken,
                                                  IEnumerable<string> twinIds,
                                                  int lastRowNumber)
    {
        var (telemetryQuery, twinResult) = await GetTelemetryQuery(clientId, start, end, twinIds);
        if (telemetryQuery is null)
        {
            return null;
        }

        var kqlQuery = ((telemetryQuery as IAdxQueryFilterGroup)!
                       .Order(TelemetryTable.SourceTimestamp)
                       as IAdxQuerySelector)!;

        var (pagedKqlQuery, ctxToken, totalRowsCount) = await CreatePagedQueryAsync(
                                                                                             clientId,
                                                                                             kqlQuery.GetQuery(),
                                                                                             pageSize,
                                                                                             lastRowNumber,
                                                                                             continuationToken);

        try
        {
            using var reader = await adxQueryRunner.QueryAsync(clientId, pagedKqlQuery);
            var result = reader.Parse<Telemetry>().ToList();

            //Populate id from twinResult
            foreach (var telemetry in result)
            {
                telemetry.DtId = string.IsNullOrEmpty(telemetry.TrendId)
                                     ? twinResult.FirstOrDefault(x => x.ExternalId == telemetry.ExternalId)?.Id
                                     : twinResult.FirstOrDefault(x => x.TrendId.ToString() == telemetry.TrendId)?.Id;

                if (telemetry.ScalarValue is JValue jValue)
                {
                    telemetry.ScalarValue = jValue.Value;
                }
            }

            return new PagedTelemetry
            {
                Telemetry = result,
                ContinuationToken = ctxToken,
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

    private async Task<List<DataQualityResponseData>> GetDataQuality(Guid? clientId, DateTime start, DateTime end, string interval, IReadOnlyCollection<string> twinIds)
    {
        if (!configuration.GetValue<bool>("EnableDataQuality"))
        {
            return [];
        }

        var query = $"""
                     let dates = range LastValidationUpdatedAt from datetime({start:s}) to datetime({end:s}) step time({interval});
                     TelemetryDataQuality
                        {GetExternalIdsClause(twinIds)}
                     | where {DataQualityTelemetryTable.LastValidationUpdatedAt} between (datetime({start:s}) .. datetime({end:s}))
                     | make-series
                         IsNull=countif(isempty({DataQualityTelemetryTable.ValidationResults})) default=real(null),
                         ValueOutOfRangeCount=countif({DataQualityTelemetryTable.ValidationResults}["ValueOutOfRange"] == true) default=real(null),
                         InvalidCount=countif({DataQualityTelemetryTable.ValidationResults}["Valid"] == false) default=real(null)
                         on {DataQualityTelemetryTable.LastValidationUpdatedAt}
                         in range (datetime({start:s}), datetime({end:s}), time({interval}))
                     | extend IsNull = series_fill_forward(IsNull)
                     | extend ValueOutOfRangeCount = series_fill_forward(ValueOutOfRangeCount)
                     | extend InvalidCount = series_fill_forward(InvalidCount)
                     | mv-expand {DataQualityTelemetryTable.LastValidationUpdatedAt}, ValueOutOfRangeCount, InvalidCount, IsNull
                     | extend {DataQualityTelemetryTable.LastValidationUpdatedAt}=todatetime({DataQualityTelemetryTable.LastValidationUpdatedAt})
                     | join kind=rightouter dates on {DataQualityTelemetryTable.LastValidationUpdatedAt}
                     | extend {DataQualityTelemetryTable.LastValidationUpdatedAt}={DataQualityTelemetryTable.LastValidationUpdatedAt}1
                     | order by {DataQualityTelemetryTable.LastValidationUpdatedAt} asc
                     | project
                         IsNull=tobool(IsNull),
                         IsValueOutOfRange=tobool(ValueOutOfRangeCount),
                         IsInvalid=tobool(InvalidCount),
                         {DataQualityTelemetryTable.LastValidationUpdatedAt};
                     """;
        var reader = await adxQueryRunner.QueryAsync(clientId, query);
        var result = reader.Parse<DataQualityResponseData>().ToList();

        var firstEntry = result.FirstOrDefault();
        if (firstEntry is not null && !firstEntry.IsNull.HasValue)
        {
            await PopulateWithLastKnownDataQuality(clientId, start, interval, twinIds, result);
        }

        return result;
    }

    private async Task PopulateWithLastKnownDataQuality(Guid? clientId, DateTime start, string interval, IReadOnlyCollection<string> twinIds, List<DataQualityResponseData> result)
    {
        var lastKnownQuery = $"""
                               TelemetryDataQuality
                                    {GetExternalIdsClause(twinIds)}
                                   | where {DataQualityTelemetryTable.LastValidationUpdatedAt} < datetime({start:s})
                                   | order by {DataQualityTelemetryTable.LastValidationUpdatedAt} desc
                                   | take 1
                                   | extend ValueOutOfRange=tobool({DataQualityTelemetryTable.ValidationResults}["ValueOutOfRange"])
                                   | extend IsValid=tobool({DataQualityTelemetryTable.ValidationResults}["Valid"])
                                   | extend IsNull=isempty({DataQualityTelemetryTable.ValidationResults})
                                   | summarize by bin({DataQualityTelemetryTable.LastValidationUpdatedAt}, time({interval})), ValueOutOfRange, IsValid, IsNull
                                   | project {DataQualityTelemetryTable.LastValidationUpdatedAt}, IsValueOutOfRange=ValueOutOfRange, IsInvalid=IsValid, IsNull;
                              """;
        var lastKnownReader = await adxQueryRunner.QueryAsync(clientId, lastKnownQuery);
        var lastKnownResult = lastKnownReader.Parse<DataQualityResponseData>().FirstOrDefault();
        if (lastKnownResult is not null)
        {
            foreach (var item in result)
            {
                if (item.IsNull is false)
                {
                    break;
                }

                item.IsValueOutOfRange ??= lastKnownResult.IsValueOutOfRange;
                item.IsInvalid ??= lastKnownResult.IsInvalid;
            }
        }
    }

    private async Task<List<TwinDetails>> PopulateIds(Guid? clientId, IAdxQueryBuilder kqlQuery)
    {
        using var twinReader = await adxQueryRunner.QueryAsync(clientId, kqlQuery.GetQuery());
        return twinReader.Parse<TwinDetails>().ToList();
    }

    private async Task<(IAdxQuerySelector QuerySelector, List<TwinDetails> TwinDetails)> GetTelemetryQuery(Guid? clientId,
                                                                                 DateTime start,
                                                                                 DateTime end,
                                                                                 IEnumerable<string> twinIds,
                                                                                 Guid? siteId = null,
                                                                                 bool latestOnly = false)
    {
        var twinList = twinIds?.ToList();
        if ((twinList is null || !twinList.Any()) && siteId is null)
        {
            return (null, new List<TwinDetails>());
        }

        var kqlQuery = adxQueryHelper.GetIdsForTwins(twinList, siteId);
        var twinResult = await PopulateIds(clientId, kqlQuery);

        if (twinResult.Count == 0)
        {
            return (null, new List<TwinDetails>());
        }

        var telemetryValuesQuery = adxQueryHelper.GetTelemetryValues(start, end, twinResult);
        telemetryValuesQuery = latestOnly ? adxQueryHelper.GetLatestValue(telemetryValuesQuery) : adxQueryHelper.RemoveDuplicates(telemetryValuesQuery);

        return (telemetryValuesQuery, twinResult);
    }
}
