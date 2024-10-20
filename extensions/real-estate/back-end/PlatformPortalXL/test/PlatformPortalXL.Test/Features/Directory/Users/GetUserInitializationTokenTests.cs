using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.ServicesApi.DirectoryApi;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Directory.Users
{
    public class GetUserInitializationTokenTests : BaseInMemoryTest
    {
        public GetUserInitializationTokenTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GivenValidToken_GetUserInitializationToken_ReturnsTokenInformation()
        {
            var token = Fixture.Create<string>();
            var expectedTokenInfo = Fixture.Create<UserInitializationToken>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"initializeUserTokens/{token}")
                    .ReturnsJson(expectedTokenInfo);

                var response = await client.GetAsync($"initializeUserTokens/{token}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<UserInitializationToken>();
                result.Should().BeEquivalentTo(expectedTokenInfo);
            }
        }

    }
}