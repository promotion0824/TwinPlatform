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
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Management.Connectors
{
    public class GetConnectorLogsTests : BaseInMemoryTest
    {
        public GetConnectorLogsTests(ITestOutputHelper output) : base(output) { }
        
        [Fact]
        public async Task UserHasNoAccess_GetConnectorLogs_ReturnForbidden()
        {
            var siteId = Guid.NewGuid();
            var connectorId = Guid.NewGuid();
            
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite( Guid.NewGuid(), Permissions.ManageConnectors, siteId))
            {
                var response = await client.GetAsync($"sites/{siteId}/connectors/{connectorId}/logs");
                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task UserHasAccess_GetConnectorLogs_ReturnConnectorLogs()
        {
            var siteId = Guid.NewGuid();
            var connectorId = Guid.NewGuid();
            var logs = Fixture.Build<ConnectorLogRecord>()
                .CreateMany(10)
                .ToList();
            
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite( Guid.NewGuid(), Permissions.ManageConnectors, siteId))
            {
                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get,
                        $"connectors/{connectorId}/logs/latest?count=1000&includeErrors={false}")
                    .ReturnsJson(logs);
                    
                var response = await client.GetAsync($"sites/{siteId}/connectors/{connectorId}/logs");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var returnedLogs = await response.Content.ReadAsAsync<List<ConnectorLogDto>>();
                returnedLogs.Should().BeEquivalentTo(ConnectorLogDto.MapFrom(logs));
            }
        }

    }
}