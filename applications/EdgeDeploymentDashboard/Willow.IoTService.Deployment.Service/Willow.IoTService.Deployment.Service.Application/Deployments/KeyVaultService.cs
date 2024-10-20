namespace Willow.IoTService.Deployment.Service.Application.Deployments;

using Azure;
using Azure.Core;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;

/// <inheritdoc />
public class KeyVaultService(
    IConfiguration configuration,
    TokenCredential tokenCredential)
    : IKeyVaultService
{
    private const string Prefix = "edge-deployment-service--";

    /// <inheritdoc />
    public async Task<string> GetSecretFromKeyVault(string secretName, CancellationToken cancellationToken)
    {
        var keyVaultUrlString = configuration.GetValue<string>("AppSecretsUrl");
        var keyVaultUrl = new Uri(keyVaultUrlString!);
        var keyVaultClient = new SecretClient(keyVaultUrl, tokenCredential);
        var secretKey = $"{Prefix}{secretName}";
        try
        {
            var secret = await keyVaultClient.GetSecretAsync(secretKey, cancellationToken: cancellationToken);
            return secret.Value.Value;
        }
        catch (RequestFailedException e)
        {
            throw new SecretNotFoundException(secretKey, keyVaultUrlString!, e);
        }
    }
}
