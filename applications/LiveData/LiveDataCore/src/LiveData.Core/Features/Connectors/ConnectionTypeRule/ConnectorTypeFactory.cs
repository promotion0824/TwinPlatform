namespace Willow.LiveData.Core.Features.Connectors.ConnectionTypeRule;

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Willow.LiveData.Core.Infrastructure.Enumerations;

internal class ConnectorTypeFactory : IConnectorTypeFactory
{
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<ConnectorTypeFactory> logger;

    public ConnectorTypeFactory(ILogger<ConnectorTypeFactory> logger, IServiceProvider serviceProvider)
    {
        this.logger = logger;
        this.serviceProvider = serviceProvider;
    }

    /// <inheritdoc/>
    public IConnectorType GetConnector(string connectionType)
    {
        var connectorTypeEnum = GetConnectionTypeEnum(connectionType);
        return connectorTypeEnum switch
        {
            ConnectionType.IoTEdge => serviceProvider.GetService<IIoTEdgeConnectorType>(),
            ConnectionType.VM => serviceProvider.GetService<IVmConnectorType>(),
            ConnectionType.StreamAnalyticsIoTHub => serviceProvider.GetService<IStreamingAnalyticsConnectorType>(),
            ConnectionType.StreamAnalyticsEventHub => serviceProvider.GetService<IStreamingAnalyticsConnectorType>(),
            ConnectionType.PublicAPI => serviceProvider.GetService<IPublicApiConnectorType>(),
            _ => serviceProvider.GetService<IVmConnectorType>(),
        };
    }

    private ConnectionType GetConnectionTypeEnum(string connectionType)
    {
        var connectorTypeEnum = ConnectionType.VM;
        try
        {
            if (!string.IsNullOrEmpty(connectionType))
            {
                connectorTypeEnum = Enum.Parse<ConnectionType>(connectionType.Replace(" ", string.Empty), true);
            }
        }
        catch (Exception ex) when (ex is ArgumentNullException or ArgumentException)
        {
            logger.LogWarning("Invalid ConnectionType: {Message}", ex.Message);
            connectorTypeEnum = ConnectionType.VM;
        }

        return connectorTypeEnum;
    }
}
