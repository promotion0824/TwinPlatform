namespace Willow.IoTService.Deployment.Service.Worker.Infrastructure;

using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Security.KeyVault.Secrets;

internal class PrefixKeyVaultSecretManager : KeyVaultSecretManager
{
    private const string Prefix = "edge-deployment-service--";

    public override bool Load(SecretProperties secret)
    {
        return secret.Name.StartsWith(Prefix);
    }

    public override string GetKey(KeyVaultSecret secret)
    {
        return secret.Name[Prefix.Length..].Replace("--", ConfigurationPath.KeyDelimiter);
    }
}
