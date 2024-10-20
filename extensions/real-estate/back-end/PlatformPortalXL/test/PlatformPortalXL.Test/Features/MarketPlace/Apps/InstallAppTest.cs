using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using PlatformPortalXL.Extensions;
using Willow.Platform.Models;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using PlatformPortalXL.Features.MarketPlace;
using Moq.Contrib.HttpClient;
using System.Net.Http.Json;
using PlatformPortalXL.Helpers;

namespace PlatformPortalXL.Test.Features.MarketPlace.Apps
{
    public class InstallAppTest : BaseInMemoryTest
    {
        public InstallAppTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task AuthNotProvided_InstallApp_ReturnsUnauthorized()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                var response = await client.GetAsync($"apps/{Guid.NewGuid()}");
                response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        [Fact]
        public async Task AppIdProvided_InstallApp_ReturnsNoContent()
        {
            var siteId = Guid.NewGuid();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageApps, siteId))
            {
                var existingAppId = Guid.NewGuid();
                var expectedApp = Fixture.Build<App>()
                    .With(a => a.Id, existingAppId)
                    .With(a => a.ManifestJson, "{}")
                    .Create();

                var arrangement = server.Arrange();
                arrangement.GetMarketPlaceApi()
                    .SetupRequest(HttpMethod.Post, $"sites/{siteId}/installedApps")
                    .ReturnsResponse(HttpStatusCode.OK);

                arrangement.GetMarketPlaceApi()
                    .SetupRequest(HttpMethod.Get, $"apps/{existingAppId}")
                    .ReturnsJson(expectedApp);

                var request = new InstallAppRequest() { AppId = existingAppId };
                var response = await client.PostAsJsonAsync($"sites/{siteId}/installedApps", request);

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }

        [Fact]
        public async Task AppIdProvided_InstallApp_WithPointsCapability_ReturnsNoContent()
        {
            var manifest = Fixture.Build<AppManifest>()
                .With(x => x.Capabilities, () => new List<string> {Capabilities.ProvidePoints})
                .Create();

            var existingAppId = Guid.NewGuid();
            var expectedApp = Fixture.Build<App>()
                .With(a => a.Id, existingAppId)
                .With(a => a.ManifestJson, () => JsonSerializerHelper.Serialize(manifest))
                .Create();

            var siteId = Guid.NewGuid();

            var templateConfig = new { Url = "" };

            var expectedSite = Fixture.Build<Site>()
                .With(x => x.Id, siteId)
                .Create();

            var expectedConnectorType = Fixture.Build<ConnectorType>()
                .With(x => x.Id, existingAppId)
                .Create();

            var expectedConnector = new Connector
            {
                ClientId = expectedSite.CustomerId,
                ConnectorTypeId = expectedApp.Id,
                ErrorThreshold = 10,
                IsEnabled = true,
                Name = $"{expectedApp.Name} Connector",
                SiteId = siteId,
                Configuration = JsonSerializerHelper.Serialize(templateConfig)
            };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageApps, siteId))
            {
                var arrangement = server.Arrange();
                arrangement.GetMarketPlaceApi()
                    .SetupRequest(HttpMethod.Post, $"sites/{siteId}/installedApps")
                    .ReturnsResponse(HttpStatusCode.OK);

                arrangement.GetMarketPlaceApi()
                    .SetupRequest(HttpMethod.Get, $"apps/{existingAppId}")
                    .ReturnsJson(expectedApp);

                arrangement.GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}")
                    .ReturnsJson(expectedSite);

                arrangement.GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"connectortypes/{expectedConnectorType.Id}")
                    .ReturnsJson(expectedConnectorType);

                arrangement.GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"schemas/{expectedConnectorType.ConnectorConfigurationSchemaId}/template")
                    .ReturnsJson(templateConfig);

                arrangement.GetConnectorApi()
                    .SetupRequest(HttpMethod.Post, "connectors")
                    .ReturnsJson(expectedConnector);

                arrangement.GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/connectors/bytype/{expectedConnector.ConnectorTypeId}")
                    .ReturnsResponse(HttpStatusCode.NotFound);

                var request = new InstallAppRequest() { AppId = existingAppId };
                var response = await client.PostAsJsonAsync($"sites/{siteId}/installedApps", request);

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }

        [Fact]
        public async Task AppIdProvided_InstallApp_WithPointsCapability_ConnectorExists_ReturnsNoContent()
        {
            var manifest = Fixture.Build<AppManifest>()
                .With(x => x.Capabilities, () => new List<string> {Capabilities.ProvidePoints})
                .Create();

            var existingAppId = Guid.NewGuid();
            var expectedApp = Fixture.Build<App>()
                .With(a => a.Id, existingAppId)
                .With(a => a.ManifestJson, () => JsonSerializerHelper.Serialize(manifest))
                .Create();

            var siteId = Guid.NewGuid();

            var templateConfig = new { Url = "" };

            var expectedSite = Fixture.Build<Site>()
                .With(x => x.Id, siteId)
                .Create();

            var expectedConnector = new Connector
            {
                ClientId = expectedSite.CustomerId,
                ConnectorTypeId = expectedApp.Id,
                ErrorThreshold = 10,
                IsEnabled = false,
                Name = $"{expectedApp.Name} Connector",
                SiteId = siteId,
                Configuration = JsonSerializerHelper.Serialize(templateConfig)
            };

            
            var connectorFormData = expectedConnector.ToFormData(true);
            var connectorNvCollection = new NameValueCollection();
            foreach (var kv in connectorFormData)
            {
                connectorNvCollection.Add(kv.Key, kv.Value);
            }

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageApps, siteId))
            {
                

                var arrangement = server.Arrange();
                arrangement.GetMarketPlaceApi()
                    .SetupRequest(HttpMethod.Post, $"sites/{siteId}/installedApps")
                    .ReturnsResponse(HttpStatusCode.OK);

                arrangement.GetMarketPlaceApi()
                    .SetupRequest(HttpMethod.Get, $"apps/{existingAppId}")
                    .ReturnsJson(expectedApp);

                arrangement.GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}")
                    .ReturnsJson(expectedSite);

                arrangement.GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/connectors/bytype/{expectedConnector.ConnectorTypeId}")
                    .ReturnsJson(expectedConnector);

                arrangement.GetConnectorApi()
                    .SetupRequest(HttpMethod.Post, $"connectors/{expectedConnector.Id}/enable")
                    .ReturnsResponse(HttpStatusCode.OK);

                var request = new InstallAppRequest() { AppId = existingAppId };
                var response = await client.PostAsJsonAsync($"sites/{siteId}/installedApps", request);

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_InstallApp_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ManageApps, siteId))
            {
                var response = await client.PostAsJsonAsync($"sites/{siteId}/installedApps", new InstallAppRequest());

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

    }
}
