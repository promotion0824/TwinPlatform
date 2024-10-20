namespace Willow.IoTService.Deployment.DbSync.Application.Infrastructure;

using ConnectorCore.Contracts;
using Willow.IoTService.Deployment.DataAccess.Services;

/// <summary>
///     Service for updating the modules and module configurations.
/// </summary>
public interface IUpdateModuleService
{
    /// <summary>
    ///     Task for updating the Connector Module in DeploymentDashboardDb.
    /// </summary>
    /// <param name="connectorConfig">Connector Configuration.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>A <see cref="Task" /> for updating the module.</returns>
    Task<ModuleDto> UpdateModuleAsync(IConnectorMessage connectorConfig, CancellationToken cancellationToken);

    /// <summary>
    ///     Task for updating the Connector Module configuration in DeploymentDashboardDb.
    /// </summary>
    /// <param name="connectorConfig">Connector Configuration.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>A <see cref="Task" /> for updating the module config.</returns>
    Task<ModuleDto> UpdateModuleConfigAsync(IConnectorMessage connectorConfig, CancellationToken cancellationToken);
}
