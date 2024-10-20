#nullable disable
using Microsoft.Extensions.Configuration;

namespace Willow.Common.Configuration
{
    public static class ConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder ConfigureWillowAppConfiguration(this IConfigurationBuilder builder)
        {
            return builder
                .AddDotEnvVariablesFile()
                .ConfigurePrefixedAzureKeyVault();
        }

    }
}