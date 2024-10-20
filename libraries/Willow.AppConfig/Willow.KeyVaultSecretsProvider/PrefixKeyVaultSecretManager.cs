namespace Willow.KeyVaultSecretsProvider;

using System;
using System.Collections.Generic;
using System.Linq;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;

/// <summary>
/// A custom <see cref="KeyVaultSecretManager"/> that allows for a prefix to be added to the secret names.
/// </summary>
public class PrefixKeyVaultSecretManager : KeyVaultSecretManager
{
    private readonly List<string> prefixes = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="PrefixKeyVaultSecretManager"/> class.
    /// </summary>
    /// <param name="prefix">The prefix to add to the secret names.</param>
    public PrefixKeyVaultSecretManager(string prefix)
    {
        prefixes.Add($"{prefix}--");
        prefixes.Add("WillowCommon--");
    }

    /// <summary>
    /// Determines if the secret should be loaded.
    /// </summary>
    /// <param name="secret">The secret properties.</param>
    /// <returns>True if there are secrets that start with the prefix. False otherwise.</returns>
    public override bool Load(SecretProperties secret)
    {
        return prefixes.Any(prefix => secret.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets the key for the secret.
    /// </summary>
    /// <param name="secret">The keyvault secret.</param>
    /// <returns>The key for the secret.</returns>
    public override string GetKey(KeyVaultSecret secret)
    {
        var matchingPrefix = prefixes.First(prefix => secret.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
        return secret.Name[matchingPrefix.Length..]
            .Replace("--", ConfigurationPath.KeyDelimiter);
    }
}
