namespace Willow.IoTService.Deployment.Service.Application.Deployments;

/// <summary>
/// Service Bus Admin Service.
/// </summary>
public interface IServiceBusAdminService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceBusAdminService"/> class.
    /// </summary>
    /// <param name="connectorId">ConnectorId.</param>
    /// <returns>cnc-commands topic listen connection config.</returns>
    Task<ServiceBusConnectionConfig?> GetOrCreateServiceBusConnectionConfigAsync(string connectorId);
}
