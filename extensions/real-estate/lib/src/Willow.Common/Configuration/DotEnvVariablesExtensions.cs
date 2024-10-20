using Microsoft.Extensions.Configuration;

namespace Willow.Common.Configuration;

public static class DotEnvVariablesExtensions
{
    public static IConfigurationBuilder AddDotEnvVariablesFile(this IConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Add(new DotEnvVariablesConfigurationSource());
        return configurationBuilder;
    }

    public static IConfigurationBuilder AddDotEnvVariablesFile(
        this IConfigurationBuilder configurationBuilder,
        string filePath)
    {
        configurationBuilder.Add(new DotEnvVariablesConfigurationSource { FilePath = filePath });
        return configurationBuilder;
    }
}
