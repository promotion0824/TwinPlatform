using System.Net;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using Willow.HealthChecks;
using FluentAssertions;
using System.Net.Http;
using Workflow.Tests;

namespace WorkflowCore.Test.HealthCheck
{
    public class WillowHealthCheckTests : BaseInMemoryTest
    {
        public WillowHealthCheckTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task HealthCheck_Healthz_Url_ReturnsHealthReport()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient())
            {
                var response = await client.GetAsync("healthz");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<HealthCheckDto>();
                result.Key.Should().BeEquivalentTo("DigitalTwinCore");
                result.Description.Should().BeEquivalentTo("DigitalTwinCore app health.");
            }
        }


        [Fact]
        public async Task HealthCheck_Livez_Url_ReturnsLive()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient())
            {
                var response = await client.GetAsync("livez");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsStringAsync();
                result.Should().BeEquivalentTo("Live");
            }
        }

        [Fact]
        public async Task HealthCheck_Readyz_Url_ReturnsReady()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient())
            {
                var response = await client.GetAsync("readyz");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsStringAsync();
                result.Should().BeEquivalentTo("Ready");
            }
        }
    }
}
