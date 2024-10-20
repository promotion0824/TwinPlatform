using System;
using System.Reflection;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;

namespace Willow.Common.Configuration
{
    public class KeyVaultPrefixSecretManager : KeyVaultSecretManager
    {
        private const string WillowCommonPrefix = "WillowCommon--"; // We will remove it in future?
        private const string CommonPrefix = "Common--";
        private readonly string _prefix;
        private readonly string _versionedPrefix;

        public KeyVaultPrefixSecretManager()
        {
            var assemblyName = Assembly.GetEntryAssembly()?.GetName();
            _prefix = $"{assemblyName?.Name}--";
            // The appVersion obtains the app version (1.0.0.0), which
            // is set in the project file and obtained from the entry
            // assembly. The versionPrefix holds the major version
            // for the PrefixKeyVaultSecretManager.
            _versionedPrefix = $"{assemblyName?.Name}--{assemblyName?.Version?.Major}--";
        }

        public override bool Load(SecretProperties secret)
        {
            var secretName = secret.Name;
            // Load a vault secret when its secret name starts with the
            // common prefix or application prefix. Other secrets won't be loaded.
            return secretName.StartsWith(WillowCommonPrefix, StringComparison.InvariantCulture)
                   || secretName.StartsWith(CommonPrefix, StringComparison.InvariantCulture)
                   || secretName.StartsWith(_prefix, StringComparison.InvariantCulture)
                   || secretName.StartsWith(_versionedPrefix, StringComparison.InvariantCulture);
        }

        public override string GetKey(KeyVaultSecret secret)
        {
            var secretName = secret.Name;
            // Remove the prefix from the secret name and replace two
            // dashes in any name with the KeyDelimiter, which is the
            // delimiter used in configuration (usually a colon). Azure
            // Key Vault doesn't allow a colon in secret names.

            var prefix = _prefix;


            var isWillowCommonSecret = secretName.StartsWith(WillowCommonPrefix, StringComparison.InvariantCulture);
            var isCommonSecret = secretName.StartsWith(CommonPrefix, StringComparison.InvariantCulture);
            var isVersionedPrefixSecret = secretName.StartsWith(_versionedPrefix, StringComparison.InvariantCulture);

            if (isVersionedPrefixSecret)
            {
                prefix = _versionedPrefix;
            }

            if (isCommonSecret)
            {
                prefix = CommonPrefix;
            }

            if (isWillowCommonSecret)
            {
                prefix = WillowCommonPrefix;
            }

            return secretName.Substring(prefix.Length)
                .Replace("--", ConfigurationPath.KeyDelimiter, StringComparison.InvariantCulture);
        }
    }
}