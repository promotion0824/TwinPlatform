using System;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;

namespace Willow.Infrastructure.Azure
{
    public class PrefixKeyVaultSecretManager : IKeyVaultSecretManager
    {
        private const string WillowCommonPrefix = "WillowCommon--"; // We will remove it in future
        private const string CommonPrefix = "Common--";
        private readonly string _prefix;

        public PrefixKeyVaultSecretManager(string prefix)
        {
            _prefix = $"{prefix}--";
        }

        public bool Load(SecretItem secret)
        {
            // Load a vault secret when its secret name starts with the
            // common prefix or application prefix. Other secrets won't be loaded.
            var name = secret.Identifier.Name;
            return name.StartsWith(WillowCommonPrefix, StringComparison.InvariantCulture)
                || name.StartsWith(CommonPrefix, StringComparison.InvariantCulture)
                || name.StartsWith(_prefix, StringComparison.InvariantCulture);
        }

        public string GetKey(SecretBundle secret)
        {
            // Remove the prefix from the secret name and replace two
            // dashes in any name with the KeyDelimiter, which is the
            // delimiter used in configuration (usually a colon). Azure
            // Key Vault doesn't allow a colon in secret names.
            var isWillowCommonSecret = secret.SecretIdentifier.Name.StartsWith(WillowCommonPrefix, StringComparison.InvariantCulture);
            var isCommonSecret = secret.SecretIdentifier.Name.StartsWith(CommonPrefix, StringComparison.InvariantCulture);
            var prefix = _prefix;
            if (isCommonSecret)
            {
                prefix = CommonPrefix;
            }
            if (isWillowCommonSecret)
            {
                prefix = WillowCommonPrefix;
            }

            var secretName = secret.SecretIdentifier.Name.Substring(prefix.Length);
            return secretName.Replace("--", ConfigurationPath.KeyDelimiter, StringComparison.InvariantCulture);
        }
    }
}
