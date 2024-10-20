using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Willow.IoTService.Monitoring.Dtos.LiveDataCore;

namespace Willow.IoTService.Monitoring.Services.LiveDataCore;

public interface IConnectorStatsQueries
{
    Task<ConnectorStatsDto?> ConnectorStats(Guid? customerId, Guid connectorId, DateTime start, DateTime end);

    Task<UniqueTrendsResult?> UniqueTrends(Guid? customerId, IList<Guid>? connectorIds, DateTime start, DateTime end);
    Task<IReadOnlyDictionary<string, string>> MissingTrends(Guid? customerId, IList<Guid>? connectorIds, DateTime start, DateTime end);
}
