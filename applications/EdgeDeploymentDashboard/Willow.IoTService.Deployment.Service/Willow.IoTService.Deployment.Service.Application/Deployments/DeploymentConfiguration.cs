namespace Willow.IoTService.Deployment.Service.Application.Deployments;

using Microsoft.Azure.Devices;

/// <summary>
///     IoT Hub deployment configuration.
/// </summary>
/// <param name="Configuration">IoT Hub Configuration to be deployed.</param>
/// <param name="DeviceId">IoT Hub Edge Device Id.</param>
public record DeploymentConfiguration(Configuration Configuration, string DeviceId);
