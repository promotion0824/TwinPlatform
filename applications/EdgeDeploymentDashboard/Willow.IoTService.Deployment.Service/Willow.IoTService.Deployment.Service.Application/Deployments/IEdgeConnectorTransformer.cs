namespace Willow.IoTService.Deployment.Service.Application.Deployments;

/// <summary>
///     Transforms the edge connector manifest.
/// </summary>
public interface IEdgeConnectorTransformer
{
    /// <summary>
    ///     Determines if the module type can be transformed.
    /// </summary>
    /// <param name="moduleType">Type of the module to be deployed.</param>
    /// <returns>Boolean indicating if the module type transformation is supported or not.</returns>
    /// <remarks>Currently supported only for module types containing casbacnetrpc|chipkinbacnet|bacnet|modbus|opcua.</remarks>
    bool CanTransform(string moduleType);

    /// <summary>
    /// Transforms the edge connector manifest with variable substitutions.
    /// </summary>
    /// <param name="config">Connector manifest.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task TransformAsync(EdgeConnectorTransformConfig config, CancellationToken cancellationToken = default);
}
