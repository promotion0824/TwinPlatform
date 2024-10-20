namespace Willow.IoTService.Deployment.Service.Application.Deployments;

using Microsoft.Azure.Devices;

/// <summary>
///     Transforms the base module deployment template.
/// </summary>
public interface IBaseModuleTransformer
{
    /// <summary>
    ///     Transforms the base module deployment template.
    /// </summary>
    /// <param name="content">Template for transformation.</param>
    void Transform(ConfigurationContent content);
}
