namespace Willow.LiveData.IoTHubAdaptor.Infrastructure.KeyVault;

using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Security.KeyVault.Secrets;

internal class PrefixKeyVaultSecretManager : KeyVaultSecretManager
{
    private const string CommonPrefix = "Common--";
    private readonly string prefix;

    public PrefixKeyVaultSecretManager(string prefix)
    {
        this.prefix = $"{prefix}--";
    }

    public override bool Load(SecretProperties secret)
    {
        ArgumentNullException.ThrowIfNull(secret);

        // Load a vault secret when its secret name starts with the
        // common prefix or application prefix. Other secrets won't be loaded.
        return secret.Name.StartsWith(CommonPrefix) || secret.Name.StartsWith(prefix);
    }

    public override string GetKey(KeyVaultSecret secret)
    {
        ArgumentNullException.ThrowIfNull(secret);

        // Remove the prefix from the secret name and replace two
        // dashes in any name with the KeyDelimiter, which is the
        // delimiter used in configuration (usually a colon). Azure
        // Key Vault doesn't allow a colon in secret names.
        return (secret.Name.StartsWith(CommonPrefix) ?
                    secret.Name[CommonPrefix.Length..]
                    : secret.Name[prefix.Length..])
           .Replace("--", ConfigurationPath.KeyDelimiter);
    }
}
