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
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.MarketPlace.Apps
{
    public class GetAppsTest : BaseInMemoryTest
    {
        public GetAppsTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task AuthNotProvided_GetApps_ReturnsUnauthorized()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                var response = await client.GetAsync($"apps");
                response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        [Fact]
        public async Task AppExists_GetApps_ReturnsApps()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null))
            {
                var expectedApps = Fixture.Build<App>().With(a => a.ManifestJson, "{}").CreateMany(5);
                var arrangement = server.Arrange();
                arrangement.GetMarketPlaceApi()
                    .SetupRequest(HttpMethod.Get, $"apps")
                    .ReturnsJson(AppDto.MapFrom(expectedApps, arrangement.GetImageUrlHelper()));

                var response = await client.GetAsync($"apps");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<AppDto>>();
                result.Should().HaveCount(5);
                result.Should().BeEquivalentTo(AppDto.MapFrom(expectedApps, arrangement.GetImageUrlHelper()), config => {
                    config.Excluding(a => a.IconUrl);
                    config.Excluding(a => a.CategoryNames);
                    config.Excluding(a => a.ImageUrls);
                    return config;
                });
            }
        }

        [Fact]
        public async Task AppExistsAndSiteIdProvided_GetApps_ReturnsAppsWithCorrectIsInstalledProperty()
        {
            var siteId = Guid.NewGuid();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewApps, siteId))
            {
                var allApps = Fixture.Build<App>().With(a => a.ManifestJson, "{}").CreateMany(5);
                var arrangement = server.Arrange();
                arrangement.GetMarketPlaceApi()
                    .SetupRequest(HttpMethod.Get, $"apps")
                    .ReturnsJson(AppDto.MapFrom(allApps, arrangement.GetImageUrlHelper()));

                var privateApps = new List<AppDto>();
                arrangement.GetMarketPlaceApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/privateApps")
                    .ReturnsJson(privateApps);

                var siteInstallations = new List<Installation>();
                foreach (var app in allApps.Take(2))
                {
                    var installation = Fixture.Build<Installation>()
                                              .With(i => i.AppId, app.Id)
                                              .Create();
                    siteInstallations.Add(installation);
                }

                arrangement.GetMarketPlaceApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/installedApps")
                    .ReturnsJson(siteInstallations);

                var response = await client.GetAsync($"apps?siteId={siteId}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<AppDto>>();
                result.Should().HaveCount(5);
                result.Should().BeEquivalentTo(AppDto.MapFrom(allApps, arrangement.GetImageUrlHelper()), config => {
                    config.Excluding(a => a.IconUrl);
                    config.Excluding(a => a.CategoryNames);
                    config.Excluding(a => a.ImageUrls);
                    config.Excluding(a => a.IsInstalled);
                    return config;
                });
                result.Where(a => a.IsInstalled).Should().HaveCount(2);
                result.Where(a => !a.IsInstalled).Should().HaveCount(3);
                foreach (var app in result)
                {
                    if (siteInstallations.Select(sia => sia.AppId).Contains(app.Id))
                    {
                        app.IsInstalled.Should().BeTrue();
                    }
                    else
                    {
                        app.IsInstalled.Should().BeFalse();
                    }
                }
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_GetAppsWithSiteId_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ViewApps, siteId))
            {
                var response = await client.GetAsync($"apps?siteId={siteId}");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task AppExistsAndSiteIdProvided_GetApps_ReturnsAppsWithPrivateAppsIncluded()
        {
            var siteId = Guid.NewGuid();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewApps, siteId))
            {
                var allApps = new List<App>();
                var arrangement = server.Arrange();

                var apps = Fixture.Build<App>().With(a => a.ManifestJson, "{}").CreateMany(5);
                allApps.AddRange(apps);
                arrangement.GetMarketPlaceApi()
                    .SetupRequest(HttpMethod.Get, $"apps")
                    .ReturnsJson(AppDto.MapFrom(apps, arrangement.GetImageUrlHelper()));

                var privateApps = Fixture.Build<App>().With(a => a.ManifestJson, "{}").CreateMany(2);
                allApps.AddRange(privateApps);
                arrangement.GetMarketPlaceApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/privateApps")
                    .ReturnsJson(AppDto.MapFrom(privateApps, arrangement.GetImageUrlHelper()));

                var siteInstallations = new List<Installation>();
                arrangement.GetMarketPlaceApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/installedApps")
                    .ReturnsJson(siteInstallations);

                var response = await client.GetAsync($"apps?siteId={siteId}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<AppDto>>();
                result.Should().HaveCount(7);
                result.Should().BeEquivalentTo(AppDto.MapFrom(allApps, arrangement.GetImageUrlHelper()), config => {
                    config.Excluding(a => a.IconUrl);
                    config.Excluding(a => a.CategoryNames);
                    config.Excluding(a => a.ImageUrls);
                    config.Excluding(a => a.IsInstalled);
                    return config;
                });
                result.Where(a => a.IsInstalled).Should().HaveCount(0);
                result.Where(a => !a.IsInstalled).Should().HaveCount(allApps.Count);
                foreach (var app in result)
                {
                    if (siteInstallations.Select(sia => sia.AppId).Contains(app.Id))
                    {
                        app.IsInstalled.Should().BeTrue();
                    }
                    else
                    {
                        app.IsInstalled.Should().BeFalse();
                    }
                }
            }
        }
    }
}