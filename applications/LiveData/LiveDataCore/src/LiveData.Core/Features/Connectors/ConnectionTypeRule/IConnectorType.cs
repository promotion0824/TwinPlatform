namespace Willow.LiveData.Core.Features.Connectors.ConnectionTypeRule
{
    using System.Collections.Generic;
    using Willow.LiveData.Core.Features.Connectors.Models;
    using Willow.LiveData.Core.Infrastructure.Enumerations;

    internal interface IConnectorType
    {
        ConnectionState GetStatus(ConnectorStats connectorStats,
            IEnumerable<ConnectorState> connectorStates,
            ConnectorTelemetryBucket connectorTelemetryBucket);

        ConnectionState GetStatus(ConnectorStats connectorStats,
            IEnumerable<ConnectorState> connectorStates,
            ConnectorTelemetryBucket connectorTelemetryBucket,
            IEnumerable<HeartbeatTelemetry> heartbeatTelemetry);

        ConnectionSetStatus GetSetState(IEnumerable<ConnectorState> connectorStates, ConnectorTelemetryBucket connectorTelemetryBucket = null);
    }
}
