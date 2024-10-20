using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Helpers;
using PlatformPortalXL.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Willow.Platform.Models;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.ArcGis
{
    public class ProxyTests : BaseInMemoryTest
    {
		public ProxyTests(ITestOutputHelper output) : base(output)
        {
		}

		[Fact]
        public async Task GivenValidCredentials_Proxy_ReturnsResponse()
        {
			var arcgisurl = HttpUtility.UrlEncode("arcgis/sharing/rest/content/items/c56d09d737464c288852cb16f80680b1?f=json&token=uX16Dk744yf30y0rf1Qh6nh2Q-rkFk3Pz0k18BtXW-hsmoSmolcfIJ11cd-2DFMUFApf2t0d2oRvtif3KOJLXVPKUn1fRqVijzGmWZRoh_5JDhMAELQwSDP5hqzwRr8k");

			var arcgisresponse = "{\"id\":\"c56d09d737464c288852cb16f80680b1\",\"owner\":\"dycampbell\",\"created\":1631307536699,\"modified\":1631307639559,\"guid\":null,\"name\":null,\"title\":\"Airfield\",\"type\":\"Web Map\",\"typeKeywords\":[\"ArcGIS Online\",\"Explorer Web Map\",\"Map\",\"Online Map\",\"Web Map\"],\"description\":null,\"tags\":[\"Airfield\",\"Digital Twin\"],\"snippet\":null,\"thumbnail\":null,\"documentation\":null,\"extent\":[[-97.0994,32.8665],[-96.9732,32.9279]],\"categories\":[],\"spatialReference\":null,\"accessInformation\":null,\"licenseInfo\":null,\"culture\":\"en - us\",\"properties\":null,\"url\":null,\"proxyFilter\":null,\"access\":\"shared\",\"size\":658,\"appCategories\":[],\"industries\":[],\"languages\":[]}";

			var siteId = Guid.Parse("3b1f27d9-a295-4d54-9839-fc5f5c2460fc");

			var customerId = Guid.Parse("35a800f0-67a4-4e24-ba63-cb7e0d485791");

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

				server.Arrange().GetArcGisApi()
					.SetupRequest(HttpMethod.Get, arcgisurl)
					.ReturnsJson(arcgisresponse);

				var response = await client.GetAsync($"sites/{siteId}/arcGisProxy?arcGisUrl={arcgisurl}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsStringAsync();
				result.Should().Be(JsonSerializerHelper.Serialize(arcgisresponse));
            }
        }

        [Fact]
        public async Task GivenArcGisFeatureDisabled_Proxy_ReturnsBadRequest()
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

                var response = await client.GetAsync($"sites/{site.Id}/arcGisProxy");

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
				var response = await client.GetAsync($"sites/{site.Id}/arcGisProxy");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }
    }
}
