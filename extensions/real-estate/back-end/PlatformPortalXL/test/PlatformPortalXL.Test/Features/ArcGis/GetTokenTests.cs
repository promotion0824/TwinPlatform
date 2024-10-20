using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Platform.Models;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.ArcGis
{
    public class GetTokenTests : BaseInMemoryTest
    {
        public GetTokenTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GivenValidCredentials_GetToken_ReturnsToken()
        {
			var customerId = Guid.Parse("35a800f0-67a4-4e24-ba63-cb7e0d485791");
            var siteId = Guid.Parse("3b1f27d9-a295-4d54-9839-fc5f5c2460fc");
            var site = Fixture.Build<Site>()
                .With(x => x.Id, siteId)
				.With(x => x.CustomerId, customerId)
                .With(x => x.Features, new SiteFeatures { IsArcGisEnabled = true })
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, site.Id))
            {
				server.Arrange().GetSiteApi()
					.SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
					.ReturnsJson(site);
				server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}")
                    .ReturnsJson(site);
                var response = await client.GetAsync($"sites/{siteId}/arcGisToken");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<ArcGisDto>();
                result.Token.Should().BeEquivalentTo("token123");
            }
        }

        [Fact]
        public async Task GivenArcGisFeatureDisabled_GetToken_ReturnsBadRequest()
        {
            var site = Fixture.Create<Site>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, site.Id))
            {
				server.Arrange().GetSiteApi()
					.SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
					.ReturnsJson(site);
				server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
                    .ReturnsJson(site);
                var response = await client.GetAsync($"sites/{site.Id}/arcGisToken");

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_GetToken_ReturnsForbidden()
        {
			var customerId = Guid.Parse("35a800f0-67a4-4e24-ba63-cb7e0d485791");
			var siteId = Guid.Parse("3b1f27d9-a295-4d54-9839-fc5f5c2460fc");
			var site = Fixture.Build<Site>()
				.With(x => x.Id, siteId)
				.With(x => x.CustomerId, customerId)
				.With(x => x.Features, new SiteFeatures { IsArcGisEnabled = true })
				.Create();

			using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ViewSites, site.Id))
            {
				server.Arrange().GetSiteApi()
					.SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
					.ReturnsJson(site);
				server.Arrange().GetDirectoryApi()
					.SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
					.ReturnsJson(site);
				var response = await client.GetAsync($"sites/{site.Id}/arcGisToken");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }
    }
}
