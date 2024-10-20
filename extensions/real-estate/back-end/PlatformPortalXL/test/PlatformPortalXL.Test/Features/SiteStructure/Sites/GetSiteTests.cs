using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using PlatformPortalXL.ServicesApi.ConnectorApi;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;
using PlatformPortalXL.Test.MockServices;
using Willow.Platform.Models;
using Willow.Platform.Users;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using Willow.Workflow;
using PlatformPortalXL.ServicesApi.InsightApi;

namespace PlatformPortalXL.Test.Features.SiteStructure.Sites
{
    public class GetSiteTests : BaseInMemoryTest
    {
        public GetSiteTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task SiteExists_ReturnsThatSite()
        {
            var customerId = Guid.NewGuid();

            var user = Fixture.Build<User>()
                .With(x => x.CustomerId, customerId)
                .Create();

            var expectedCustomer = Fixture.Build<Customer>()
                .With(x => x.Id, customerId)
                .With(x => x.Features, new CustomerFeatures())
                .Create();

            var site = Fixture.Build<Site>()
                .With(x => x.CustomerId, customerId)
                .Without(x => x.Features)
                .Create();
            var siteId = site.Id;
            var siteDirectoryCoreDB = Fixture.Create<Site>();
            var siteSettings = Fixture.Create<SiteSettings>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}")
                    .ReturnsJson(site);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}")
                    .ReturnsJson(siteDirectoryCoreDB);
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/settings")
                    .ReturnsJson(siteSettings);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{customerId}")
                    .ReturnsJson(expectedCustomer);

                var response = await client.GetAsync($"sites/{siteId}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<SiteDetailDto>();
                var expectedSiteDetailDto = SiteDetailDto.Map(site, server.Assert().GetImageUrlHelper());
                expectedSiteDetailDto.Features = SiteFeaturesDto.Map(siteDirectoryCoreDB.Features);
				expectedSiteDetailDto.ArcGisLayers = ArcGisLayerDto.Map(siteDirectoryCoreDB.ArcGisLayers);
                expectedSiteDetailDto.Settings = SiteSettingsDto.Map(siteSettings);
				expectedSiteDetailDto.WebMapId = siteDirectoryCoreDB.WebMapId;
                result.Should().BeEquivalentTo(expectedSiteDetailDto);
            }
        }

        [Fact]
        public async Task SiteExistsForCustomerWithPorfolioDashboard_ReturnsThatSite()
        {
            var customerId = Guid.NewGuid();

            var user = Fixture.Build<User>()
                .With(x => x.CustomerId, customerId)
                .Create();

            var expectedCustomer = Fixture.Build<Customer>()
                .With(x => x.Id, customerId)
                .With(x => x.Features, new CustomerFeatures { IsConnectivityViewEnabled = true })
                .Create();

            var site = Fixture.Build<Site>()
                .With(x => x.CustomerId, customerId)
                .Without(x => x.Features)
                .With(x => x.Status, SiteStatus.Operations)
                .Create();
            var siteId = site.Id;
            var siteDirectoryCoreDB = Fixture.Build<Site>()
                .With(x => x.Id, siteId)
                .With(x => x.CustomerId, customerId)
                .With(x => x.Features, new SiteFeatures())
				.With(x => x.ArcGisLayers, new List<ArcGisLayer>())
                .Without(x => x.Widgets)
                .Create();
            var siteSettings = Fixture.Create<SiteSettings>();

            var expectedConnectorLogRecords = new Dictionary<Guid, ConnectorLogRecord>();

            var gateways = new List<Gateway>
            {
                Fixture.Build<Gateway>()
                    .With(x => x.IsEnabled, true)
                    .With(x => x.IsOnline, true)
                    .With(x => x.SiteId, site.Id)
                    .With(x => x.CustomerId, customerId)
                    .With(x => x.Connectors, Fixture.Build<Connector>()
                        .With(x => x.IsArchived, false)
                        .With(x => x.IsEnabled, true)
                        .With(x => x.SiteId, site.Id)
                        .With(x => x.ClientId, customerId)
                        .CreateMany(2)
                        .ToList())
                    .Create()
            };

            foreach (var connector in gateways.Single().Connectors)
            {
                expectedConnectorLogRecords[connector.Id] = Fixture.Build<ConnectorLogRecord>()
                    .With(x => x.ConnectorId, connector.Id)
                    .With(x => x.CreatedAt, DateTime.UtcNow)
                    .With(x => x.StartTime, DateTime.UtcNow)
                    .Create();
            }

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequestSequence(HttpMethod.Get, $"sites/{siteId}")
                    .ReturnsJson(site)
                    .ReturnsJson(site);
				server.Arrange().GetDirectoryApi()
					.SetupRequestSequence(HttpMethod.Get, $"sites/{siteId}")
					.ReturnsJson(siteDirectoryCoreDB)
					.ReturnsJson(siteDirectoryCoreDB);
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/settings")
                    .ReturnsJson(siteSettings);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{customerId}")
                    .ReturnsJson(expectedCustomer);
                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/gateways")
                    .ReturnsJson(gateways);
                server.Arrange().GetInsightApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}/insights?statuses=Open&statuses=InProgress&statuses=Acknowledged")
                    .ReturnsJson(new List<Insight>());

                server.Arrange().GetInsightApi()
                    .SetupRequest(HttpMethod.Post, $"insights/statistics")
                    .ReturnsJson(new InsightStatisticsResponse() { StatisticsByPriority = new List<SiteInsightStatistics>() });

                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"siteStatistics?siteIds={site.Id}")
                    .ReturnsJson(new List<SiteTicketStatistics>());

                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}/tickets")
                    .ReturnsJson(new List<Ticket>());


                foreach (var connector in gateways.SelectMany(g => g.Connectors))
                {
                    server.Arrange().GetConnectorApi()
                        .SetupRequest(HttpMethod.Get, $"connectors/{connector.Id}/logs/latest?count=1&includeErrors={true}&source=Connector")
                        .ReturnsJson(new ConnectorLogRecord[] { expectedConnectorLogRecords[connector.Id] });
                }

                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}/points/count")
                    .ReturnsJson(new CountResponse { Count = 1000 });


                var response = await client.GetAsync($"sites/{siteId}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<SiteDetailDto>();
                var expectedSiteDetailDto = SiteDetailDto.Map(site, server.Assert().GetImageUrlHelper());
                expectedSiteDetailDto.Features = SiteFeaturesDto.Map(siteDirectoryCoreDB.Features);
				expectedSiteDetailDto.ArcGisLayers = ArcGisLayerDto.Map(siteDirectoryCoreDB.ArcGisLayers);
                expectedSiteDetailDto.Settings = SiteSettingsDto.Map(siteSettings);
                expectedSiteDetailDto.IsOnline = true;
				expectedSiteDetailDto.WebMapId = siteDirectoryCoreDB.WebMapId;
                result.Should().BeEquivalentTo(expectedSiteDetailDto);
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_GetSite_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var response = await client.GetAsync($"sites/{siteId}");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }
    }
}
