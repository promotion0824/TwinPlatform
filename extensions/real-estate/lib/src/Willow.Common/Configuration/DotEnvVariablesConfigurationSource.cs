#nullable enable
using Microsoft.Extensions.Configuration;

namespace Willow.Common.Configuration;

public class DotEnvVariablesConfigurationSource : IConfigurationSource
{
    public string? FilePath { get; set; }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new DotEnvVariablesConfigurationProvider(FilePath);
    }
}
