namespace Willow.IoTService.Deployment.Service.Application.Deployments;

using Microsoft.Azure.Devices;

/// <summary>
///     Service for getting the module configuration content.
/// </summary>
public interface IModuleConfigContentService
{
    /// <summary>
    ///     Gets the module configuration content.
    /// </summary>
    /// <param name="request">A <see cref="GetConfigContentRequest" /> request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The IoT Hub deployment configuration.</returns>
    Task<ConfigurationContent> GetContent(GetConfigContentRequest request, CancellationToken cancellationToken);
}
