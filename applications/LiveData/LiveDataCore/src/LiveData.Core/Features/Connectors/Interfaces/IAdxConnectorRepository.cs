namespace Willow.LiveData.Core.Features.Connectors.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Willow.LiveData.Core.Features.Connectors.DTOs;
using Willow.LiveData.Core.Features.Connectors.Models;

internal interface IAdxConnectorRepository
{
    Task<IEnumerable<ConnectorStats>> GetConnectorStatusAsync(Guid? clientId, List<Guid> connectorIds);

    Task<IEnumerable<ConnectorTelemetryBucket>> GetTelemetryCountByLastXHoursAsync(Guid? clientId, List<Guid> connectorIds, DateTime start, DateTime end, bool singleBin = false);

    Task<IEnumerable<ConnectorState>> GetConnectorStateOvertimeAsync(Guid? clientId, List<Guid> connectorIds, DateTime start, DateTime end);

    Task<List<UniqueTrends>> GetTelemetryAndCapabilityCountByLastXHoursAsync(Guid? clientId, List<Guid> validGuids, DateTime start, DateTime end);

    Task<List<MissingTrendsDetail>> GetMissingTrendsForXHoursAsync(Guid? clientId, List<Guid> validGuids, DateTime start, DateTime end);
}
