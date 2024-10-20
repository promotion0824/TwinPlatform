namespace Willow.Tests.Infrastructure
{
    using System;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    public class ServerFixtureConfiguration
    {
        public Type StartupType { get; set; }

        public Action<IServiceCollection> MainServicePostConfigureServices { get; set; }

        public Action<IConfigurationBuilder> MainServicePostAppConfiguration { get; set; }

        public bool EnableTestAuthentication { get; set; }
    }
}
