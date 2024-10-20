using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Willow.IoTService.Monitoring.Dtos.LiveDataCore;
using Willow.IoTService.Monitoring.Services.Core;

namespace Willow.IoTService.Monitoring.Services.LiveDataCore;

public class ConnectorStatsQueries : IConnectorStatsQueries
{
    private readonly ILiveDataCoreApiService _liveDataCoreApiService;

    public ConnectorStatsQueries(ILiveDataCoreApiService liveDataCoreApiService)
    {
        _liveDataCoreApiService = liveDataCoreApiService;
    }

    public Task<ConnectorStatsDto?> ConnectorStats(Guid? customerId, Guid connectorId, DateTime start, DateTime end)
    {
        return _liveDataCoreApiService.GetConnectorStats(customerId, connectorId, start, end);
    }

    /// <summary>
    /// Get the Expected and Actual Trending Capabilities
    /// </summary>
    /// <param name="customerId"></param>
    /// <param name="connectorIds"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public Task<UniqueTrendsResult?> UniqueTrends(
        Guid? customerId,        // The customer owning the capabilities
        IList<Guid>? connectorIds,      // The connectors to query
        DateTime start,         // The begin of the time range
        DateTime end)       // The finish of the time range
    {
        return _liveDataCoreApiService.GetUniqueTrends(customerId, connectorIds, start, end);
    }

    public Task<IReadOnlyDictionary<string, string>> MissingTrends(
        Guid? customerId,        // The customer owning the capabilities
        IList<Guid>? connectorIds,      // The connectors to query
        DateTime start,         // The begin of the time range
        DateTime end)       // The finish of the time range
    {
        return _liveDataCoreApiService.GetMissingTrends(customerId, connectorIds, start, end);
    }
}
