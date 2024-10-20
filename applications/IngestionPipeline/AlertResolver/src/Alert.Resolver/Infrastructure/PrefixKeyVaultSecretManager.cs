using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Security.KeyVault.Secrets;

namespace Willow.Alert.Resolver.Infrastructure;

internal sealed class PrefixKeyVaultSecretManager : KeyVaultSecretManager
{
    private const string CommonPrefix = "WillowCommon--";
    private readonly string _prefix;

    public PrefixKeyVaultSecretManager(string prefix)
    {
        _prefix = $"{prefix}--";
    }

    public override bool Load(SecretProperties secret)
    {
        return secret.Name.StartsWith(_prefix) || secret.Name.StartsWith(CommonPrefix);
    }

    public override string GetKey(KeyVaultSecret secret)
    {
        return secret.Name.StartsWith(_prefix)
                   ? secret.Name[_prefix.Length..]
                   : secret.Name[CommonPrefix.Length..]
                           .Replace("--", ConfigurationPath.KeyDelimiter);
    }
}
