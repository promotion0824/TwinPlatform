namespace Willow.LiveData.Core.Features.Connectors.Repositories;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Willow.LiveData.Core.Common;
using Willow.LiveData.Core.Features.Connectors.DTOs;
using Willow.LiveData.Core.Features.Connectors.Interfaces;
using Willow.LiveData.Core.Features.Connectors.Models;
using Willow.LiveData.Core.Infrastructure.Database.Adx;

internal class AdxConnectorRepository : AdxBaseRepository, IAdxConnectorRepository
{
    private const int HourlyDayRange = 7;
    private const string HourlyBin = "60";
    private const string FourHourBin = "240";
    private readonly IAdxQueryRunner adxQueryRunner;
    private readonly IContinuationTokenProvider<string, string> continuationTokenProvider;

    public AdxConnectorRepository(IAdxQueryRunner adxQueryRunner, IContinuationTokenProvider<string, string> continuationTokenProvider)
        : base(adxQueryRunner, continuationTokenProvider)
    {
        this.adxQueryRunner = adxQueryRunner;
        this.continuationTokenProvider = continuationTokenProvider;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ConnectorTelemetryBucket>> GetTelemetryCountByLastXHoursAsync(Guid? clientId, List<Guid> connectorIds, DateTime start, DateTime end, bool singleBin = false)
    {
        var kqlQuery = @$"
                set query_bin_auto_size={GetBinByDateRange(start, end, singleBin)}m;
                set query_bin_auto_at=datetime({start:s});
                Telemetry
                {GetConnectorIdsClause(connectorIds)}
                {GetSourceTimestampClause(start, end)}
                {GetSkipHeartBeatClause()}
                | summarize Telemetry_Count = count(), TrendIdCount=dcount(TrendId), ExternalIdCount=dcount(ExternalId) by bin_auto(SourceTimestamp), ConnectorId
                | make-series Telemetries = take_any(Telemetry_Count), TrendIds = take_any(TrendIdCount), ExternalIds = take_any(ExternalIdCount)
                    {GetMakeSeriesTimestampClause(start, end)} step {GetBinByDateRange(start, end, singleBin)}m by ConnectorId
                | mv-expand Timestamp = SourceTimestamp,
                    Telemetry_Count = {GetSeriesFillConstClause("Telemetries")},
                    Unique_TrendIdCount = {GetSeriesFillConstClause("TrendIds")},
                    Unique_ExternalIdCount = {GetSeriesFillConstClause("ExternalIds")}
                | project ConnectorId = tostring(ConnectorId), StartTimestamp = todatetime(Timestamp), TotalTelemetryCount= tolong(Telemetry_Count),
                    UniqueCapabilityCount=max_of(tolong(Unique_TrendIdCount), tolong(Unique_ExternalIdCount))
                | extend EndTimestamp = datetime_add('minute', toint({GetBinByDateRange(start, end, singleBin)}), StartTimestamp)
                | order by ConnectorId, StartTimestamp";

        using var reader = await adxQueryRunner.QueryAsync(clientId, kqlQuery);
        return reader.Parse<ConnectorTelemetryBucket>().ToList();
    }

    /// <inheritdoc/>
    public async Task<List<UniqueTrends>> GetTelemetryAndCapabilityCountByLastXHoursAsync(Guid? clientId, List<Guid> connectorIds, DateTime start, DateTime end)
    {
        var kqlQuery = @$"
            ActiveTwins
            | where  isnotempty( TrendId)
            {GetConnectorIdsClause(connectorIds)}
            | extend Status = tolower(Raw.customProperties.enabled)
            | summarize ActiveCapabilities = countif(Status=='true'), InactiveCapabilities = countif(Status == 'false') by ConnectorId
            | project tostring(ConnectorId), ActiveCapabilities, InactiveCapabilities, TotalCapabilities=ActiveCapabilities+InactiveCapabilities
            | join kind=inner(
                Telemetry
                {GetConnectorIdsClause(connectorIds)}
                {GetSourceTimestampClause(start, end)}
                {GetSkipHeartBeatClause()}
                | distinct ConnectorId, TrendId, ExternalId
                | summarize TrendingCapabilities = count() by ConnectorId
                )
                on $left.ConnectorId == $right.ConnectorId
            | project ConnectorId, TotalCapabilities, ActiveCapabilities, InactiveCapabilities, TrendingCapabilities";

        using var reader = await adxQueryRunner.QueryAsync(clientId, kqlQuery);
        return reader.Parse<UniqueTrends>().ToList();
    }

    /// <inheritdoc/>
    public async Task<List<MissingTrendsDetail>> GetMissingTrendsForXHoursAsync(Guid? clientId, List<Guid> connectorIds, DateTime start, DateTime end)
    {
        var kqlQuery = @$"
            let trendingTelemetry = Telemetry
            {GetConnectorIdsClause(connectorIds)}
            {GetSourceTimestampClause(start, end)}
            | distinct ConnectorId, toguid(TrendId), ExternalId;
            let activeTwins = ActiveTwins
            | where isnotempty(TrendId)
            {GetConnectorIdsClause(connectorIds)}
            | extend Status = tolower(Raw.customProperties.enabled)
            | where Status == 'true';
            trendingTelemetry
            | where  isnotempty(TrendId)
            | join kind = inner (
                activeTwins
                ) on ($left.TrendId == $right.TrendId)
            | union kind= outer (
                trendingTelemetry
                | where  isempty(TrendId)
                | join kind= inner (
                    activeTwins
                    ) on $left.ExternalId == $right.ExternalId
                )
            | join kind = rightanti(
                activeTwins
                ) on $left.TrendId == $right.TrendId
            | join kind= leftouter (
                ActiveRelationships
                | where Name in ('hostedBy', 'isCapabilityOf')
                | summarize IsHostedBy = strcat_array(make_list_if(TargetId, Name == 'hostedBy'), ','), IsCapabilityOf = strcat_array(make_list_if(TargetId, Name == 'isCapabilityOf'), ',')  by SourceId
                ) on $left.Id == $right.SourceId
            | project ConnectorId, TwinId = Id, TrendId, ExternalId, Name, Model = ModelId, IsCapabilityOf, IsHostedBy";

        using var reader = await adxQueryRunner.QueryAsync(clientId, kqlQuery);
        return reader.Parse<CapabilityDetail>().GroupBy(x => x.ConnectorId, x => x).Select(x =>
        new MissingTrendsDetail
        {
            ConnectorId = x.Key,
            Details = x.ToList(),
        }).ToList();
    }

    private static string GetBinByDateRange(DateTime start, DateTime end, bool singleBin = false)
    {
        if (singleBin)
        {
            var duration = end - start;
            return ((int)duration.TotalMinutes).ToString(CultureInfo.InvariantCulture);
        }

        var dateRangeInDays = (end - start).Days;
        return dateRangeInDays > HourlyDayRange ? FourHourBin : HourlyBin;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ConnectorStats>> GetConnectorStatusAsync(Guid? clientId, List<Guid> connectorIds)
    {
        var kqlQuery = @$"
                ActiveTwins
                | where  isnotempty( TrendId)
                {GetConnectorIdsClause(connectorIds)}
                | extend Status = tolower(Raw.customProperties.enabled)
                | summarize ActiveTwins_CountAll = count(), ActiveTwins_CountInactive = countif(Status == 'false') by ConnectorId, Id
                | summarize Capabilities_Count = sum(ActiveTwins_CountAll), Disabled_Capabilities_Count = sum(ActiveTwins_CountInactive)  by ConnectorId
                | join kind = inner(ActiveTwins
                | where  isnotempty(TrendId)
                {GetConnectorIdsClause(connectorIds)}
                | extend Status = tolower(Raw.customProperties.enabled)
                | join kind = inner(ActiveRelationships | where Name == 'hostedBy')
                    on  $left.Id == $right.SourceId
                | summarize Hosting_Devices_Count = dcount(TargetId) by ConnectorId)
                on $left.ConnectorId == $right.ConnectorId
                | project ConnectorId, CapabilitiesCount = Capabilities_Count, DisabledCapabilitiesCount = Disabled_Capabilities_Count, HostingDevicesCount = Hosting_Devices_Count";

        using var reader = await adxQueryRunner.QueryAsync(clientId, kqlQuery);
        var connectorStats = reader.Parse<ConnectorStats>().ToList();

        foreach (var connectorId in connectorIds.Where(connectorId => !connectorStats.Exists(c => c.ConnectorId == connectorId)))
        {
            connectorStats.Add(new ConnectorStats { ConnectorId = connectorId });
        }

        return connectorStats;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ConnectorState>> GetConnectorStateOvertimeAsync(Guid? clientId, List<Guid> connectorIds, DateTime start, DateTime end)
    {
        var kqlQuery = @$"
                ConnectorState
                {GetConnectorIdsClause(connectorIds)}";
        using var reader = await adxQueryRunner.QueryAsync(clientId, kqlQuery);
        return reader.Parse<ConnectorState>().ToList();
    }

    private static string GetSourceTimestampClause(DateTime start, DateTime end)
    {
        return $"| where SourceTimestamp between (datetime({start:s}) .. datetime({end:s}))";
    }

    private static string GetSkipHeartBeatClause()
    {
        return $"| where ExternalId !has 'Heartbeat' ";
    }

    private static string GetMakeSeriesTimestampClause(DateTime start, DateTime end)
    {
        return $"on SourceTimestamp from datetime({start:s}) to datetime({end:s})";
    }

    private static string GetSeriesFillConstClause(string columnName, string constantValue = "0")
    {
        return $"series_fill_const({columnName}, {constantValue})";
    }
}
