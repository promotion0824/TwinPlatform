namespace Willow.LiveData.Core.Features.Connectors.Helpers;

using System.Collections.Generic;
using System.Linq;
using Willow.LiveData.Core.Features.Connectors.ConnectionTypeRule;
using Willow.LiveData.Core.Features.Connectors.DTOs;
using Willow.LiveData.Core.Features.Connectors.Models;

internal class ConnectorStatsResultHelper : IConnectorStatsResultHelper
{
    private readonly IConnectorTypeFactory connectorFactory;

    public ConnectorStatsResultHelper(IConnectorTypeFactory connectorFactory)
    {
        this.connectorFactory = connectorFactory;
    }

    /// <inheritdoc/>
    public IEnumerable<ConnectorStatsDto> UpdateConnectorStatsDto(IEnumerable<ConnectorStats> connectorStats,
                                                                  IEnumerable<ConnectorTelemetryBucket> connectorTelemetryBuckets,
                                                                  IEnumerable<ConnectorTelemetryBucket> currentTelemetryBuckets,
                                                                  IEnumerable<ConnectorState> connectorStates)
    {
        var connectorStatsDtoList = new List<ConnectorStatsDto>();
        var connectorTelemetryBucketsList = connectorTelemetryBuckets.ToList();
        var currentTelemetryBucketsList = currentTelemetryBuckets.ToList();
        var connectorStateList = connectorStates.ToList();
        var connectorStatsList = connectorStats.ToList();

        if (!connectorStatsList.Any() || (!connectorTelemetryBucketsList.Any() && !connectorStateList.Any()))
        {
            return Enumerable.Empty<ConnectorStatsDto>();
        }

        foreach (var conStatus in connectorStatsList)
        {
            connectorStatsDtoList.Add(
                                      MapTo(conStatus,
                                            connectorTelemetryBucketsList.Where(x => x.ConnectorId == conStatus.ConnectorId.ToString()),
                                            currentTelemetryBucketsList.Where(x => x.ConnectorId == conStatus.ConnectorId.ToString()),
                                            connectorStateList.Where(x => x.ConnectorId.ToString() == conStatus.ConnectorId.ToString())));
        }

        return connectorStatsDtoList;
    }

    /// <inheritdoc/>
    public ConnectorStatsDto MapTo(ConnectorStats connectorStats,
                                   IEnumerable<ConnectorTelemetryBucket> telemetry,
                                   IEnumerable<ConnectorTelemetryBucket> currentTelemetry,
                                   IEnumerable<ConnectorState> connectorStates)
    {
        var connectorStatesList = connectorStates.ToList();
        var telemetryList = telemetry.ToList();
        var currentTelemetryList = currentTelemetry.ToList();
        var connector = GetConnector(connectorStats, connectorStatesList);
        UpdateExpectedTelemetryCounts(connectorStats, currentTelemetryList, connectorStatesList);

        return new ConnectorStatsDto
        {
            TotalCapabilitiesCount = connectorStats?.CapabilitiesCount ?? 0,
            ConnectorId = connectorStats?.ConnectorId ?? System.Guid.Empty,
            DisabledCapabilitiesCount = connectorStats?.DisabledCapabilitiesCount ?? 0,
            HostingDevicesCount = connectorStats?.HostingDevicesCount ?? 0,
            CurrentSetState = connector.GetSetState(connectorStatesList).ToString(),
            CurrentStatus = connector.GetStatus(connectorStats, connectorStatesList, currentTelemetryList.FirstOrDefault()).ToString(),
            Telemetry = GetTelemetryBucketDto(connectorStats, telemetryList, connectorStatesList, connector),
        };
    }

    private IConnectorType GetConnector(ConnectorStats connectorStats, IEnumerable<ConnectorState> connectorStates)
    {
        var latestConnectorState = GetLatestState(connectorStats, connectorStates);
        var connector = connectorFactory.GetConnector(latestConnectorState?.ConnectionType ?? "VM");
        return connector;
    }

    private static ConnectorState GetLatestState(ConnectorStats connectorStats, IEnumerable<ConnectorState> connectorStates)
    {
        return connectorStates?.
               OrderByDescending(x => x.TimestampUtc).
               FirstOrDefault(x => x.ConnectorId.ToString() == connectorStats.ConnectorId.ToString());
    }

    private static IEnumerable<ConnectorTelemetryBucketDto> GetTelemetryBucketDto(ConnectorStats connectorStats,
                                                                                  IEnumerable<ConnectorTelemetryBucket> telemetry,
                                                                                  IEnumerable<ConnectorState> connectorStates,
                                                                                  IConnectorType connector)
    {
        telemetry = telemetry?.OrderByDescending(x => x.StartTimestamp);
        return GetConnectorTelemetryBucketDtoList(connectorStats, telemetry, connectorStates, connector);
    }

    private static IEnumerable<ConnectorTelemetryBucketDto> GetConnectorTelemetryBucketDtoList(ConnectorStats connectorStats,
                                                                                               IEnumerable<ConnectorTelemetryBucket> telemetry,
                                                                                               IEnumerable<ConnectorState> connectorStates,
                                                                                               IConnectorType connector)
    {
        var connectorTelemetryBucketDtoList = new List<ConnectorTelemetryBucketDto>();
        var connectorStatesList = connectorStates.ToList();
        var telemetryList = telemetry.ToList();
        UpdateExpectedTelemetryCounts(connectorStats, telemetryList, connectorStatesList);
        foreach (var item in telemetryList)
        {
            connectorTelemetryBucketDtoList.Add(GetConnectorTelemetryBucketDto(connectorStats,
                                                                               connectorStatesList,
                                                                               connector,
                                                                               item));
        }

        return connectorTelemetryBucketDtoList;
    }

    private static ConnectorTelemetryBucketDto GetConnectorTelemetryBucketDto(ConnectorStats connectorStats,
                                                                              IEnumerable<ConnectorState> connectorStates,
                                                                              IConnectorType connector,
                                                                              ConnectorTelemetryBucket item)
    {
        var connectorStatesList = connectorStates.ToList();

        return new ConnectorTelemetryBucketDto()
        {
            TotalTelemetryCount = item.TotalTelemetryCount,
            Timestamp = item.StartTimestamp,
            UniqueCapabilityCount = item.UniqueCapabilityCount,
            ExpectedTelemetryCount = item.ExpectedTelemetryCount,
            SetState = connector.GetSetState(connectorStatesList, item).ToString(),
            Status = connector.GetStatus(connectorStats, connectorStatesList, item).ToString(),
        };
    }

    private static void UpdateExpectedTelemetryCounts(ConnectorStats connectorStats,
                                                      IEnumerable<ConnectorTelemetryBucket> telemetry,
                                                      IReadOnlyCollection<ConnectorState> connectorStates)
    {
        foreach (var item in telemetry)
        {
            if (connectorStates is null || connectorStates.Count == 0)
            {
                item.ExpectedTelemetryCount = 0;
                continue;
            }

            var connectorStateList = connectorStates.Where(x => x.TimestampUtc < item.EndTimestamp &&
                                                                x.TimestampUtc >= item.StartTimestamp).ToList();

            // Always add the latest entry prior to start of interval if available
            // If there is a change in interval within the bin,
            // then the previous interval will be applicable till that point
            var previousEntry = connectorStates.Where(x => x.TimestampUtc < item.StartTimestamp)
                                               .MaxBy(x => x.TimestampUtc);
            if (previousEntry != null)
            {
                connectorStateList.Add(previousEntry);
            }

            if (connectorStateList.Count == 0)
            {
                item.ExpectedTelemetryCount = 0;
                continue;
            }

            item.ExpectedTelemetryCount = HandleConnectorStateChanges(connectorStats, item, connectorStateList);
        }
    }

    private static int HandleConnectorStateChanges(ConnectorStats connectorStats,
                                                   ConnectorTelemetryBucket item,
                                                   IEnumerable<ConnectorState> connectorState)
    {
        var binSize = (int)(item.EndTimestamp - item.StartTimestamp).TotalSeconds;
        var totalExpectedTelemetryCount = 0;
        var segmentStartTime = item.StartTimestamp;
        var connectorStateList = connectorState.OrderBy(x => x.TimestampUtc).ToList();
        var totalSegments = connectorStateList.Count;

        // For state changes within a bin, break down the bin into segments for each entry
        // Compute expected counted for each segment and sum them up to get overall expected count for the bin
        for (var segment = 0; segment < totalSegments; segment++)
        {
            var segmentInterval = connectorStateList[segment].Interval;

            // If interval is not specified or 0, use binSize as the interval so that we don't end up with NaN results
            // This will also make sure that connectors such as Stream Analytics Integrations have valid Telemetry per bin
            if (segmentInterval is null or 0)
            {
                segmentInterval = binSize;
            }

            // If it's the last (or only) segment set segment end to be same as the Telemetry bucket item
            // Otherwise, use the next segment start as current segment's end.
            var segmentEndTime = segment == totalSegments - 1 ? item.EndTimestamp : connectorStateList[segment + 1].TimestampUtc;
            var segmentBinSize = (int)(segmentEndTime - segmentStartTime).TotalSeconds;

            segmentStartTime = segmentEndTime;

            // We can't skip the segment before updating the start time for the next segment
            if (!connectorStateList[segment].IsEnabled)
            {
                continue;
            }

            var expectedTelemetryCount = connectorStats.EnabledCapabilitiesCount * segmentBinSize / segmentInterval;
            totalExpectedTelemetryCount += (int)expectedTelemetryCount;
        }

        return totalExpectedTelemetryCount;
    }
}
