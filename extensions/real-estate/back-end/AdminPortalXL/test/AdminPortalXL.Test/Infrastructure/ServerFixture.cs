using Willow.Tests.Infrastructure.Xunit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Willow.Tests.Infrastructure.Security;
using FluentAssertions;
using System.Text.Json;

namespace Willow.Tests.Infrastructure
{
    public class DependencyServiceConfiguration
    {
        public string Name { get; set; }
        public bool IsMultiRegion { get; set; }
        public Action<DependencyServiceHttpHandler> Setup { get; set; }
    }

    public class ServerFixtureConfiguration
    {
        public bool EnableTestAuthentication { get; set; }
        public Type StartupType { get; set; }
        public List<DependencyServiceConfiguration> DependencyServices { get; set; } = new List<DependencyServiceConfiguration>();
        public Action<IServiceCollection> MainServicePostConfigureServices { get; set; }
        public Action<IConfigurationBuilder, TestContext> MainServicePostAppConfiguration { get; set; }
        public IList<string> RegionIds { get; set; }
    }

    public class ServerFixture : IDisposable
    {
        private readonly ServerFixtureConfiguration _fixtureConfiguration;
        private readonly TestContext _testContext;
        private TestServer _server;

        static ServerFixture()
        {
            AssertionOptions.AssertEquivalencyUsing(options =>
                options.Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation)).WhenTypeIs<DateTime>()
            );
        }

        public ServerFixture(ServerFixtureConfiguration fixtureConfiguration, TestContext testContext)
        {
            _fixtureConfiguration = fixtureConfiguration;
            _testContext = testContext;
            _server = StartMainServer(_fixtureConfiguration);
        }

        private TestServer StartMainServer(ServerFixtureConfiguration fixtureConfiguration)
        {
            var wrappedStartupType = typeof(MainServiceStartup<>).MakeGenericType(fixtureConfiguration.StartupType);

            var host = new WebHostBuilder()
                .UseEnvironment("Test")
                .ConfigureAppConfiguration((context, configuration) =>
                {
                    configuration.SetBasePath(Directory.GetCurrentDirectory());
                    fixtureConfiguration.MainServicePostAppConfiguration?.Invoke(configuration, _testContext);
                })
                .ConfigureLogging((context, logger) =>
                {
                    logger.AddProvider(new XunitLoggerProvider(_testContext.Output));
                })
                .ConfigureServices(services =>
                {
                    services.AddSingleton<ServerFixtureConfiguration>(fixtureConfiguration);
                    services.AddSingleton<ServerFixture>(this);
                })
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup(wrappedStartupType);

            return new TestServer(host);
        }

        public ServerArrangement Arrange()
        {
            return new ServerArrangement(_server.Host.Services);
        }

        public ServerAssertion Assert()
        {
            return new ServerAssertion(_server.Host.Services);
        }

        public void Dispose()
        {
            _server.Dispose();
        }

        public HttpClient CreateClient()
        {
            return _server.CreateClient();
        }

        public HttpClient CreateClient(IEnumerable<string> roles, Guid? userId = null, string auth0UserId = null)
        {
            if (!_fixtureConfiguration.EnableTestAuthentication)
            {
                throw new InvalidOperationException("TestAuthentication is not enabled.");
            }

            var client = _server.CreateClient();
            var token = new TestAuthToken
            {
                UserId = userId ?? Guid.NewGuid(),
                Roles = roles?.ToArray(),
                Auth0UserId = auth0UserId,
            };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TestAuthToken.TestScheme, JsonSerializer.Serialize(token));
            return client;
        }
    }

}
