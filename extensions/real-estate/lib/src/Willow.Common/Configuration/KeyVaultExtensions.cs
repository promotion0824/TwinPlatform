#nullable enable
using System;
using System.Reflection;
using Azure.Identity;
using Microsoft.Extensions.Configuration;

namespace Willow.Common.Configuration
{
    public static class KeyVaultExtensions
    {
        public static IConfigurationBuilder ConfigurePrefixedAzureKeyVault(this IConfigurationBuilder builder)
        {
            var keyVaultConfig = builder.Build().GetSection("Azure:KeyVault").Get<KeyVaultConfig>();

            if (string.IsNullOrEmpty(keyVaultConfig?.KeyVaultName))
            {
                return builder;
            }

            return builder.AddAzureKeyVault(new Uri($"https://{keyVaultConfig.KeyVaultName}.vault.azure.net"),
                new DefaultAzureCredential(),
                new KeyVaultPrefixSecretManager());
        }
    }
}