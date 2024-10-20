namespace ConnectorCore.KeyVault
{
    using System.Collections.Generic;
    using System.Linq;
    using Azure.Extensions.AspNetCore.Configuration.Secrets;
    using Azure.Security.KeyVault.Secrets;
    using Microsoft.Extensions.Configuration;

    internal class PrefixKeyVaultSecretManager : KeyVaultSecretManager
    {
        private readonly List<string> prefixes = new();

        public PrefixKeyVaultSecretManager(string prefix)
        {
            prefixes.Add($"{prefix}--");
            prefixes.Add("WillowCommon--");
        }

        public override bool Load(SecretProperties secret)
        {
            return prefixes.Any(prefix => secret.Name.StartsWith(prefix));
        }

        public override string GetKey(KeyVaultSecret secret)
        {
            var matchingPrefix = prefixes.First(prefix => secret.Name.StartsWith(prefix));
            return secret.Name[matchingPrefix.Length..]
                .Replace("--", ConfigurationPath.KeyDelimiter);
        }
    }
}
