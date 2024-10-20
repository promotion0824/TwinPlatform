namespace Connector.Nunit.Tests.Infrastructure
{
    using System;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public class ServerFixtureConfiguration
    {
        public Action<IServiceCollection> MainServicePostConfigureServices { get; set; }

        public Action<IConfigurationBuilder> MainServicePostAppConfiguration { get; set; }

        public bool EnableTestAuthentication { get; set; }
    }
}
