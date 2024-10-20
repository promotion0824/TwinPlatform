namespace Willow.IoTService.Deployment.Service.Application.Deployments;

using Microsoft.Azure.Devices;
using Willow.IoTService.Deployment.Common.Messages;

/// <summary>
///     Default transformer for module deployment template.
/// </summary>
public interface IDefaultTransformer
{
    /// <summary>
    ///     Transforms the module deployment template.
    /// </summary>
    /// <param name="content">Deployment configuration template to be transformed.</param>
    /// <param name="configs">Dictionary of container configurations included in the template.</param>
    void Transform(ConfigurationContent content, IReadOnlyDictionary<string, IContainerConfiguration>? configs = null);
}
