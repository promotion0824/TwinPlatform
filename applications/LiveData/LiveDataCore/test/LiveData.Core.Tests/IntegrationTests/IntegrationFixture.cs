namespace Willow.LiveData.Core.Tests.IntegrationTests
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using NUnit.Framework;
    using Willow.Tests.Infrastructure;
    using Willow.Tests.Infrastructure.Abstractions;

    //[SetUpFixture]
    public class IntegrationFixture
    {
        public static TestServer Server { get; private set; }

        public static Guid ClientId { get; private set; }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var fixtureConfiguration = ServerFixtureConfigurations.PostgreSqlServer;
            var wrappedStartupType = typeof(MainServiceStartup<>).MakeGenericType(fixtureConfiguration.StartupType);

            var host = new WebHostBuilder()
                .UseEnvironment("Test")
                .ConfigureAppConfiguration((context, configuration) =>
                {
                    var env = context.HostingEnvironment;
                    var dirName = FindAppSettings(Directory.GetCurrentDirectory());
                    configuration.SetBasePath(dirName);
                    configuration
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                        .AddJsonFile("appsettings.unversioned.json", optional: true);

                    fixtureConfiguration.MainServicePostAppConfiguration?.Invoke(configuration);
                })
                .ConfigureServices(services =>
                {
                    services.AddSingleton(fixtureConfiguration);
                })
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseSetting(WebHostDefaults.ApplicationKey, fixtureConfiguration.StartupType.GetTypeInfo().Assembly.FullName)
                .UseStartup(wrappedStartupType);

            Server = new TestServer(host);
            ClientId = Server.Host.Services.GetService<IConfiguration>().GetValue<Guid>("TestClientId");
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

        [OneTimeTearDown]
        public void OneTimeTeardown()
        {
            var eraser = Server.Host.Services.GetRequiredService<IDatabaseEraser>();
            eraser.EraseDb();
        }
    }
}
