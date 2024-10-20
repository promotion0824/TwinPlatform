namespace Willow.IoTService.Deployment.Service.Application.Deployments;

/// <summary>
///     Service for deploying configurations to devices.
/// </summary>
public interface IDeploymentService
{
    /// <summary>
    ///     Gets the Hostname of the IoT Hub.
    /// </summary>
    public string Hostname { get; }

    /// <summary>
    ///     Deploy a configuration to a device.
    /// </summary>
    /// <param name="configuration">IoT Hub deployment configuration.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>A <see cref="Task" /> for deploying a configuration.</returns>
    public Task<string> DeployAsync(DeploymentConfiguration configuration, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Removes historical deployments for a device.
    /// </summary>
    /// <param name="configurationId">Deployment configuration Id to be removed.</param>
    /// <param name="deploymentsLeft">Number of deployments to be left in IoTHub.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <remarks>Removes historical deployments older than number of deploymentsLeft so that IoT Hub limitation is not reached.</remarks>
    /// <returns>A <see cref="Task" /> for removing historical deployments.</returns>
    public Task RemoveHistoricalDeploymentsAsync(
        string configurationId,
        int deploymentsLeft = 2,
        CancellationToken cancellationToken = default);
}
