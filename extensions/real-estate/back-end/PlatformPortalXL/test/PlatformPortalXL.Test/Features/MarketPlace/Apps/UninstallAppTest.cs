using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using Moq.Contrib.HttpClient;
using PlatformPortalXL.Helpers;

namespace PlatformPortalXL.Test.Features.MarketPlace.Apps
{
    public class UninstallAppTest : BaseInMemoryTest
    {
        public UninstallAppTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task AuthNotProvided_UninstallApp_ReturnsUnauthorized()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                var response = await client.DeleteAsync($"sites/{Guid.NewGuid()}/installedApps/{Guid.NewGuid()}");
                response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        [Fact]
        public async Task AppInstalled_UninstallApp_ReturnsNoContent()
        {
            var siteId = Guid.NewGuid();
            var existingAppId = Guid.NewGuid();
            var expectedApp = Fixture.Build<App>().With(a => a.Id, existingAppId).With(a => a.ManifestJson, "{}").Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageApps, siteId))
            {
                
                var arrangement = server.Arrange();

                arrangement.GetMarketPlaceApi()
                    .SetupRequest(HttpMethod.Delete, $"sites/{siteId}/installedApps/{existingAppId}")
                    .ReturnsResponse(HttpStatusCode.OK);

                arrangement.GetMarketPlaceApi()
                    .SetupRequest(HttpMethod.Get, $"apps/{existingAppId}")
                    .ReturnsJson(expectedApp);

                var response = await client.DeleteAsync($"sites/{siteId}/installedApps/{existingAppId}");

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }

        [Fact]
        public async Task AppInstalled_WithPointsCapability_UninstallApp_ReturnsNoContent()
        {
            var manifest = Fixture.Build<AppManifest>()
                .With(x => x.Capabilities, () => new List<string> {Capabilities.ProvidePoints})
                .Create();

            var siteId = Guid.NewGuid();
            var existingAppId = Guid.NewGuid();
            var expectedApp = Fixture.Build<App>()
                .With(a => a.Id, existingAppId)
                .With(a => a.ManifestJson, () => JsonSerializerHelper.Serialize(manifest))
                .Create();

            var expectedConnector = Fixture.Build<Connector>()
                .With(x => x.ConnectorTypeId, existingAppId)
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageApps, siteId))
            {
                
                var arrangement = server.Arrange();

                arrangement.GetMarketPlaceApi()
                    .SetupRequest(HttpMethod.Delete, $"sites/{siteId}/installedApps/{existingAppId}")
                    .ReturnsResponse(HttpStatusCode.OK);

                arrangement.GetMarketPlaceApi()
                    .SetupRequest(HttpMethod.Get, $"apps/{existingAppId}")
                    .ReturnsJson(expectedApp);

                arrangement.GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/connectors/bytype/{expectedConnector.ConnectorTypeId}")
                    .ReturnsJson(expectedConnector);

                arrangement.GetConnectorApi()
                    .SetupRequest(HttpMethod.Post, $"connectors/{expectedConnector.Id}/disable")
                    .ReturnsJson(expectedConnector);

                var response = await client.DeleteAsync($"sites/{siteId}/installedApps/{existingAppId}");

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_UninstallApp_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ManageApps, siteId))
            {
                var response = await client.DeleteAsync($"sites/{siteId}/installedApps/{Guid.NewGuid()}");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

    }
}
