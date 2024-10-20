using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Platform.Models;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Connectivity.SiteConnectivity
{
    public class GetSiteConnectorConnectivityTests : BaseInMemoryTest
    {
        public GetSiteConnectorConnectivityTests(ITestOutputHelper output) : base(output)
        {
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

            var siteId = site.Id;
            var expectedLastUpdated = DateTime.UtcNow;

            var expectedConnectorLogRecords = new Dictionary<Guid, ConnectorLogRecord>();

            var gateways = new List<Gateway>
                {
                    Fixture.Build<Gateway>()
                        .With(x => x.IsEnabled, true)
                        .With(x => x.IsOnline, true)
                        .With(x => x.SiteId, site.Id)
                        .With(x => x.CustomerId, site.CustomerId)
                        .With(x => x.Connectors, Fixture.Build<Connector>()
                            .With(x => x.IsArchived, false)
                            .With(x => x.IsEnabled, true)
                            .With(x => x.SiteId, site.Id)
                            .With(x => x.ClientId, site.CustomerId)
                            .CreateMany(2)
                            .ToList())
                        .Create()
                };

            foreach (var connector in gateways.Single().Connectors)
            {
                expectedConnectorLogRecords[connector.Id] = Fixture.Build<ConnectorLogRecord>()
                    .With(x => x.ConnectorId, connector.Id)
                    .With(x => x.StartTime, expectedLastUpdated)
                    .With(x => x.CreatedAt, expectedLastUpdated)
                    .With(x => x.EndTime, expectedLastUpdated.AddMinutes(5))
                    .With(x => x.PointCount, 1000)
                    .With(x => x.ErrorCount, 0)
                    .Create();
            }

            var expectedDtos = new List<ConnectorConnectivityDto>();

            foreach (var connector in gateways.Single().Connectors)
            {
                expectedDtos.Add(new ConnectorConnectivityDto
                {
                    Id = connector.Id,
                    Name = connector.Name,
                    ErrorCount = 0,
                    GatewayStatus = ServiceStatus.Online,
                    Status = ServiceStatus.Online,
                    History = new List<ConnectorConnectivityDataPointDto>
                    {
                        new ConnectorConnectivityDataPointDto
                        {
                            Start = PortfolioDashboardConnectorLog.MapQuarterHour(expectedConnectorLogRecords[connector.Id].StartTime),
                            End = PortfolioDashboardConnectorLog.MapQuarterHour(expectedConnectorLogRecords[connector.Id].StartTime).AddMinutes(15),
                            ErrorCount = expectedConnectorLogRecords[connector.Id].ErrorCount,
                            PointCount = expectedConnectorLogRecords[connector.Id].PointCount
                        }
                    }
                });
            }

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}")
                    .ReturnsJson(site);

                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
                    .ReturnsJson(site);

                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}/points/count")
                    .ReturnsJson(new CountResponse { Count = 1000 });

                var connectorApiHandler = server.Arrange().GetConnectorApi();

                connectorApiHandler
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}/gateways")
                    .ReturnsJson(gateways);

                foreach (var connector in gateways.Single().Connectors)
                {
                    connectorApiHandler
                        .SetupRequest(HttpMethod.Get, $"connectors/{connector.Id}/logs/latest?count=1&includeErrors={true}&source=Connector")
                        .ReturnsJson(new ConnectorLogRecord[] { expectedConnectorLogRecords[connector.Id] });

                    var uri = $"connectors/{connector.Id}/logs?start={PortfolioDashboardConnectorLog.MapQuarterHour(expectedLastUpdated.AddDays(-1)):yyyy-MM-dd'T'HH:mm:sss'Z'}&end={PortfolioDashboardConnectorLog.MapQuarterHour(expectedLastUpdated.AddMinutes(15)):yyyy-MM-dd'T'HH:mm:ss'Z'}&source=Connector";
                    connectorApiHandler
                        .SetupRequest(HttpMethod.Get, uri)
                        .ReturnsJson(new ConnectorLogRecord[] { expectedConnectorLogRecords[connector.Id] });

                }

                var response = await client.GetAsync($"connectivity/sites/{siteId}/connectors");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<ConnectorConnectivityDto>>();

                foreach (var connector in result)
                {
                    connector.History.Should().HaveCount(24 * 4 + 1);
                    connector.History = connector.History.Where(h => h.ErrorCount != 0 || h.PointCount != 0).ToList();
                }
                result.Should().BeEquivalentTo(expectedDtos);
            }
        }

        [Fact]
        public async Task SiteExistsAndConnectorIsOffline_ReturnsConnectorsWithOfflineStatus()
        {
            var site = Fixture.Build<Site>()
                .With(x => x.Status, SiteStatus.Operations)
                .With(x => x.Features, new SiteFeatures
                {
                    IsTicketingDisabled = true,
                    IsInsightsDisabled = true
                })
                .Create();

            var siteId = site.Id;
            var expectedLastUpdated = DateTime.MinValue;

            var expectedConnectorLogRecords = new Dictionary<Guid, ConnectorLogRecord>();

            var gateways = new List<Gateway>
                {
                    Fixture.Build<Gateway>()
                        .With(x => x.IsEnabled, true)
                        .With(x => x.IsOnline, true)
                        .With(x => x.SiteId, site.Id)
                        .With(x => x.CustomerId, site.CustomerId)
                        .With(x => x.Connectors, Fixture.Build<Connector>()
                            .With(x => x.IsArchived, false)
                            .With(x => x.IsEnabled, true)
                            .With(x => x.SiteId, site.Id)
                            .With(x => x.ClientId, site.CustomerId)
                            .CreateMany(2)
                            .ToList())
                        .Create()
                };

            foreach (var connector in gateways.Single().Connectors)
            {
                expectedConnectorLogRecords[connector.Id] = Fixture.Build<ConnectorLogRecord>()
                    .With(x => x.ConnectorId, connector.Id)
                    .With(x => x.CreatedAt, expectedLastUpdated)
                    .With(x => x.StartTime, expectedLastUpdated)
                    .With(x => x.EndTime, expectedLastUpdated.AddMinutes(5))
                    .With(x => x.PointCount, 1000)
                    .With(x => x.ErrorCount, 0)
                    .Create();
            }

            var expectedDtos = new List<ConnectorConnectivityDto>();

            foreach (var connector in gateways.Single().Connectors)
            {
                expectedDtos.Add(new ConnectorConnectivityDto
                {
                    Id = connector.Id,
                    Name = connector.Name,
                    ErrorCount = 0,
                    GatewayStatus = ServiceStatus.Online,
                    Status = ServiceStatus.Offline,
                    History = new List<ConnectorConnectivityDataPointDto>()
                });
            }

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}")
                    .ReturnsJson(site);

                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
                    .ReturnsJson(site);

                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}/points/count")
                    .ReturnsJson(new CountResponse { Count = 1000 });

                var connectorApiHandler = server.Arrange().GetConnectorApi();

                connectorApiHandler
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}/gateways")
                    .ReturnsJson(gateways);

                foreach (var connector in gateways.Single().Connectors)
                {
                    connectorApiHandler
                        .SetupRequest(HttpMethod.Get, $"connectors/{connector.Id}/logs/latest?count=1&includeErrors={true}&source=Connector")
                        .ReturnsJson(new ConnectorLogRecord[] { expectedConnectorLogRecords[connector.Id] });

                    var uri = $"connectors/{connector.Id}/logs?start={PortfolioDashboardConnectorLog.MapQuarterHour(DateTime.UtcNow.AddDays(-1)):yyyy-MM-dd'T'HH:mm:sss'Z'}&end={PortfolioDashboardConnectorLog.MapQuarterHour(DateTime.UtcNow.AddMinutes(15)):yyyy-MM-dd'T'HH:mm:ss'Z'}&source=Connector";
                    connectorApiHandler
                        .SetupRequest(HttpMethod.Get, uri)
                        .ReturnsJson(new ConnectorLogRecord[0]);

                }

                var response = await client.GetAsync($"connectivity/sites/{siteId}/connectors");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<ConnectorConnectivityDto>>();

                foreach (var connector in result)
                {
                    connector.History.Should().HaveCount(24 * 4 + 1);
                    connector.History = connector.History.Where(h => h.ErrorCount != 0 || h.PointCount != 0).ToList();
                }

                result.Should().BeEquivalentTo(expectedDtos);
            }
        }

        [Fact]
        public async Task SiteExistsAndConnectorHasErrors_ReturnsConnectorsWithOnlineWithErrorsStatus()
        {
            var site = Fixture.Build<Site>()
                .With(x => x.Status, SiteStatus.Operations)
                .With(x => x.Features, new SiteFeatures
                {
                    IsTicketingDisabled = true,
                    IsInsightsDisabled = true
                })
                .Create();
            var siteId = site.Id;
            var expectedLastUpdated = DateTime.UtcNow;

            var expectedConnectorLogRecords = new Dictionary<Guid, ConnectorLogRecord>();

            var gateways = new List<Gateway>
                {
                    Fixture.Build<Gateway>()
                        .With(x => x.IsEnabled, true)
                        .With(x => x.IsOnline, true)
                        .With(x => x.SiteId, site.Id)
                        .With(x => x.CustomerId, site.CustomerId)
                        .With(x => x.Connectors, Fixture.Build<Connector>()
                            .With(x => x.IsArchived, false)
                            .With(x => x.IsEnabled, true)
                            .With(x => x.SiteId, site.Id)
                            .With(x => x.ClientId, site.CustomerId)
                            .With(x => x.ErrorThreshold, 100)
                            .CreateMany(2)
                            .ToList())
                        .Create()
                };

            foreach (var connector in gateways.Single().Connectors)
            {
                expectedConnectorLogRecords[connector.Id] = Fixture.Build<ConnectorLogRecord>()
                    .With(x => x.ConnectorId, connector.Id)
                    .With(x => x.StartTime, expectedLastUpdated)
                    .With(x => x.CreatedAt, expectedLastUpdated)
                    .With(x => x.EndTime, expectedLastUpdated.AddMinutes(5))
                    .With(x => x.PointCount, 1000)
                    .With(x => x.ErrorCount, 500)
                    .Create();
            }

            var expectedDtos = new List<ConnectorConnectivityDto>();

            foreach (var connector in gateways.Single().Connectors)
            {
                expectedDtos.Add(new ConnectorConnectivityDto
                {
                    Id = connector.Id,
                    Name = connector.Name,
                    ErrorCount = 500,
                    GatewayStatus = ServiceStatus.Online,
                    Status = ServiceStatus.Online,
                    History = new List<ConnectorConnectivityDataPointDto>
                    {
                        new ConnectorConnectivityDataPointDto
                        {
                            Start = PortfolioDashboardConnectorLog.MapQuarterHour(expectedConnectorLogRecords[connector.Id].StartTime),
                            End = PortfolioDashboardConnectorLog.MapQuarterHour(expectedConnectorLogRecords[connector.Id].StartTime).AddMinutes(15),
                            ErrorCount = expectedConnectorLogRecords[connector.Id].ErrorCount,
                            PointCount = expectedConnectorLogRecords[connector.Id].PointCount
                        }
                    }
                }); ;
            }

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}")
                    .ReturnsJson(site);

                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
                    .ReturnsJson(site);

                var connectorApiHandler = server.Arrange().GetConnectorApi();

                connectorApiHandler
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}/gateways")
                    .ReturnsJson(gateways);

                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}/points/count")
                    .ReturnsJson(new CountResponse { Count = 1000 });


                foreach (var connector in gateways.Single().Connectors)
                {
                    connectorApiHandler
                        .SetupRequest(HttpMethod.Get, $"connectors/{connector.Id}/logs/latest?count=1&includeErrors={true}&source=Connector")
                        .ReturnsJson(new ConnectorLogRecord[] { expectedConnectorLogRecords[connector.Id] });

                    var uri = $"connectors/{connector.Id}/logs?start={PortfolioDashboardConnectorLog.MapQuarterHour(expectedLastUpdated.AddDays(-1)):yyyy-MM-dd'T'HH:mm:sss'Z'}&end={PortfolioDashboardConnectorLog.MapQuarterHour(expectedLastUpdated.AddMinutes(15)):yyyy-MM-dd'T'HH:mm:ss'Z'}&source=Connector";
                    connectorApiHandler
                        .SetupRequest(HttpMethod.Get, uri)
                        .ReturnsJson(new ConnectorLogRecord[] { expectedConnectorLogRecords[connector.Id] });
                }

                var response = await client.GetAsync($"connectivity/sites/{siteId}/connectors");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<ConnectorConnectivityDto>>();

                foreach (var connector in result)
                {
                    connector.History.Should().HaveCount(24 * 4 + 1);
                    connector.History = connector.History.Where(h => h.ErrorCount != 0 || h.PointCount != 0).ToList();
                }

                result.Should().BeEquivalentTo(expectedDtos);
            }
        }

        [Fact]
        public async Task SiteExistsWithNoGateways_ReturnsDefaultGateway()
        {
            var site = Fixture.Build<Site>()
                .With(x => x.Status, SiteStatus.Operations)
                .With(x => x.Features, new SiteFeatures
                {
                    IsTicketingDisabled = true,
                    IsInsightsDisabled = true
                })
                .Create();

            var siteId = site.Id;
            var expectedLastUpdated = DateTime.UtcNow;

            var expectedConnectorLogRecords = new Dictionary<Guid, ConnectorLogRecord>();

            var connectors = Fixture.Build<Connector>()
                            .With(x => x.IsArchived, false)
                            .With(x => x.IsEnabled, true)
                            .With(x => x.SiteId, site.Id)
                            .With(x => x.ClientId, site.CustomerId)
                            .CreateMany(2)
                            .ToList();

            foreach (var connector in connectors)
            {
                expectedConnectorLogRecords[connector.Id] = Fixture.Build<ConnectorLogRecord>()
                    .With(x => x.ConnectorId, connector.Id)
                    .With(x => x.StartTime, expectedLastUpdated)
                    .With(x => x.CreatedAt, expectedLastUpdated)
                    .With(x => x.EndTime, expectedLastUpdated.AddMinutes(5))
                    .With(x => x.PointCount, 1000)
                    .With(x => x.ErrorCount, 0)
                    .Create();
            }

            var expectedDtos = new List<ConnectorConnectivityDto>();

            foreach (var connector in connectors)
            {
                expectedDtos.Add(new ConnectorConnectivityDto
                {
                    Id = connector.Id,
                    Name = connector.Name,
                    ErrorCount = 0,
                    GatewayStatus = ServiceStatus.Online,
                    Status = ServiceStatus.Online,
                    History = new List<ConnectorConnectivityDataPointDto>
                    {
                        new ConnectorConnectivityDataPointDto
                        {
                            Start = PortfolioDashboardConnectorLog.MapQuarterHour(expectedConnectorLogRecords[connector.Id].StartTime),
                            End = PortfolioDashboardConnectorLog.MapQuarterHour(expectedConnectorLogRecords[connector.Id].StartTime).AddMinutes(15),
                            ErrorCount = expectedConnectorLogRecords[connector.Id].ErrorCount,
                            PointCount = expectedConnectorLogRecords[connector.Id].PointCount
                        }
                    }
                });
            }

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}")
                    .ReturnsJson(site);

                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
                    .ReturnsJson(site);

                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}/points/count")
                    .ReturnsJson(new CountResponse { Count = 1000 });

                var connectorApiHandler = server.Arrange().GetConnectorApi();

                connectorApiHandler
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}/gateways")
                    .ReturnsJson(new List<Gateway>());

                connectorApiHandler
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}/connectors?includePointsCount=True")
                    .ReturnsJson(connectors);

                foreach (var connector in connectors)
                {
                    connectorApiHandler
                        .SetupRequest(HttpMethod.Get, $"connectors/{connector.Id}/logs/latest?count=1&includeErrors={true}&source=Connector")
                        .ReturnsJson(new ConnectorLogRecord[] { expectedConnectorLogRecords[connector.Id] });

                    var uri = $"connectors/{connector.Id}/logs?start={PortfolioDashboardConnectorLog.MapQuarterHour(expectedLastUpdated.AddDays(-1)):yyyy-MM-dd'T'HH:mm:sss'Z'}&end={PortfolioDashboardConnectorLog.MapQuarterHour(expectedLastUpdated.AddMinutes(15)):yyyy-MM-dd'T'HH:mm:ss'Z'}&source=Connector";
                    connectorApiHandler
                        .SetupRequest(HttpMethod.Get, uri)
                        .ReturnsJson(new ConnectorLogRecord[] { expectedConnectorLogRecords[connector.Id] });

                }

                var response = await client.GetAsync($"connectivity/sites/{siteId}/connectors");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<ConnectorConnectivityDto>>();

                foreach (var connector in result)
                {
                    connector.History.Should().HaveCount(24 * 4 + 1);
                    connector.History = connector.History.Where(h => h.ErrorCount != 0 || h.PointCount != 0).ToList();
                }

                result.Should().BeEquivalentTo(expectedDtos);
            }
        }

        [Fact]
        public async Task SiteExistsWithDisabledConnectors_ReturnsEmptyList()
        {
            var site = Fixture.Build<Site>()
                .With(x => x.Status, SiteStatus.Operations)
                .With(x => x.Features, new SiteFeatures
                {
                    IsTicketingDisabled = true,
                    IsInsightsDisabled = true
                })
                .Create();

            var siteId = site.Id;

            var gateways = new List<Gateway>
                {
                    Fixture.Build<Gateway>()
                        .With(x => x.IsEnabled, true)
                        .With(x => x.IsOnline, true)
                        .With(x => x.SiteId, site.Id)
                        .With(x => x.CustomerId, site.CustomerId)
                        .With(x => x.Connectors, Fixture.Build<Connector>()
                            .With(x => x.IsEnabled, false)
                            .With(x => x.SiteId, site.Id)
                            .With(x => x.ClientId, site.CustomerId)
                            .With(x => x.ErrorThreshold, 100)
                            .CreateMany(2)
                            .ToList())
                        .Create()
                };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}")
                    .ReturnsJson(site);

                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
                    .ReturnsJson(site);

                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}/points/count")
                    .ReturnsJson(new CountResponse { Count = 1000 });

                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}/gateways")
                    .ReturnsJson(gateways);

                var response = await client.GetAsync($"connectivity/sites/{siteId}/connectors");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<ConnectorConnectivityDto>>();
                result.Should().BeEmpty();
            }
        }

        [Fact]
        public async Task SiteExistsWithDisabledGateway_ReturnsEmptyList()
        {
            var site = Fixture.Build<Site>()
                .With(x => x.Status, SiteStatus.Operations)
                .With(x => x.Features, new SiteFeatures
                {
                    IsTicketingDisabled = true,
                    IsInsightsDisabled = true
                })
                .Create();

            var siteId = site.Id;

            var gateways = new List<Gateway>
                {
                    Fixture.Build<Gateway>()
                        .With(x => x.IsEnabled, false)
                        .With(x => x.IsOnline, true)
                        .With(x => x.SiteId, site.Id)
                        .With(x => x.CustomerId, site.CustomerId)
                        .With(x => x.Connectors, Fixture.Build<Connector>()
                            .With(x => x.IsEnabled, false)
                            .With(x => x.SiteId, site.Id)
                            .With(x => x.ClientId, site.CustomerId)
                            .With(x => x.ErrorThreshold, 100)
                            .CreateMany(2)
                            .ToList())
                        .Create()
                };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}")
                    .ReturnsJson(site);

                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
                    .ReturnsJson(site);

                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}/points/count")
                    .ReturnsJson(new CountResponse { Count = 1000 });

                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}/gateways")
                    .ReturnsJson(gateways);

                var response = await client.GetAsync($"connectivity/sites/{siteId}/connectors");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<ConnectorConnectivityDto>>();
                result.Should().BeEmpty();
            }
        }

        [Fact]
        public async Task SiteExistsAndHasConnectorsButIsNotOperational_ReturnsEmptyList()
        {
            var site = Fixture.Build<Site>()
                .With(x => x.Status, SiteStatus.Design)
                .With(x => x.Features, new SiteFeatures
                {
                    IsTicketingDisabled = true,
                    IsInsightsDisabled = true
                })
                .Create();

            var siteId = site.Id;

            var gateways = new List<Gateway>
                {
                    Fixture.Build<Gateway>()
                        .With(x => x.IsEnabled, true)
                        .With(x => x.IsOnline, true)
                        .With(x => x.SiteId, site.Id)
                        .With(x => x.CustomerId, site.CustomerId)
                        .With(x => x.Connectors, Fixture.Build<Connector>()
                            .With(x => x.IsEnabled, true)
                            .With(x => x.SiteId, site.Id)
                            .With(x => x.ClientId, site.CustomerId)
                            .With(x => x.ErrorThreshold, 100)
                            .CreateMany(2)
                            .ToList())
                        .Create()
                };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}")
                    .ReturnsJson(site);

                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
                    .ReturnsJson(site);

                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}/points/count")
                    .ReturnsJson(new CountResponse { Count = 1000 });

                var connectorApiHandler = server.Arrange().GetConnectorApi();

                connectorApiHandler
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}/gateways")
                    .ReturnsJson(gateways);

                foreach (var connector in gateways.Single().Connectors)
                {
                    connectorApiHandler
                        .SetupRequest(HttpMethod.Get, $"connectors/{connector.Id}/logs/latest?count=1&includeErrors={true}&source=Connector")
                        .ReturnsJson(new ConnectorLogRecord[0]);

                    var uri = $"connectors/{connector.Id}/logs?start={PortfolioDashboardConnectorLog.MapQuarterHour(DateTime.UtcNow.AddDays(-1)):yyyy-MM-dd'T'HH:mm:sss'Z'}&end={PortfolioDashboardConnectorLog.MapQuarterHour(DateTime.UtcNow.AddMinutes(15)):yyyy-MM-dd'T'HH:mm:ss'Z'}&source=Connector";
                    connectorApiHandler
                        .SetupRequest(HttpMethod.Get, uri)
                        .ReturnsJson(new ConnectorLogRecord[0]);
                }

                var response = await client.GetAsync($"connectivity/sites/{siteId}/connectors");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<ConnectorConnectivityDto>>();
                result.Should().BeEmpty();
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var response = await client.GetAsync($"connectivity/sites/{siteId}/connectors");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }
    }
}
