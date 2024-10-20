using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Features.SiteStructure.Requests;
using PlatformPortalXL.Models;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.SiteStructure.Sites
{
    public class CreateNewAdtSiteTests : BaseInMemoryTest
    {
        public CreateNewAdtSiteTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GivenValidInput_CreateNewAdtSite_ReturnsTheCreatedSiteUrl()
        {
            var expectedRequestToApi = Fixture.Create<NewAdtSiteRequest>();
            var siteCreatedUrl = $"admin/sites/{expectedRequestToApi.SiteId}";

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageSites, expectedRequestToApi.SiteId))
            {
                server.Arrange().GetDigitalTwinApi()
                    .SetupRequestWithExpectedBody(HttpMethod.Post, $"admin/sites", expectedRequestToApi)
                    .ReturnsJson(HttpStatusCode.Created, siteCreatedUrl);

                var response = await client.PostAsJsonAsync($"sites", expectedRequestToApi);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<string>();
                result.Should().Be(siteCreatedUrl);
            }
        }
    }
}