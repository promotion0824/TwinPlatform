namespace Willow.IoTService.Deployment.Service.Application.Deployments;

using Microsoft.Azure.Devices;
using Willow.IoTService.Deployment.Common;

/// <inheritdoc />
public class DeploymentConfigurationCreator : IDeploymentConfigurationCreator
{
    private const int LayeredPriority = 12;
    private const int BasePriority = 4;

    /// <inheritdoc />
    public DeploymentConfiguration Create(
        Guid deploymentId,
        ConfigurationContent content,
        bool isBaseDeployment,
        string moduleType,
        string deviceName,
        string moduleName)
    {
        var priority = isBaseDeployment
                           ? BasePriority
                           : LayeredPriority;
        var deploymentName = BaseModuleDeploymentHelper.GetBaseDeploymentNameFromId(deploymentId);
        var config = new DeploymentConfiguration(
                                                 new Configuration(deploymentName)
                                                 {
                                                     Content = content,
                                                     Priority = priority,
                                                     TargetCondition = $"deviceId='{deviceName}'",
                                                     Labels = new Dictionary<string, string> { { "moduleType", moduleType }, { "moduleName", moduleName } },
                                                 },
                                                 deviceName);
        return config;
    }
}
