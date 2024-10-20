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
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Management.Connectors
{
    public class GetConnectorScansTests : BaseInMemoryTest
    {
        public GetConnectorScansTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public async Task ValidInput_GetConnectors_ConnectorScans()
        {
            var siteId = Guid.NewGuid();
            var connectorId = Guid.NewGuid();
            var scans = Fixture.Build<ConnectorScan>()
                .CreateMany(10)
                .ToList();
            
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite( Guid.NewGuid(), Permissions.ManageConnectors, siteId))
            {
                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get,
                        $"connectors/{connectorId}/scans")
                    .ReturnsJson(scans);
                    
                var response = await client.GetAsync($"sites/{siteId}/connectors/{connectorId}/scans");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var returnedLogs = await response.Content.ReadAsAsync<List<ConnectorScanDto>>();
                returnedLogs.Should().BeEquivalentTo(ConnectorScanDto.MapFrom(scans));
            }
        }
    }
}