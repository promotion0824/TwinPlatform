namespace Willow.IoTService.Deployment.Service.Application.Deployments;

/// <summary>
///     Service for getting the edge connector environment variables.
/// </summary>
public interface IEdgeConnectorEnvService
{
    /// <summary>
    ///     Gets the edge connector environment variables.
    /// </summary>
    /// <param name="connectorId">ConnectorId for the deployment.</param>
    /// <param name="moduleName">Module name for the deployment.</param>
    /// <param name="environment">Environment variable for the deployment.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of environment variables after substitution / transformation.</returns>
    Task<IEnumerable<string>> GetEdgeConnectorEnvs(
        string connectorId,
        string moduleName,
        string? environment,
        CancellationToken cancellationToken);
}
