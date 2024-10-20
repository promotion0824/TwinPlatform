using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Models;
using PlatformPortalXL.Dto;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Platform.Models;
using Willow.Platform.Users;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.MarketPlace.Apps
{
    public class GetCommonAppsTests : BaseInMemoryTest
    {
        public GetCommonAppsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task AuthNotProvided_ReturnsUnauthorized()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                var response = await client.GetAsync("me/sites/commonApps");
                response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        [Fact]
        public async Task AppsExist_ReturnsAppsInstalledOnAllSites()
        {
            var user = Fixture.Create<User>();
            var expectedSites = Fixture.CreateMany<Site>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, user.Id))
            {
                var directoryApiHandler = server.Arrange().GetDirectoryApi();
                var connectorApiHandler = server.Arrange().GetConnectorApi();

                directoryApiHandler.SetupRequest(HttpMethod.Get, $"users/{user.Id}/sites?permissionId={Permissions.ViewApps}")
                                    .ReturnsJson(expectedSites);

                var expectedApps = Fixture.Build<App>().With(a => a.ManifestJson, "{}").CreateMany(5);
                var arrangement = server.Arrange();
                arrangement.GetMarketPlaceApi()
                    .SetupRequest(HttpMethod.Get, $"apps")
                    .ReturnsJson(AppDto.MapFrom(expectedApps, arrangement.GetImageUrlHelper()));

                foreach (var site in expectedSites)
                {
                    var siteInstallations = new List<Installation>();
                    foreach (var app in expectedApps.Take(2))
                    {
                        var installation = Fixture.Build<Installation>()
                                                  .With(i => i.AppId, app.Id)
                                                  .Create();
                        siteInstallations.Add(installation);
                    }

                    arrangement.GetMarketPlaceApi()
                        .SetupRequest(HttpMethod.Get, $"sites/{site.Id}/installedApps")
                        .ReturnsJson(siteInstallations);
                }

                var response = await client.GetAsync("me/sites/commonApps");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<AppDto>>();
                result.Should().HaveCount(2);

                var expectedDtos = AppDto.MapFrom(expectedApps.Take(2), arrangement.GetImageUrlHelper());
                expectedDtos.ForEach(d => d.IsInstalled = true);
                result.Should().BeEquivalentTo(expectedDtos, config => {
                    config.Excluding(a => a.IconUrl);
                    config.Excluding(a => a.CategoryNames);
                    config.Excluding(a => a.ImageUrls);
                    return config;
                });
            }
        }

        [Fact]
        public async Task UserDoesNotHavePermissionToAnySites_ReturnsEmptyList()
        {
            var user = Fixture.Create<User>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, user.Id))
            {
                var directoryApiHandler = server.Arrange().GetDirectoryApi();
                var connectorApiHandler = server.Arrange().GetConnectorApi();

                directoryApiHandler.SetupRequest(HttpMethod.Get, $"users/{user.Id}/sites?permissionId={Permissions.ViewApps}")
                    .ReturnsJson(new List<Site>{});

                var expectedApps = Fixture.Build<App>().With(a => a.ManifestJson, "{}").CreateMany(5);
                var arrangement = server.Arrange();
                arrangement.GetMarketPlaceApi()
                    .SetupRequest(HttpMethod.Get, $"apps")
                    .ReturnsJson(AppDto.MapFrom(expectedApps, arrangement.GetImageUrlHelper()));

                var response = await client.GetAsync("me/sites/commonApps");
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<List<AppDto>>();
                result.Should().BeEmpty();
            }
        }

    }
}