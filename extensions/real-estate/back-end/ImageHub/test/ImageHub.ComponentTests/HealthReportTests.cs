using System.Net;
using System.Threading.Tasks;
using Xunit;
using Willow.HealthChecks;
using FluentAssertions;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ImageHub.Test.HealthCheck
{
    public class WillowHealthCheckTests : IClassFixture<WebApplicationFactory<ImageHub.Startup>>
    {
        private readonly HttpClient _client;

        public WillowHealthCheckTests(WebApplicationFactory<ImageHub.Startup> fixture)
        {
            _client = fixture.CreateClient();
        }

        [Fact]
        public async Task HealthCheck_Healthz_Url_ReturnsHealthReport()
        {
            var response = await _client.GetAsync("healthz");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<HealthCheckDto>();
            result.Key.Should().BeEquivalentTo("ImageHub");
            result.Description.Should().BeEquivalentTo("ImageHub app health.");
        }

        [Fact]
        public async Task HealthCheck_Livez_Url_ReturnsLive()
        {
            var response = await _client.GetAsync("livez");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsStringAsync();
            result.Should().BeEquivalentTo("Live");
        }

        [Fact]
        public async Task HealthCheck_Readyz_Url_ReturnsReady()
        {
            var response = await _client.GetAsync("readyz");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsStringAsync();
            result.Should().BeEquivalentTo("Ready");
        }
    }
}
