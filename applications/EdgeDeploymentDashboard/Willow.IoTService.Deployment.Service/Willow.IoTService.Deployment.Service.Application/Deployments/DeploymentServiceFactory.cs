namespace Willow.IoTService.Deployment.Service.Application.Deployments;

using Azure.Core;
using Microsoft.Extensions.Logging;

/// <inheritdoc />
public class DeploymentServiceFactory(ILoggerFactory loggerFactory, TokenCredential tokenCredential)
    : IDeploymentServiceFactory
{
    private readonly ILogger<DeploymentService> serviceLogger = loggerFactory.CreateLogger<DeploymentService>();

    /// <inheritdoc />
    public IDeploymentService Create(string iotHubName)
    {
        return new DeploymentService($"{iotHubName}.azure-devices.net", this.serviceLogger, tokenCredential);
    }
}
