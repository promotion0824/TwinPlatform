using System;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Newtonsoft.Json;
using PlatformPortalXL.Models;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.SiteStructure.Forge
{
    public class GetAutodeskForgeTokenTests : BaseInMemoryTest
    {
        public GetAutodeskForgeTokenTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact(Skip = "fix later for site api token")]
        public async Task GetAutodeskToken_ReturnsToken()
        {
            var siteId = Guid.NewGuid();
            var expectedToken = Fixture.Build<AutodeskTokenResponse>().Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, "forge/oauth/token")
                    .ReturnsJsonUsingNewtonsoft(expectedToken);

                var response = await client.GetAsync("forge/oauth/token");
                response.IsSuccessStatusCode.Should().BeTrue();
                var strContent = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<AutodeskTokenResponse>(strContent);
                data.Should().BeEquivalentTo(expectedToken);
            }
        }
    }
}
