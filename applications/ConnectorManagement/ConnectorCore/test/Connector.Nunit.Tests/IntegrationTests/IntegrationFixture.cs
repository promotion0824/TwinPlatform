namespace Connector.Nunit.Tests.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Connector.Nunit.Tests.Infrastructure;
    using ConnectorCore;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    [SetUpFixture]
    public class IntegrationFixture
    {
        public static WebApplicationFactory<Program> WebApplicationFactory { get; private set; }

        public static TestServer Server => WebApplicationFactory.Server;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var fixtureConfiguration = ServerFixtureConfigurations.SqlServer;

            WebApplicationFactory = new WebApplicationFactory<Program>().WithWebHostBuilder(host =>
            {
                host
                    .UseEnvironment("Test")
                    .ConfigureAppConfiguration((context, configuration) =>
                    {
                        configuration.Sources.Clear();
                        var env = context.HostingEnvironment;
                        var dirName = FindAppSettings(Directory.GetCurrentDirectory());
                        configuration.SetBasePath(dirName);
                        configuration
                            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                            .AddJsonFile("appsettings.unversioned.json", optional: true)
                            .AddEnvironmentVariables();

                        fixtureConfiguration.MainServicePostAppConfiguration?.Invoke(configuration);
                    })
                    .ConfigureServices(services =>
                    {
                        //services.AddSingleton(fixtureConfiguration);

                        //var serviceProvider = services.BuildServiceProvider();
                        //var serverFixtureConfiguration = serviceProvider.GetRequiredService<ServerFixtureConfiguration>();
                        if (fixtureConfiguration.EnableTestAuthentication)
                        {
                            services.AddTestAuthentication();
                        }

                        services.AddMvc().AddApplicationPart(typeof(Program).Assembly);

                        fixtureConfiguration.MainServicePostConfigureServices?.Invoke(services);
                    })
                    .UseKestrel()
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseSetting(WebHostDefaults.ApplicationKey, typeof(Program).Assembly.FullName);
            });
        }

        private string FindAppSettings(string appPath)
        {
            var dir = new DirectoryInfo(appPath);
            while (true)
            {
                if (dir.EnumerateFiles("*appsettings*").Any())
                {
                    return dir.FullName;
                }

                dir = dir.Parent;
                if (dir == null)
                {
                    return null;
                }
            }
        }
    }
}
