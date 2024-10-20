using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Features.Connectivity;
using PlatformPortalXL.Models;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;
using PlatformPortalXL.ServicesApi.InsightApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Platform.Models;
using Willow.Platform.Users;
using Willow.Tests.Infrastructure;
using Willow.Workflow;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Connectivity.SiteConnectivity
{
    public class GetConnectivitySummaryTests : BaseInMemoryTest
    {
        public GetConnectivitySummaryTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task UserHasAccessToSites_ReturnsSummary()
        {
            var userId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var expectedSites = Fixture.Build<Site>()
                                       .With(x => x.CustomerId, customerId)
                                       .With(x => x.Features, new SiteFeatures())
                                       .CreateMany(3);

            var expectedUser = Fixture.Build<User>()
                .With(x => x.Id, userId)
                .With(x => x.CustomerId, customerId)
                .Create();

            var expectedSiteGateways = new Dictionary<Guid, List<Gateway>>();
            var expectedConnectivityStatics = new List<ConnectivityStatistics>();
            var expectedConnectorLogRecords = new Dictionary<Guid, ConnectorLogRecord>();
            int i = 0;

            foreach (var expectedSite in expectedSites)
            {
                var gateways = new List<Gateway>
                {
                    Fixture.Build<Gateway>()
                        .With(x => x.IsEnabled, true)
                        .With(x => x.IsOnline, true)
                        .With(x => x.SiteId, expectedSite.Id)
                        .With(x => x.CustomerId, customerId)
                        .With(x => x.Connectors, Fixture.Build<Connector>()
                            .With(x => x.IsEnabled, true)
                            .With(x => x.SiteId, expectedSite.Id)
                            .With(x => x.ClientId, customerId)
                            .CreateMany(2)
                            .ToList())
                        .Create()
                };

                foreach (var connector in gateways.Single().Connectors)
                {
                    expectedConnectorLogRecords[connector.Id] = Fixture.Build<ConnectorLogRecord>().
                        With(x => x.ConnectorId, connector.Id)
                        .Create();
                }

                expectedSiteGateways[expectedSite.Id] = gateways;
                expectedConnectivityStatics.Add(
                    new ConnectivityStatistics {
                        connectors =  gateways.Single().Connectors,
                        gateways = gateways,
                        SiteId = expectedSite.Id
                    }
                );
                i++;
            }

            var expectedOutput = CreateExpectedOutput(expectedSites, expectedSiteGateways, expectedConnectorLogRecords);

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, userId))
            {
                var directoryApiHandler = server.Arrange().GetDirectoryApi();
                var connectorApiHandler = server.Arrange().GetConnectorApi();
                var workflowApiHandler = server.Arrange().GetWorkflowApi();
                var siteApiHandler = server.Arrange().GetSiteApi();

                directoryApiHandler
                    .SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(expectedSites);

                directoryApiHandler.SetupRequest(HttpMethod.Get, $"users/{userId}")
                    .ReturnsJson(expectedUser);

                siteApiHandler.SetupRequest(HttpMethod.Get, $"customers/{expectedUser.CustomerId}/sites")
                    .ReturnsJson(expectedSites);

                var expectedInsightStatics = new List<SiteInsightStatistics>();
                var expectedSiteStatics = new List<SiteTicketStatistics>();

                foreach (var site in expectedSites)
                {
                    directoryApiHandler.SetupRequest(HttpMethod.Get, $"sites/{site.Id}/features/useAzureDigitalTwins")
                        .ReturnsJson(new { UseAzureDigitalTwins = true });

                    directoryApiHandler.SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
                        .ReturnsJson(site);

                    expectedInsightStatics.Add(new SiteInsightStatistics
                    {
                        Id = site.Id,
                        OpenCount = 1,
                        UrgentCount = 1,
                        HighCount = 2,
                        MediumCount = 4
                    });

                    expectedSiteStatics.Add(new SiteTicketStatistics()
                    {
                        Id = site.Id,
                        OverdueCount = 1,
                        UrgentCount = 1,
                        HighCount = 2,
                        MediumCount = 4
                    });

                    connectorApiHandler
                        .SetupRequest(HttpMethod.Get, $"sites/{site.Id}/gateways")
                        .ReturnsJson(expectedSiteGateways[site.Id]);

                    server.Arrange().GetDigitalTwinApi()
                        .SetupRequest(HttpMethod.Get, $"sites/{site.Id}/points/count")
                        .ReturnsJson(new CountResponse { Count = 1000 });

                    foreach (var connector in expectedSiteGateways[site.Id].SelectMany(g => g.Connectors))
                    {
                        connectorApiHandler
                            .SetupRequest(HttpMethod.Get, $"connectors/{connector.Id}/logs/latest?count=1&includeErrors={true}&source=Connector")
                            .ReturnsJson(new ConnectorLogRecord[] { expectedConnectorLogRecords[connector.Id] });
                    }
                }

                server.Arrange().GetInsightApi()
                        .SetupRequest(HttpMethod.Post, $"insights/statistics")
                        .ReturnsJson(new InsightStatisticsResponse() {  StatisticsByPriority = expectedInsightStatics });
                workflowApiHandler.SetupRequest(HttpMethod.Get, $"siteStatistics?siteIds={expectedSites.ToList()[0].Id}&siteIds={expectedSites.ToList()[1].Id}&siteIds={expectedSites.ToList()[2].Id}")
                        .ReturnsJson(expectedSiteStatics);
                connectorApiHandler.SetupRequest(HttpMethod.Get, $"siteConnectivityStatistics?siteIds={expectedSites.ToList()[0].Id}&siteIds={expectedSites.ToList()[1].Id}&siteIds={expectedSites.ToList()[2].Id}")
                        .ReturnsJson(expectedConnectivityStatics);

                var response = await client.GetAsync($"connectivity");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<ConnectivitySummaryResponse>();
                result.Should().BeEquivalentTo(expectedOutput);
            }
        }

        private ConnectivitySummaryResponse CreateExpectedOutput(IEnumerable<Site> expectedSites, Dictionary<Guid, List<Gateway>> expectedSiteGateways, Dictionary<Guid, ConnectorLogRecord> expectedConnectorLogRecords)
        {
            var models = new List<PortfolioDashboardSiteStatus>();

            foreach (var site in expectedSites)
            {
                var expectedSiteOperationsStatus = new PortfolioDashboardSiteStatus
                {
                    SiteId = site.Id,
                    Name = site.Name,
                    Country = site.Country,
                    State = site.State,
                    Suburb = site.Suburb,
                    Insights = new PortfolioDashboardSiteInsights
                    {
                        OpenCount = 1,
                        UrgentCount = 1,
                        HighCount = 2,
                        MediumCount = 4
                    },
                    Tickets = new PortfolioDashboardSiteTickets
                    {
                        OverdueCount = 1,
                        ResolvedCount = 0,
                        UnresolvedCount = 0,
                        UrgentCount = 1,
                        HighCount = 2,
                        MediumCount = 4
                    },
                    Gateways = expectedSiteGateways[site.Id].Select(g => new PortfolioDashboardGatewayStatus
                    {
                        GatewayId = g.Id,
                        Name = g.Name,
                        Status = GetGatewayStatus(g),
                        LastUpdated = g.LastUpdatedAt,
                        Connectors = g.Connectors.Select(c => new PortfolioDashboardConnectorStatus
                        {
                            ConnectorId = c.Id,
                            Name = c.Name,
                            ErrorCount = expectedConnectorLogRecords[c.Id].ErrorCount,
                            PointCount = expectedConnectorLogRecords[c.Id].PointCount,
                            LastUpdated = expectedConnectorLogRecords[c.Id].CreatedAt,
                            Status = MapStatus(c, expectedConnectorLogRecords[c.Id])
                        })
                        .ToList()
                    })
                    .ToList()
                };

                expectedSiteOperationsStatus.PointCount = 1000;
                expectedSiteOperationsStatus.Status = GetSiteStatus(site, expectedSiteOperationsStatus.Gateways);

                models.Add(expectedSiteOperationsStatus);
            }

            return ConnectivitySummaryResponse.MapFrom(models);
        }

        private ConnectivitySummaryResponse CreateEmptyExpectedOutput(IEnumerable<Site> expectedSites, Dictionary<Guid, List<Gateway>> expectedSiteGateways, Dictionary<Guid, ConnectorLogRecord> expectedConnectorLogRecords)
        {
            var models = new List<PortfolioDashboardSiteStatus>();

            foreach (var site in expectedSites)
            {
                var expectedSiteOperationsStatus = new PortfolioDashboardSiteStatus
                {
                    SiteId = site.Id,
                    Name = site.Name,
                    Country = site.Country,
                    State = site.State,
                    Suburb = site.Suburb,
                    Insights = null,
                    Tickets = null,
                    Gateways = expectedSiteGateways[site.Id].Select(g => new PortfolioDashboardGatewayStatus
                    {
                        GatewayId = g.Id,
                        Name = g.Name,
                        Status = GetGatewayStatus(g),
                        LastUpdated = g.LastUpdatedAt,
                        Connectors = g.Connectors.Select(c => new PortfolioDashboardConnectorStatus
                        {
                            ConnectorId = c.Id,
                            Name = c.Name,
                            ErrorCount = expectedConnectorLogRecords[c.Id].ErrorCount,
                            PointCount = expectedConnectorLogRecords[c.Id].PointCount,
                            LastUpdated = expectedConnectorLogRecords[c.Id].CreatedAt,
                            Status = MapStatus(c, expectedConnectorLogRecords[c.Id])
                        })
                        .ToList()
                    })
                    .ToList()
                };

                expectedSiteOperationsStatus.PointCount = 1000;
                expectedSiteOperationsStatus.Status = GetSiteStatus(site, expectedSiteOperationsStatus.Gateways);

                models.Add(expectedSiteOperationsStatus);
            }

            return ConnectivitySummaryResponse.MapFrom(models);
        }

        private static ServiceStatus GetGatewayStatus(Gateway gateway) =>
            gateway.IsEnabled
                ? gateway.IsOnline.GetValueOrDefault() ? ServiceStatus.Online : ServiceStatus.Offline
                : ServiceStatus.NotOperational;


        private static ServiceStatus GetSiteStatus(Site site, List<PortfolioDashboardGatewayStatus> gateways)
        {
            if (site.Status != SiteStatus.Operations || gateways == null || !gateways.Any())
            {
                return ServiceStatus.NotOperational;
            }

            var hasOnlineGateways = gateways.Any(g => g.Status == ServiceStatus.Online);
            var hasOnlineConnectors = gateways.SelectMany(g => g.Connectors).Select(c => c.Status).Any(s => s == ServiceStatus.Online || s == ServiceStatus.OnlineWithErrors);

            if (hasOnlineConnectors || hasOnlineGateways)
            {
                return ServiceStatus.Online;
            }

            if (hasOnlineGateways)
            {
                var connectorStatuses = gateways.Where(g => g.Status == ServiceStatus.Online).SelectMany(g => g.Connectors).Select(c => c.Status);
                if (connectorStatuses.Any(s => s == ServiceStatus.Offline))
                {
                    if (connectorStatuses.Any(s => s != ServiceStatus.Offline))
                    {
                        return ServiceStatus.OnlineWithErrors;
                    }
                    else
                    {
                        return ServiceStatus.Offline;
                    }
                }
                else if (connectorStatuses.Any(s => s == ServiceStatus.Online))
                {
                    return ServiceStatus.Online;
                }
                else
                {
                    return ServiceStatus.NotOperational;
                }
            }
            else
            {
                if (site.Status != SiteStatus.Operations || gateways.Any(g => g.Status == ServiceStatus.NotOperational))
                {
                    return ServiceStatus.NotOperational;
                }
                return ServiceStatus.Offline;
            }
        }

        private static ServiceStatus MapStatus(Connector connector, ConnectorLogRecord connectorLogRecord)
        {
            if (!connector.IsEnabled)
            {
                return ServiceStatus.NotOperational;
            }

            if (connectorLogRecord == null || connectorLogRecord.CreatedAt < DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(25)))
            {
                return ServiceStatus.Offline;
            }

            return ServiceStatus.Online;
        }

        [Fact]
        public async Task UserHasAccessToSites_InsightIsDisabled_TicketIsDisabled_ReturnsSummary()
        {
            var userId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var expectedSites = Fixture.Build<Site>()
                                       .With(x => x.CustomerId, customerId)
                                       .With(x => x.Features, new SiteFeatures())
                                       .CreateMany(3);
            expectedSites.ToList()[0].Features.IsInsightsDisabled = true;
            expectedSites.ToList()[1].Features.IsInsightsDisabled = true;
            expectedSites.ToList()[2].Features.IsInsightsDisabled = true;
            expectedSites.ToList()[0].Features.IsTicketingDisabled = true;
            expectedSites.ToList()[1].Features.IsTicketingDisabled = true;
            expectedSites.ToList()[2].Features.IsTicketingDisabled = true;

            var expectedUser = Fixture.Build<User>()
                .With(x => x.Id, userId)
                .With(x => x.CustomerId, customerId)
                .Create();

            var expectedSiteGateways = new Dictionary<Guid, List<Gateway>>();
            var expectedConnectivityStatics = new List<ConnectivityStatistics>();
            var expectedConnectorLogRecords = new Dictionary<Guid, ConnectorLogRecord>();
            int i = 0;

            foreach (var expectedSite in expectedSites)
            {
                var gateways = new List<Gateway>
                {
                    Fixture.Build<Gateway>()
                        .With(x => x.IsEnabled, true)
                        .With(x => x.IsOnline, true)
                        .With(x => x.SiteId, expectedSite.Id)
                        .With(x => x.CustomerId, customerId)
                        .With(x => x.Connectors, Fixture.Build<Connector>()
                            .With(x => x.IsEnabled, true)
                            .With(x => x.SiteId, expectedSite.Id)
                            .With(x => x.ClientId, customerId)
                            .CreateMany(2)
                            .ToList())
                        .Create()
                };

                foreach (var connector in gateways.Single().Connectors)
                {
                    expectedConnectorLogRecords[connector.Id] = Fixture.Build<ConnectorLogRecord>().
                        With(x => x.ConnectorId, connector.Id)
                        .Create();
                }

                expectedSiteGateways[expectedSite.Id] = gateways;
                expectedConnectivityStatics.Add(
                    new ConnectivityStatistics {
                        connectors =  gateways.Single().Connectors,
                        gateways = gateways,
                        SiteId = expectedSite.Id
                    }
                );
                i++;
            }

            var expectedOutput = CreateEmptyExpectedOutput(expectedSites, expectedSiteGateways, expectedConnectorLogRecords);

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, userId))
            {
                var directoryApiHandler = server.Arrange().GetDirectoryApi();
                var connectorApiHandler = server.Arrange().GetConnectorApi();
                var workflowApiHandler = server.Arrange().GetWorkflowApi();
                var siteApiHandler = server.Arrange().GetSiteApi();

                directoryApiHandler
                    .SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(expectedSites);

                directoryApiHandler.SetupRequest(HttpMethod.Get, $"users/{userId}")
                    .ReturnsJson(expectedUser);

                siteApiHandler.SetupRequest(HttpMethod.Get, $"customers/{expectedUser.CustomerId}/sites")
                    .ReturnsJson(expectedSites);

                var expectedInsightStatics = new List<SiteInsightStatistics>();
                var expectedSiteStatics = new List<SiteTicketStatistics>();

                foreach (var site in expectedSites)
                {
                    directoryApiHandler.SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
                        .ReturnsJson(site);

                    expectedInsightStatics.Add(new SiteInsightStatistics
                    {
                        Id = site.Id,
                        OpenCount = 0,
                        UrgentCount = 0
                    });

                    expectedSiteStatics.Add(new SiteTicketStatistics()
                    {
                        Id = site.Id,
                        OverdueCount = 0                    });

                    connectorApiHandler
                        .SetupRequest(HttpMethod.Get, $"sites/{site.Id}/gateways")
                        .ReturnsJson(expectedSiteGateways[site.Id]);

                    server.Arrange().GetDigitalTwinApi()
                        .SetupRequest(HttpMethod.Get, $"sites/{site.Id}/points/count")
                        .ReturnsJson(new CountResponse { Count = 1000 });

                    foreach (var connector in expectedSiteGateways[site.Id].SelectMany(g => g.Connectors))
                    {
                        connectorApiHandler
                            .SetupRequest(HttpMethod.Get, $"connectors/{connector.Id}/logs/latest?count=1&includeErrors={true}&source=Connector")
                            .ReturnsJson(new ConnectorLogRecord[] { expectedConnectorLogRecords[connector.Id] });
                    }
                }

                server.Arrange().GetInsightApi().SetupRequest(HttpMethod.Get, $"siteStatistics?siteIds={expectedSites.ToList()[0].Id}&siteIds={expectedSites.ToList()[1].Id}&siteIds={expectedSites.ToList()[2].Id}")
                        .ReturnsJson(expectedInsightStatics);
                workflowApiHandler.SetupRequest(HttpMethod.Get, $"siteStatistics?siteIds={expectedSites.ToList()[0].Id}&siteIds={expectedSites.ToList()[1].Id}&siteIds={expectedSites.ToList()[2].Id}")
                        .ReturnsJson(expectedSiteStatics);
                connectorApiHandler.SetupRequest(HttpMethod.Get, $"siteConnectivityStatistics?siteIds={expectedSites.ToList()[0].Id}&siteIds={expectedSites.ToList()[1].Id}&siteIds={expectedSites.ToList()[2].Id}")
                        .ReturnsJson(expectedConnectivityStatics);

                var response = await client.GetAsync($"connectivity");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<ConnectivitySummaryResponse>();
                result.Should().BeEquivalentTo(expectedOutput);
            }
        }
    }
}
