using System.Reflection;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Willow.Alert.Resolver.Infrastructure;

namespace Willow.Alert.Resolver;

internal static class ConfigurationBuilderExtension
{
    public static IConfiguration AddKeyVaultConfiguration(this IConfigurationBuilder configurationBuilder, IConfiguration configuration)
    {
        var keyVaultName = configuration.GetValue<string>("Azure:KeyVault:KeyVaultName");
        if (string.IsNullOrEmpty(keyVaultName))
        {
            return configuration;
        }

        var keyVaultEndpoint = $"https://{keyVaultName}.vault.azure.net";
        var keyVaultClient = new SecretClient(new Uri(keyVaultEndpoint), new DefaultAzureCredential());

        var prefix = string.Join(string.Empty, Assembly.GetAssembly(typeof(Program))!.GetName().Name!.Where(c => (char.IsLetterOrDigit(c) && c < 128) || c == '-'));

        configurationBuilder.AddAzureKeyVault(keyVaultClient, new PrefixKeyVaultSecretManager(prefix));
        configuration = configurationBuilder.Build();

        return configuration;
    }
}
