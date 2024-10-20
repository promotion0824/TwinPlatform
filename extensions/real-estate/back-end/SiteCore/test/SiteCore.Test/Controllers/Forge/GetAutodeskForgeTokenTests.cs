using System.Collections.Specialized;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SiteCore.Services;
using SiteCore.Tests;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace SiteCore.Test.Controllers.Forge
{
    public class GetAutodeskForgeTokenTests : BaseInMemoryTest
    {
        public GetAutodeskForgeTokenTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GetAutodeskToken_ReturnsToken()
        {
            var expectedToken = Fixture.Build<AutodeskTokenResponse>()
                                       .With(x => x.ExpiresIn, 1000)
                                       .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var genericHandler = server.Arrange().MainServices.GetRequiredService<Mock<HttpMessageHandler>>();
                var depService = new DependencyServiceHttpHandler(genericHandler, Constants.Forge.TokenEndpoint);
                depService.SetupRequestWithExpectedQueryParameters(HttpMethod.Post, "", new NameValueCollection())
                    .ReturnsJson(expectedToken);

                var response = await client.GetAsync("forge/oauth/token");
                response.IsSuccessStatusCode.Should().BeTrue();
                var token = await response.Content.ReadAsAsync<AutodeskTokenResponse>();
                token.Should().BeEquivalentTo(expectedToken);
            }
        }
    }
}
