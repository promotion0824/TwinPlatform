using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Moq.Contrib.HttpClient;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Directory.Users
{
    public class ResetPasswordTests : BaseInMemoryTest
    {
        public ResetPasswordTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task UserEmailExists_ResetPassword_ReturnsNoContent()
        {
            var userEmail = "test@test123.com";

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Post, $"users/{userEmail}/password/reset")
                    .ReturnsResponse(HttpStatusCode.NoContent);

                var response = await client.PostAsJsonAsync($"users/{userEmail}/password/reset", new object());

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }

        [Fact]
        public async Task UserEmailDoesNotExists_ResetPassword_ReturnsNotFound()
        {
            var userEmail = "nonexisting@test123.com";

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Post, $"users/{userEmail}/password/reset")
                    .ReturnsResponse(HttpStatusCode.NotFound);

                var response = await client.PostAsJsonAsync($"users/{userEmail}/password/reset", new object());
                var srepsonse = await response.Content.ReadAsStringAsync();

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }

    }
}