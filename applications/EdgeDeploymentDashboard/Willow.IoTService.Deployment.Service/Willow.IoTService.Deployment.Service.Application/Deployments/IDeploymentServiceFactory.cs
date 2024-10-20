namespace Willow.IoTService.Deployment.Service.Application.Deployments;

/// <summary>
///     Factory for creating <see cref="IDeploymentService" /> instances.
/// </summary>
public interface IDeploymentServiceFactory
{
    /// <summary>
    ///     Creates an <see cref="IDeploymentService" /> instance.
    /// </summary>
    /// <param name="iotHubName">IoT Hub name for deployment.</param>
    /// <returns>Interface for <see cref="IDeploymentService" />.</returns>
    public IDeploymentService Create(string iotHubName);
}
