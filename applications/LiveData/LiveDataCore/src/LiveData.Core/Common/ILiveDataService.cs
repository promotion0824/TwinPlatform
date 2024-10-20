namespace Willow.LiveData.Core.Common;

using System;
using Microsoft.Extensions.Configuration;
using Willow.Infrastructure.Exceptions;
using Willow.LiveData.Core.Features.GetLiveData.GetPointLiveData;

//TODO: This interface/class can be refactored once multi-tenant support is not required.

/// <summary>
/// Represents a service for retrieving live data.
/// </summary>
public interface ILiveDataService
{
    /// <summary>
    /// Gets the live data service for a specific client.
    /// </summary>
    /// <param name="clientId">The unique identifier for the client.</param>
    /// <returns>The live data service.</returns>
    IAdxLiveDataService GetLiveDataService(Guid? clientId);
}

/// <summary>
/// Represents a service for retrieving live data.
/// </summary>
internal class LiveDataService(
    IConfiguration config,
    IAdxLiveDataService adxLiveDataService)
    : ILiveDataService
{
    public IAdxLiveDataService GetLiveDataService(Guid? clientId)
    {
        this.CheckAdxConfiguration(clientId);
        return adxLiveDataService;
    }

    /// <summary>
    /// Checks and retrieves the ADX configuration for a specific client.
    /// </summary>
    /// <param name="clientId">The unique identifier for the client.</param>
    private void CheckAdxConfiguration(Guid? clientId)
    {
        var adxDbName = config["ADX:Database"];
        var adxClusterUri = config["ADX:ClusterUri"];

        if (clientId is not null && !string.IsNullOrEmpty(config[$"{clientId}:ADX:Database"]))
        {
            adxDbName = config[$"{clientId}:ADX:Database"];
        }

        if (clientId is not null && !string.IsNullOrEmpty(config[$"{clientId}:ADX:ClusterUri"]))
        {
            adxClusterUri = config[$"{clientId}:ADX:ClusterUri"];
        }

        if (string.IsNullOrEmpty(adxDbName))
        {
            throw new BadRequestException($"ADX DB name is not configured for the client {clientId}");
        }

        if (string.IsNullOrEmpty(adxClusterUri))
        {
            throw new BadRequestException($"ADX Cluster URI is not configured for the client {clientId}");
        }
    }
}
