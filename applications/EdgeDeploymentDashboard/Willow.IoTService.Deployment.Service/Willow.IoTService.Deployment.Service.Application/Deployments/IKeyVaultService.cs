namespace Willow.IoTService.Deployment.Service.Application.Deployments;

/// <summary>
/// Represents a service for interacting with Azure Key Vault.
/// </summary>
public interface IKeyVaultService
{
    /// <summary>
    /// Retrieves a secret from Azure Key Vault.
    /// </summary>
    /// <param name="secretName">The name of the secret to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation. The result of the task is the retrieved secret value as a string.</returns>
    Task<string> GetSecretFromKeyVault(
        string secretName,
        CancellationToken cancellationToken);
}
