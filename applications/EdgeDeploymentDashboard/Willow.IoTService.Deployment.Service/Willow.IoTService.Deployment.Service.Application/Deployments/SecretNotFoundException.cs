namespace Willow.IoTService.Deployment.Service.Application.Deployments;

/// <inheritdoc />
public class SecretNotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SecretNotFoundException"/> class
    /// that represents an exception that is thrown when a secret is not found in a key vault.
    /// </summary>
    /// <param name="secretName">Secret name.</param>
    /// <param name="keyVaultUrl">KeyVault url.</param>
    /// <param name="innerException">Exception message.</param>
    public SecretNotFoundException(
        string secretName,
        string keyVaultUrl,
        Exception? innerException)
        : base($"Could not get secret {secretName} from key vault {keyVaultUrl}", innerException)
    {
        this.SecretName = secretName;
        this.KeyVaultUrl = keyVaultUrl;
    }

    /// <summary>
    /// Gets secret Name.
    /// </summary>
    public string SecretName { get; }

    /// <summary>
    /// Gets KeyVault Url.
    /// </summary>
    public string KeyVaultUrl { get; }
}
