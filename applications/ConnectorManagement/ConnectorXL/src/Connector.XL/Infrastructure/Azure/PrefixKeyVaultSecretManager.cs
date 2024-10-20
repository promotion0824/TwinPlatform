namespace Connector.XL.Infrastructure.Azure;

using global::Azure.Extensions.AspNetCore.Configuration.Secrets;
using global::Azure.Security.KeyVault.Secrets;

internal class PrefixKeyVaultSecretManager : KeyVaultSecretManager
{
    private readonly List<string> prefixes = [];

    public PrefixKeyVaultSecretManager(string prefix)
    {
        prefixes.Add($"{prefix}--");
        prefixes.Add("WillowCommon--");
        prefixes.Add("Common--");
    }

    public override bool Load(SecretProperties secret)
    {
        return prefixes.Any(prefix => secret.Name.StartsWith(prefix));
    }

    public override string GetKey(KeyVaultSecret secret)
    {
        var matchingPrefix = prefixes.First(prefix => secret.Name.StartsWith(prefix));
        return secret.Name[matchingPrefix.Length..].Replace("--", ConfigurationPath.KeyDelimiter);
    }
}
