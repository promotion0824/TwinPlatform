namespace Willow.LiveData.Core.Infrastructure.Azure.KeyVault;

using System;
using System.Collections.Generic;
using System.Linq;
using global::Azure.Extensions.AspNetCore.Configuration.Secrets;
using global::Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;

internal class PrefixKeyVaultSecretManager : KeyVaultSecretManager
{
    private readonly List<string> prefixes = new();

    public PrefixKeyVaultSecretManager(string prefix)
    {
        prefixes.Add($"{prefix}--");
        prefixes.Add("WillowCommon--");
        prefixes.Add("Common--");
    }

    /// <inheritdoc/>
    public override bool Load(SecretProperties secret)
    {
        ArgumentNullException.ThrowIfNull(secret);

        // Load a vault secret when its secret name starts with any of the
        // common prefixes or application prefix. Other secrets won't be loaded.
        return prefixes.Any(prefix => secret.Name.StartsWith(prefix));
    }

    /// <inheritdoc/>
    public override string GetKey(KeyVaultSecret secret)
    {
        ArgumentNullException.ThrowIfNull(secret);

        // Remove the prefix from the secret name and replace two
        // dashes in any name with the KeyDelimiter, which is the
        // delimiter used in configuration (usually a colon). Azure
        // Key Vault doesn't allow a colon in secret names.
        var matchingPrefix = prefixes.First(prefix => secret.Name.StartsWith(prefix));
        return secret.Name[matchingPrefix.Length..]
                     .Replace("--", ConfigurationPath.KeyDelimiter);
    }
}
