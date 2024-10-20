using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using NSubstitute.ReturnsExtensions;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Management.Connectors
{
    public class GetConnectorsTests : BaseInMemoryTest
    {
        public GetConnectorsTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public async Task UserHasNoAccess_GetConnectors_ReturnForbidden()
        {
            var siteId = Guid.NewGuid();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(Guid.NewGuid(), Permissions.ManageConnectors, siteId))
            {
                var response = await client.GetAsync($"sites/{siteId}/connectors");
                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task ValidInput_GetConnectors_ReturnConnectors()
        {
            var siteId = Guid.NewGuid();

            var connectors = new List<Connector>
            {
                Fixture.Build<Connector>()
                                  .WithAutoProperties()
                                  .With(x => x.IsArchived, true)
                                  .With(x => x.IsEnabled, true)
                                  .Create(),

                Fixture.Build<Connector>()
                                  .WithAutoProperties()
                                  .With(x => x.IsArchived, false)
                                  .With(x => x.IsEnabled, false)
                                  .Create(),

                Fixture.Build<Connector>()
                                  .WithAutoProperties()
                                  .With(x => x.IsArchived, false)
                                  .With(x => x.IsEnabled, true)
                                  .Create()
            };

            var expectedConnectorLogRecords = new Dictionary<Guid, ConnectorLogRecord>();
            var expectedLastUpdated = DateTime.UtcNow;

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(Guid.NewGuid(), Permissions.ManageConnectors, siteId))
            {
                var connectorApiHandler = server.Arrange().GetConnectorApi();

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

                    connectorApiHandler.SetupRequest(HttpMethod.Get, $"connectors/{connector.Id}/logs/latest?count=1&includeErrors={true}&source=Connector")
                                       .ReturnsJson(new ConnectorLogRecord[] { expectedConnectorLogRecords[connector.Id] });

                    server.Arrange().GetConnectorApi()
                                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/connectors?includePointsCount={true}")
                                    .ReturnsJson(connectors);
                }

                var response = await client.GetAsync($"sites/{siteId}/connectors");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<ConnectorDto>>();
                result[0].Status.Should().Be(ServiceStatus.Archived);
                result[1].Status.Should().Be(ServiceStatus.NotOperational);
                result[2].Status.Should().Be(ServiceStatus.Online);
            }
        }

        [Fact]
        public async Task UserHasNoAccess_GetConnector_ReturnForbidden()
        {
            var siteId = Guid.NewGuid();
            var connectorId = Guid.NewGuid();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(Guid.NewGuid(), Permissions.ManageConnectors, siteId))
            {
                var response = await client.GetAsync($"sites/{siteId}/connectors/{connectorId}");
                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task ConnectorExists_GetConnector_ReturnConnectors()
        {
            var siteId = Guid.NewGuid();
            var connector = Fixture.Create<Connector>();

            var expectedLastUpdated = DateTime.UtcNow;
            var expectedConnectorLogRecord = Fixture.Build<ConnectorLogRecord>()
                                                   .With(x => x.ConnectorId, connector.Id)
                                                   .With(x => x.StartTime, expectedLastUpdated)
                                                   .With(x => x.CreatedAt, expectedLastUpdated)
                                                   .With(x => x.EndTime, expectedLastUpdated.AddMinutes(5))
                                                   .With(x => x.PointCount, 1000)
                                                   .With(x => x.ErrorCount, 0)
                                                   .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(Guid.NewGuid(), Permissions.ManageConnectors, siteId))
            {
                var connectorApiHandler = server.Arrange().GetConnectorApi();

                connectorApiHandler.SetupRequest(HttpMethod.Get, $"connectors/{connector.Id}/logs/latest?count=1&includeErrors={true}&source=Connector")
                    .ReturnsJson(new ConnectorLogRecord[] { expectedConnectorLogRecord });

                connectorApiHandler.SetupRequest(HttpMethod.Get, $"sites/{siteId}/connectors/{connector.Id}")
                    .ReturnsJson(connector);

                var response = await client.GetAsync($"sites/{siteId}/connectors/{connector.Id}");
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<ConnectorDto>();

                result.Id.Should().Be(connector.Id);
            }
        }
    }
}
