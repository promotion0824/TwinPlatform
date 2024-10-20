namespace Willow.LiveData.Core.Features.Connectors.Helpers
{
    using System.Collections.Generic;
    using Willow.LiveData.Core.Features.Connectors.DTOs;
    using Willow.LiveData.Core.Features.Connectors.Models;

    internal interface IConnectorStatsResultHelper
    {
        ConnectorStatsDto MapTo(ConnectorStats connectorStats,
                                IEnumerable<ConnectorTelemetryBucket> telemetry,
                                IEnumerable<ConnectorTelemetryBucket> currentTelemetry,
                                IEnumerable<ConnectorState> connectorStates);

        IEnumerable<ConnectorStatsDto> UpdateConnectorStatsDto(IEnumerable<ConnectorStats> connectorStats,
                                                               IEnumerable<ConnectorTelemetryBucket> connectorTelemetryBuckets,
                                                               IEnumerable<ConnectorTelemetryBucket> currentTelemetryBuckets,
                                                               IEnumerable<ConnectorState> connectorStates);
    }
}
