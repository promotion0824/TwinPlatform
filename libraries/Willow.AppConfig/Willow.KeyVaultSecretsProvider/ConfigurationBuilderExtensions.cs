namespace Willow.KeyVaultSecretsProvider;

using System;
using Azure.Identity;
using Microsoft.Extensions.Configuration;

/// <summary>
/// Extension methods for <see cref="IConfigurationBuilder"/> to add Azure Key Vault as a configuration source.
/// </summary>
public static class ConfigurationBuilderExtensions
{
    /// <summary>
    /// Adds Azure Key Vault as a configuration source.
    /// </summary>
    /// <param name="configurationBuilder">The configuration builder instance.</param>
    /// <param name="keyVaultName">The keyvault name.</param>
    /// <param name="assemblyName">The name of the assembly to prefix the secret names with.</param>
    /// <returns>The updated configuration builder.</returns>
    public static IConfigurationBuilder AddKeyVaultProvider(this IConfigurationBuilder configurationBuilder, string keyVaultName, string assemblyName)
    {
        if (string.IsNullOrWhiteSpace(keyVaultName))
        {
            return configurationBuilder;
        }

        var keyVaultConfigBuilder = new ConfigurationBuilder();
        var prefix = assemblyName.Replace(".", string.Empty);
        keyVaultConfigBuilder.AddAzureKeyVault(new Uri($"https://{keyVaultName}.vault.azure.net"), new DefaultAzureCredential(), new PrefixKeyVaultSecretManager(prefix));

        return configurationBuilder.AddConfiguration(keyVaultConfigBuilder.Build());
    }
}
