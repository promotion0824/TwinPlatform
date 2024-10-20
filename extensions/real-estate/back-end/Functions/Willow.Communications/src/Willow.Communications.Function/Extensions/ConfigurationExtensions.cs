using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using Azure.Identity;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Security.KeyVault.Secrets;
using System.Linq;

namespace Willow.Communications.Function.Extensions;
public static class ConfigurationExtensions
{
    public static string Get(this IConfiguration config, string name, ILogger logger, bool required = true)
    {
        string val = "";

        try
        {
            val = config[name];
        }
        catch
        {
            // Not in config
        }

        if (required && (string.IsNullOrWhiteSpace(val) || val.StartsWith("[value", StringComparison.InvariantCultureIgnoreCase)))
        {
            if (logger != null)
                logger.LogError($"Missing configuration entry: {name}");

            throw new Exception($"Missing configuration entry: {name}");
        }

        if (val == null || val.StartsWith("[value", StringComparison.InvariantCultureIgnoreCase))
            return null;

        return val;
    }

    public static IConfigurationBuilder AddPrefixedKeyVault(
        this IConfigurationBuilder config,
        string keyVaultName,
        string[] prefixes)
    {
        return config.AddAzureKeyVault(
            new Uri($"https://{keyVaultName}.vault.azure.net"),
            new DefaultAzureCredential(),
            new PrefixedKeyVaultSecretManager(prefixes));
    }

    public class PrefixedKeyVaultSecretManager : KeyVaultSecretManager
    {
        string[] _prefixes;

        /// <summary>
        /// Initializes a new instance of the <see cref="PrefixedKeyVaultSecretManager"/> class with the specified prefixes.
        /// </summary>
        /// <param name="prefixes">The prefixes to filter the Key Vault secrets.</param>
        public PrefixedKeyVaultSecretManager(string[] prefixes)
        {
            _prefixes = prefixes;
        }

        /// <summary>
        /// Determines if a secret should be loaded based on the specified prefixes.
        /// </summary>
        /// <param name="secret">The secret properties.</param>
        /// <returns><c>true</c> if the secret should be loaded; otherwise, <c>false</c>.</returns>
        public override bool Load(SecretProperties secret)
        {
            return _prefixes.Any(p => secret.Name.StartsWith(p + "--", StringComparison.InvariantCulture));
        }

        /// <summary>
        /// Extracts the key from the secret's name while replacing delimiters.
        /// </summary>
        /// <param name="secret">The Key Vault secret.</param>
        /// <returns>The extracted key.</returns>
        public override string GetKey(KeyVaultSecret secret)
        {
            var prefix = _prefixes.FirstOrDefault(p => secret.Name.StartsWith(p + "--", StringComparison.InvariantCulture));
            return secret.Name.Substring((prefix + "--").Length)
                .Replace("--", ConfigurationPath.KeyDelimiter, StringComparison.InvariantCulture);
        }
    }
}
