namespace Willow.LiveData.Core.Features.Connectors.Interfaces;

using System;
using System.Threading.Tasks;
using Willow.LiveData.Core.Features.Connectors.DTOs;

/// <summary>
/// Connector Service.
/// </summary>
public interface IConnectorService
{
    /// <summary>
    /// Gets the connector status.
    /// </summary>
    /// <param name="clientId">The client ID.</param>
    /// <param name="connectorStatusRequest">A list of connector IDs.</param>
    /// <param name="start">The start time.</param>
    /// <param name="end">The end time.</param>
    /// <param name="singleBin">Single bin.</param>
    /// <returns>A connector status result.</returns>
    Task<ConnectorStatusResult> GetConnectorStatusAsync(Guid? clientId,
                                                       ConnectorList connectorStatusRequest,
                                                       DateTime? start,
                                                       DateTime? end,
                                                       string singleBin);

    /// <summary>
    /// Gets unique trends.
    /// </summary>
    /// <param name="clientId">The client ID.</param>
    /// <param name="connectorStatusRequest">A list of connector IDs.</param>
    /// <param name="start">The start time.</param>
    /// <param name="end">The end time.</param>
    /// <returns>A unique trends result.</returns>
    Task<UniqueTrendsResult> GetUniqueTrendsAsync(Guid? clientId,
                                                       ConnectorList connectorStatusRequest,
                                                       DateTime? start,
                                                       DateTime? end);

    /// <summary>
    /// Gets missing trends.
    /// </summary>
    /// <param name="clientId">The client ID.</param>
    /// <param name="connectorStatusRequest">A list of connector IDs.</param>
    /// <param name="start">The start time.</param>
    /// <param name="end">The end time.</param>
    /// <returns>A missing trends result.</returns>
    Task<MissingTrendsResult> GetMissingTrendsAsync(Guid? clientId,
                                                       ConnectorList connectorStatusRequest,
                                                       DateTime? start,
                                                       DateTime? end);
}
