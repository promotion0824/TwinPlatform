namespace Willow.LiveData.Core.Features.Connectors.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Willow.LiveData.Core.Features.Connectors.DTOs;
using Willow.LiveData.Core.Features.Connectors.Helpers;
using Willow.LiveData.Core.Features.Connectors.Interfaces;
using Willow.LiveData.Core.Features.Connectors.Models;
using Willow.LiveData.Core.Infrastructure.Extensions;

internal class ConnectorService : IConnectorService
{
    private readonly IConnectorStatsResultHelper connectorStatsResultHelper;
    private readonly IAdxConnectorRepository connectorRepository;
    private const int DefaultInterval = 1;

    public ConnectorService(IConnectorStatsResultHelper connectorStatsResultHelper,
                            IAdxConnectorRepository connectorRepository)
    {
        this.connectorStatsResultHelper = connectorStatsResultHelper;
        this.connectorRepository = connectorRepository;
    }

    /// <inheritdoc/>
    public async Task<ConnectorStatusResult> GetConnectorStatusAsync(
        Guid? clientId,
        ConnectorList connectorStatusRequest,
        DateTime? start,
        DateTime? end,
        string singleBin)
    {
        var singleBinning = bool.TryParse(singleBin, out var parsedSingleBin) && parsedSingleBin;
        var (startTime, endTime, validGuids, invalidGuids, connectorStateList) = await GetRequestParameters(start, end, clientId, connectorStatusRequest);
        var connectorStatusResult = Enumerable.Empty<ConnectorStats>();
        var capabilitiesCountResult = Enumerable.Empty<ConnectorTelemetryBucket>();
        var currentCapabilitiesCountResult = Enumerable.Empty<ConnectorTelemetryBucket>();

        if (validGuids.Count > 0)
        {
            connectorStatusResult = await connectorRepository.GetConnectorStatusAsync(clientId, validGuids);

            capabilitiesCountResult = await connectorRepository.GetTelemetryCountByLastXHoursAsync(clientId,
                                                                                                     validGuids,
                                                                                                     startTime,
                                                                                                     endTime,
                                                                                                     singleBinning);
            currentCapabilitiesCountResult = await connectorRepository.GetTelemetryCountByLastXHoursAsync(clientId,
                                                                                                       validGuids,
                                                                                                       singleBinning ? startTime : DateTime.UtcNow - TimeSpan.FromHours(1),
                                                                                                       singleBinning ? endTime : DateTime.UtcNow,
                                                                                                       singleBinning);
        }

        ErrorData errList = null;
        if (invalidGuids != null && invalidGuids.Count > 0)
        {
            invalidGuids.RemoveAll(x => x.Length == 0);
            errList = new ErrorData
            {
                Ids = invalidGuids,
                Message = connectorStatusRequest?.ConnectorIds.GetErrorMessageOnInvalidGuids("Connector Ids"),
            };
        }

        return new ConnectorStatusResult
        {
            Data = connectorStatsResultHelper.UpdateConnectorStatsDto(connectorStatusResult, capabilitiesCountResult, currentCapabilitiesCountResult, connectorStateList),
            ErrorList = errList,
        };
    }

    /// <inheritdoc/>
    public async Task<UniqueTrendsResult> GetUniqueTrendsAsync(
        Guid? clientId,
        ConnectorList connectorStatusRequest,
        DateTime? start,
        DateTime? end)
    {
        var (startTime, endTime, validGuids, invalidGuids, connectorStateList) = await GetRequestParameters(start, end, clientId, connectorStatusRequest);

        var result = new UniqueTrendsResult();

        if (invalidGuids != null && invalidGuids.Count > 0)
        {
            invalidGuids.RemoveAll(x => x.Length == 0);
            result.ErrorList = new ErrorData
            {
                Ids = invalidGuids,
                Message = connectorStatusRequest?.ConnectorIds.GetErrorMessageOnInvalidGuids("Connector Ids"),
            };
        }

        if (validGuids.Count > 0)
        {
            result.Data = await connectorRepository.GetTelemetryAndCapabilityCountByLastXHoursAsync(clientId,
                                                                                           validGuids,
                                                                                           startTime,
                                                                                           endTime);
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<MissingTrendsResult> GetMissingTrendsAsync(Guid? clientId, ConnectorList connectorStatusRequest, DateTime? start, DateTime? end)
    {
        var (startTime, endTime, validGuids, invalidGuids, connectorStateList) = await GetRequestParameters(start, end, clientId, connectorStatusRequest);

        var result = new MissingTrendsResult();

        if (invalidGuids != null && invalidGuids.Count > 0)
        {
            invalidGuids.RemoveAll(x => x.Length == 0);
            result.ErrorList = new ErrorData
            {
                Ids = invalidGuids,
                Message = connectorStatusRequest?.ConnectorIds.GetErrorMessageOnInvalidGuids("Connector Ids"),
            };
        }

        if (validGuids.Count > 0)
        {
            result.Data = await connectorRepository.GetMissingTrendsForXHoursAsync(clientId,
                                                                                           validGuids,
                                                                                           startTime,
                                                                                           endTime);
        }

        return result;
    }

    private async Task<(DateTime StartTime, DateTime EndTime, List<Guid> BalidGuids, List<string> InvalidGuids, List<ConnectorState> ConnectorStateList)> GetRequestParameters(
        DateTime? start,
        DateTime? end,
        Guid? clientId,
        ConnectorList connectorStatusRequest)
    {
        // If date range is not provided, return default interval of last hour
        var startTime = start ?? DateTime.UtcNow - TimeSpan.FromHours(DefaultInterval);
        var endTime = end ?? DateTime.UtcNow;

        var validGuids = connectorStatusRequest?.ConnectorIds.GetValidGuids();

        var connectorStateOverTime = await connectorRepository.GetConnectorStateOvertimeAsync(
                                                                                               clientId,
                                                                                               validGuids,
                                                                                               startTime,
                                                                                               endTime);

        // Only process those Guids for which we have entries in ConnectorState table
        // If no entry is present, that means it doesn't belong to this client
        var connectorStateList = connectorStateOverTime.ToList();
        validGuids = connectorStateList.Select(connectorState => connectorState.ConnectorId).Distinct().ToList();
        var invalidGuids = connectorStatusRequest?.ConnectorIds.GetInvalidGuids();

        return (startTime, endTime, validGuids, invalidGuids, connectorStateList);
    }
}
