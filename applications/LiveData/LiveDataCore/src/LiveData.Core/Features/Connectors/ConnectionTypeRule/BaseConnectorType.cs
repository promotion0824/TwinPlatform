namespace Willow.LiveData.Core.Features.Connectors.ConnectionTypeRule;

using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using Willow.LiveData.Core.Features.Connectors.Models;
using Willow.LiveData.Core.Infrastructure.Configuration;
using Willow.LiveData.Core.Infrastructure.Enumerations;
using Willow.LiveData.Core.Infrastructure.Extensions;

internal abstract class BaseConnectorType : IConnectorType
{
    private const int Percent = 100;
    private readonly decimal thresholdPercentage;

    protected BaseConnectorType(IOptions<TelemetryConfiguration> telemetryOptions)
    {
        thresholdPercentage = telemetryOptions.Value.ThresholdPercentage / Percent;
    }

    /// <inheritdoc/>
    public ConnectionSetStatus GetSetState(IEnumerable<ConnectorState> connectorStates, ConnectorTelemetryBucket connectorTelemetryBucket = null)
    {
        var connectorStatesList = connectorStates.ToList();

        // If no bin provided, provide latest SetState
        // If bins are provided, provide latest SetState within the bin
        // If bins are provided but no SetState information available, provide the earliest SetState (this is for time intervals prior to having the ConnectorState table
        var latestState = connectorTelemetryBucket is null
                              ? connectorStatesList.MaxBy(x => x.TimestampUtc)
                              : connectorStatesList.Where(x => x.TimestampUtc < connectorTelemetryBucket.EndTimestamp).MaxBy(x => x.TimestampUtc) ?? connectorStatesList.MinBy(x => x.TimestampUtc);

        if (RevertIfUnknown(latestState))
        {
            return ConnectionSetStatus.UNKNOWN;
        }

        if (latestState!.IsArchived)
        {
            return ConnectionSetStatus.ARCHIVED;
        }

        return !latestState.IsEnabled ? ConnectionSetStatus.DISABLED : ConnectionSetStatus.ENABLED;
    }

    /// <inheritdoc/>
    public virtual ConnectionState GetStatus(ConnectorStats connectorStats, IEnumerable<ConnectorState> connectorStates, ConnectorTelemetryBucket connectorTelemetryBucket)
    {
        var connectorStatesList = connectorStates.ToList();
        var latestState = connectorStatesList.AsQueryable().WhereIf(connectorTelemetryBucket != null, x => x.TimestampUtc < connectorTelemetryBucket.EndTimestamp)
                                             .OrderByDescending(x => x.TimestampUtc).FirstOrDefault() ?? connectorStatesList.MinBy(x => x.TimestampUtc);

        if (RevertIfUnknown(latestState))
        {
            return ConnectionState.UNKNOWN;
        }

        if (latestState!.IsArchived)
        {
            return ConnectionState.ARCHIVED;
        }

        return latestState.IsEnabled switch
        {
            false => ConnectionState.DISABLED,
            true when connectorStats?.EnabledCapabilitiesCount == 0 => ConnectionState.READY,
            true when connectorTelemetryBucket is null => ConnectionState.OFFLINE,
            true when connectorTelemetryBucket.TotalTelemetryCount == 0 => ConnectionState.OFFLINE,
            true when connectorTelemetryBucket.TotalTelemetryCount < connectorTelemetryBucket.ExpectedTelemetryCount * thresholdPercentage => ConnectionState.DISRUPTED,
            true when connectorTelemetryBucket.TotalTelemetryCount > 0 => ConnectionState.ONLINE,
            _ => ConnectionState.UNKNOWN,
        };
    }

    /// <inheritdoc/>
    public virtual ConnectionState GetStatus(ConnectorStats connectorStats,
                                             IEnumerable<ConnectorState> connectorStates,
                                             ConnectorTelemetryBucket connectorTelemetryBucket,
                                             IEnumerable<HeartbeatTelemetry> heartbeatTelemetry)
    {
        return GetStatus(connectorStats, connectorStates, connectorTelemetryBucket);
    }

    private static bool RevertIfUnknown(ConnectorState connectorState) => connectorState == null || (connectorState.ConnectorId == System.Guid.Empty);
}
