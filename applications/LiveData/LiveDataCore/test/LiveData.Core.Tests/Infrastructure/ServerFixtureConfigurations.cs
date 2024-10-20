namespace Willow.Tests.Infrastructure
{
    using System.Collections.Generic;
    using Microsoft.Extensions.Configuration;

    public class ServerFixtureConfigurations
    {
        public static readonly ServerFixtureConfiguration PostgreSqlServer = new ServerFixtureConfiguration
        {
            EnableTestAuthentication = true,
            StartupType = typeof(Program),
            MainServicePostConfigureServices = _ =>
            {
            },
            MainServicePostAppConfiguration = (builder) =>
            {
                var config = new Dictionary<string, string>
                {
                    ["EnableSwagger"] = "true",
                };
                builder.AddInMemoryCollection(config);
            },
        };
    }
}
