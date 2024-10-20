using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Features.LiveData;
using PlatformPortalXL.Models;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Willow.Platform.Models;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Connectivity.SiteConnectivity
{
    public class GetConnectorstatsTests : BaseInMemoryTest
    {
        public GetConnectorstatsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task PortfolioExists_ReturnsConnectors()
        {
            var customerId = Guid.NewGuid();
            var portfolioId = Guid.NewGuid();

            var sites = Fixture.Build<Site>()
                .With(x => x.CustomerId, customerId)
                .With(x => x.PortfolioId, portfolioId)
                .With(x => x.Status, SiteStatus.Operations)
                .With(x => x.Features, new SiteFeatures
                {
                    IsTicketingDisabled = true,
                    IsInsightsDisabled = true
                })
                .CreateMany(3).ToList();

            var connectors = sites.SelectMany(x =>
                Fixture.Build<Connector>()
                        .With(x => x.IsArchived, false)
                        .With(x => x.IsEnabled, true)
                        .With(x => x.SiteId, x.Id)
                        .With(x => x.ClientId, x.CustomerId)
                        .CreateMany(2).ToList()
             ).ToList();

            var connectorsStats = new List<ConnectorStats>();
            var expectedDtos = new List<ConnectorStatsDto>();

            foreach (var connector in connectors)
            {
                var connectorStats = Fixture.Build<ConnectorStats>().With(x => x.ConnectorId, connector.Id).Create();
                connectorsStats.Add(connectorStats);
                expectedDtos.Add(ConnectorStatsDto.MapFromModel(connector.SiteId, connector.Name, connectorStats));
            }

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnPortfolio(null, Permissions.ViewPortfolios, portfolioId))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{customerId}/sites?portfolioId={portfolioId}")
                    .ReturnsJson(sites);

                var connectorApi = server.Arrange().GetConnectorApi();
                foreach (var site in sites)
                {
                    connectorApi
                        .SetupRequest(HttpMethod.Get, $"sites/{site.Id}/connectors?includePointsCount=False")
                        .ReturnsJson(connectors.Where(x => x.SiteId == site.Id));
                }

                server.Arrange().GetLiveDataApi()
                    .SetupRequest(HttpMethod.Get, $"api/livedata/stats/connectors?clientId={customerId}")
                    .ReturnsJson(new { Data = connectorsStats });

                var response = await client.PostAsync($"customers/{customerId}/portfolio/{portfolioId}/livedata/stats/connectors",
                    JsonContent.Create(new LiveDataConnectorStatsRequest()));

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<SiteConnectorStatsDto>>();

                result.Should().BeEquivalentTo(SiteConnectorStatsDto.MapFromModels(sites.Select(x => x.Id), expectedDtos));
            }
        }

        [Fact]
        public async Task PortfolioNotAllSitesHaveConnectors_ReturnsConnectors()
        {
            var customerId = Guid.NewGuid();
            var portfolioId = Guid.NewGuid();

            var sites = Fixture.Build<Site>()
                .With(x => x.CustomerId, customerId)
                .With(x => x.PortfolioId, portfolioId)
                .With(x => x.Status, SiteStatus.Operations)
                .With(x => x.Features, new SiteFeatures
                {
                    IsTicketingDisabled = true,
                    IsInsightsDisabled = true
                })
                .CreateMany(3).ToList();

            var connectors = sites.Take(2).SelectMany(x =>
                Fixture.Build<Connector>()
                        .With(x => x.IsArchived, false)
                        .With(x => x.IsEnabled, true)
                        .With(x => x.SiteId, x.Id)
                        .With(x => x.ClientId, x.CustomerId)
                        .CreateMany(2).ToList()
             ).ToList();

            var connectorsStats = new List<ConnectorStats>();
            var expectedDtos = new List<ConnectorStatsDto>();

            foreach (var connector in connectors)
            {
                var connectorStats = Fixture.Build<ConnectorStats>().With(x => x.ConnectorId, connector.Id).Create();
                connectorsStats.Add(connectorStats);
                expectedDtos.Add(ConnectorStatsDto.MapFromModel(connector.SiteId, connector.Name, connectorStats));
            }

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnPortfolio(null, Permissions.ViewPortfolios, portfolioId))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{customerId}/sites?portfolioId={portfolioId}")
                    .ReturnsJson(sites);

                var connectorApi = server.Arrange().GetConnectorApi();
                foreach (var site in sites)
                {
                    connectorApi
                        .SetupRequest(HttpMethod.Get, $"sites/{site.Id}/connectors?includePointsCount=False")
                        .ReturnsJson(connectors.Where(x => x.SiteId == site.Id));
                }

                server.Arrange().GetLiveDataApi()
                    .SetupRequest(HttpMethod.Get, $"api/livedata/stats/connectors?clientId={customerId}")
                    .ReturnsJson(new { Data = connectorsStats });

                var response = await client.PostAsync($"customers/{customerId}/portfolio/{portfolioId}/livedata/stats/connectors",
                    JsonContent.Create(new LiveDataConnectorStatsRequest()));

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<SiteConnectorStatsDto>>();

                result.Should().BeEquivalentTo(SiteConnectorStatsDto.MapFromModels(sites.Select(x => x.Id), expectedDtos));
            }
        }

        [Fact]
        public async Task PortfolioNoConnectors_ReturnsConnectors()
        {
            var customerId = Guid.NewGuid();
            var portfolioId = Guid.NewGuid();

            var sites = Fixture.Build<Site>()
                .With(x => x.CustomerId, customerId)
                .With(x => x.PortfolioId, portfolioId)
                .With(x => x.Status, SiteStatus.Operations)
                .With(x => x.Features, new SiteFeatures
                {
                    IsTicketingDisabled = true,
                    IsInsightsDisabled = true
                })
                .CreateMany(1).ToList();

            var connectors = new List<Connector>();
            var connectorsStats = new List<ConnectorStats>();
            var expectedDtos = new List<ConnectorStatsDto>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnPortfolio(null, Permissions.ViewPortfolios, portfolioId))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{customerId}/sites?portfolioId={portfolioId}")
                    .ReturnsJson(sites);

                var connectorApi = server.Arrange().GetConnectorApi();
                foreach (var site in sites)
                {
                    connectorApi
                        .SetupRequest(HttpMethod.Get, $"sites/{site.Id}/connectors?includePointsCount=False")
                        .ReturnsJson(connectors.Where(x => x.SiteId == site.Id));
                }

                var response = await client.PostAsync($"customers/{customerId}/portfolio/{portfolioId}/livedata/stats/connectors",
                    JsonContent.Create(new LiveDataConnectorStatsRequest()));

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<SiteConnectorStatsDto>>();

                result.Should().BeEquivalentTo(SiteConnectorStatsDto.MapFromModels(sites.Select(x => x.Id), expectedDtos));
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPortfolioPermission_ReturnsForbidden()
        {
            var customerId = Guid.NewGuid();
            var portfolioId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnPortfolio(null, Permissions.ViewPortfolios, portfolioId))
            {
                var response = await client.PostAsync($"customers/{customerId}/portfolio/{portfolioId}/livedata/stats/connectors", 
                    JsonContent.Create(new LiveDataConnectorStatsRequest()));

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task SiteExists_ReturnsConnectors()
        {
            var site = Fixture.Build<Site>()
                .With(x => x.Status, SiteStatus.Operations)
                .With(x => x.Features, new SiteFeatures
                {
                    IsTicketingDisabled = true,
                    IsInsightsDisabled = true
                })
                .Create();

            var connectors = Fixture.Build<Connector>()
                        .With(x => x.IsArchived, false)
                        .With(x => x.IsEnabled, true)
                        .With(x => x.SiteId, site.Id)
                        .With(x => x.ClientId, site.CustomerId)
                        .CreateMany(2);

            var connectorsStats = new List<ConnectorStats>();
            var expectedDtos = new List<ConnectorStatsDto>();

            foreach (var connector in connectors)
            {
                var connectorStats = Fixture.Build<ConnectorStats>().With(x => x.ConnectorId, connector.Id).Create();
                connectorsStats.Add(connectorStats);
                expectedDtos.Add(ConnectorStatsDto.MapFromModel(connector.SiteId, connector.Name, connectorStats));
            }

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, site.Id))
            {
                server.Arrange().GetConnectorApi()
                        .SetupRequest(HttpMethod.Get, $"sites/{site.Id}/connectors?includePointsCount=False")
                        .ReturnsJson(connectors.Where(x => x.SiteId == site.Id));

                server.Arrange().GetLiveDataApi()
                    .SetupRequest(HttpMethod.Get, $"api/livedata/stats/connectors?clientId={site.CustomerId}")
                    .ReturnsJson(new { Data = connectorsStats });

                var response = await client.GetAsync($"sites/{site.Id}/livedata/stats/connectors");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<ConnectorStatsDto>>();

                result.Should().BeEquivalentTo(expectedDtos);
            }
        }

        [Fact]
        public async Task SiteHasNoConnectors_ReturnsConnectors()
        {
            var site = Fixture.Build<Site>()
                .With(x => x.Status, SiteStatus.Operations)
                .With(x => x.Features, new SiteFeatures
                {
                    IsTicketingDisabled = true,
                    IsInsightsDisabled = true
                })
                .Create();

            var connectors = new List<Connector>();
            var connectorsStats = new List<ConnectorStats>();
            var expectedDtos = new List<ConnectorStatsDto>();

            foreach (var connector in connectors)
            {
                var connectorStats = Fixture.Build<ConnectorStats>().With(x => x.ConnectorId, connector.Id).Create();
                connectorsStats.Add(connectorStats);
                expectedDtos.Add(ConnectorStatsDto.MapFromModel(connector.SiteId, connector.Name, connectorStats));
            }

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, site.Id))
            {
                server.Arrange().GetConnectorApi()
                        .SetupRequest(HttpMethod.Get, $"sites/{site.Id}/connectors?includePointsCount=False")
                        .ReturnsJson(connectors.Where(x => x.SiteId == site.Id));

                var response = await client.GetAsync($"sites/{site.Id}/livedata/stats/connectors");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<ConnectorStatsDto>>();

                result.Should().BeEquivalentTo(expectedDtos);
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectSitePermission_ReturnsForbidden()
        {
            var customerId = Guid.NewGuid();
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var response = await client.GetAsync($"sites/{siteId}/livedata/stats/connectors");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }
    }
}
