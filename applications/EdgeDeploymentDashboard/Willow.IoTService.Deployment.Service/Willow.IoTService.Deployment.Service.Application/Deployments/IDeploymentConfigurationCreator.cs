namespace Willow.IoTService.Deployment.Service.Application.Deployments;

using Microsoft.Azure.Devices;

/// <summary>
///     Creates a deployment configuration.
/// </summary>
public interface IDeploymentConfigurationCreator
{
    /// <summary>
    ///     Creates a deployment configuration.
    /// </summary>
    /// <param name="deploymentId">ID of the deployment.</param>
    /// <param name="content">Deployment configuration contents.</param>
    /// <param name="isBaseDeployment">Denotes if a deployment is for base modules or custom modules.</param>
    /// <param name="moduleType">Type of module to be deployed.</param>
    /// <param name="deviceName">IoT Hub Edge device name.</param>
    /// <param name="moduleName">Deployment module name.</param>
    /// <returns>An IoT Hub Deployment configuration.</returns>
    DeploymentConfiguration Create(
        Guid deploymentId,
        ConfigurationContent content,
        bool isBaseDeployment,
        string moduleType,
        string deviceName,
        string moduleName);
}
