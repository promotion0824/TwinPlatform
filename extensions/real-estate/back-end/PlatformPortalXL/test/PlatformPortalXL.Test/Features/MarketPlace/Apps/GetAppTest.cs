using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Models;
using PlatformPortalXL.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using static PlatformPortalXL.Services.MarketPlaceApi.MarketPlaceApiService;
using System.Globalization;
using PlatformPortalXL.Helpers;

namespace PlatformPortalXL.Test.Features.MarketPlace.Apps
{
    public class GetAppTest : BaseInMemoryTest
    {
        public GetAppTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task AuthNotProvided_GetApp_ReturnsUnauthorized()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                var response = await client.GetAsync($"apps/{Guid.NewGuid()}");
                response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        [Fact]
        public async Task AppExist_GetApp_ReturnsApp()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null))
            {
                var existingAppId = Guid.NewGuid();
                var galleries = Fixture.Build<GalleryVisual>().CreateMany(3);
                var expectedApp = Fixture.Build<App>()
                                         .With(a => a.Id, existingAppId)
                                         .With(a => a.ManifestJson, "{}")
                                         .With(a => a.Gallery, galleries.ToList())
                                         .Create();

                var arrangement = server.Arrange();
                arrangement.GetMarketPlaceApi()
                    .SetupRequest(HttpMethod.Get, $"apps/{existingAppId}")
                    .ReturnsJson(AppDto.MapFrom(expectedApp, arrangement.GetImageUrlHelper()));

                var response = await client.GetAsync($"apps/{existingAppId}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<AppDto>();
                result.Should().BeEquivalentTo(AppDto.MapFrom(expectedApp, arrangement.GetImageUrlHelper()), config => {
                    config.Excluding(a => a.IconUrl);
                    config.Excluding(a => a.CategoryNames);
                    config.Excluding(a => a.ImageUrls);
                    return config;
                });
            }
        }

        [Fact]
        public async Task AppExistsAndSiteIdProvided_GetApp_ReturnAppWithCorrectIsInstalledProperty()
        {
            var siteId = Guid.NewGuid();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewApps, siteId))
            {
                var existingAppId = Guid.NewGuid();
                var expectedApp = Fixture.Build<App>()
                                         .With(a => a.Id, existingAppId)
                                         .With(a => a.ManifestJson, "{}")
                                         .Create();

                var arrangement = server.Arrange();
                arrangement.GetMarketPlaceApi()
                    .SetupRequest(HttpMethod.Get, $"apps/{existingAppId}?siteId={siteId}")
                    .ReturnsJson(AppDto.MapFrom(expectedApp, arrangement.GetImageUrlHelper()));
                arrangement.GetMarketPlaceApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/installedApps")
                    .ReturnsJson(new List<Installation>() { new Installation() { AppId = existingAppId } });

                var response = await client.GetAsync($"apps/{existingAppId}?siteId={siteId}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<AppDto>();
                result.Should().BeEquivalentTo(AppDto.MapFrom(expectedApp, arrangement.GetImageUrlHelper()), config => {
                    config.Excluding(a => a.IconUrl);
                    config.Excluding(a => a.CategoryNames);
                    config.Excluding(a => a.ImageUrls);
                    config.Excluding(a => a.IsInstalled);
                    return config;
                });

                result.IsInstalled.Should().BeTrue();
            }
        }

        [Fact]
        public async Task ConfigurationUrlProvided_GetApp_ReturnAppWithSiteIdQueryAppended()
        {
            var siteId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var utcNow = DateTime.UtcNow;

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewApps, siteId))
            {
                var appId = Guid.NewGuid();
                var appManifest = Fixture.Build<AppManifest>().Create();
                var expectedApp = Fixture.Build<App>()
                                         .With(a => a.Id, appId)
                                         .With(a => a.ManifestJson, JsonSerializerHelper.Serialize(appManifest))
                                         .Create();
                var expectedSignature = Guid.NewGuid().ToString();

                var arrangement = server.Arrange();
                arrangement.SetCurrentDateTime(utcNow);
                arrangement.GetMarketPlaceApi()
                    .SetupRequest(HttpMethod.Get, $"apps/{appId}?siteId={siteId}")
                    .ReturnsJson(expectedApp);
                arrangement.GetMarketPlaceApi()
                    .SetupRequest(HttpMethod.Get, $"apps/{appId}/signatures?payload={{\"siteId\"%3A\"{siteId}\",\"userId\"%3A\"{userId}\",\"timestamp\"%3A\"{utcNow.ToString("o", CultureInfo.InvariantCulture).Replace(":", "%3A")}\"}}")
                    .ReturnsJson(new GetSignatureResponse { Signature = expectedSignature });
                arrangement.GetMarketPlaceApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/installedApps")
                    .ReturnsJson(new List<Installation>() { new Installation() { AppId = appId } });

                var response = await client.GetAsync($"apps/{appId}?siteId={siteId}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<AppDto>();
                result.Should().BeEquivalentTo(AppDto.MapFrom(expectedApp, arrangement.GetImageUrlHelper()), config => {
                    config.Excluding(a => a.IconUrl);
                    config.Excluding(a => a.CategoryNames);
                    config.Excluding(a => a.ImageUrls);
                    config.Excluding(a => a.IsInstalled);
                    config.Excluding(a => a.Manifest);
                    return config;
                });

                result.Manifest.Should().NotBeNull();
                var uriBuilder = new UriBuilder(result.Manifest.ConfigurationUrl);
                var query = HttpUtility.ParseQueryString(uriBuilder.Query);
                var siteIdInConfigurationQuery = query["siteId"];
                siteIdInConfigurationQuery.Should().Be(siteId.ToString());

                result.IsInstalled.Should().BeTrue();
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_GetAppWithSiteId_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ViewApps, siteId))
            {
                var response = await client.GetAsync($"apps/{Guid.NewGuid()}?siteId={siteId}");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

    }
}
