using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq.Contrib.HttpClient;
using PlatformPortalXL.Models;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

using Willow.Api.DataValidation;

namespace PlatformPortalXL.Test.Features.Management.Connectors
{
    public class StopConnectorScanTests: BaseInMemoryTest
    {
        public StopConnectorScanTests(ITestOutputHelper output) : base(output) { }
        
        [Fact]
        public async Task UserHasNoAccess_StopConnectorScan_ReturnForbidden()
        {
            var siteId = Guid.NewGuid();
            var connectorId = Guid.NewGuid();
            var scanId = Guid.NewGuid();
            
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite( Guid.NewGuid(), Permissions.ManageConnectors, siteId))
            {
                var response = await client.PostAsync($"sites/{siteId}/connectors/{connectorId}/scans/{scanId}/stop", null);
                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }
        
        [Theory]
        [InlineData(ScanStatus.Failed)]
        [InlineData(ScanStatus.Finished)]
        public async Task ScanIsCompleted_StopConnectorScan_ReturnUnprocessableEntity(ScanStatus completedStatus)
        {
            var siteId = Guid.NewGuid();
            var connectorId = Guid.NewGuid();
            var scanId = Guid.NewGuid();
            var scan = Fixture.Build<ConnectorScan>()
                .With(x => x.Status, completedStatus)
                .Create();
            
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite( Guid.NewGuid(), Permissions.ManageConnectors, siteId))
            {
                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"connectors/{connectorId}/scans/{scanId}")
                    .ReturnsJson(scan);
                var response = await client.PostAsync($"sites/{siteId}/connectors/{connectorId}/scans/{scanId}/stop", null);
                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var error = await response.Content.ReadAsAsync<ValidationError>();
                error.Items.Should().HaveCount(1);
                error.Items.First().Message.Should().Be("Only New and Scanning requests may be stopped.");
            }
        }
        
        [Theory]
        [InlineData(ScanStatus.New)]
        [InlineData(ScanStatus.Scanning)]
        public async Task ScanIsNotCompleted_StopConnectorScan_StopScan(ScanStatus incompletedStatus)
        {
            var siteId = Guid.NewGuid();
            var connectorId = Guid.NewGuid();
            var scanId = Guid.NewGuid();
            var scan = Fixture.Build<ConnectorScan>()
                .With(x => x.Status, incompletedStatus)
                .Create();
            
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite( Guid.NewGuid(), Permissions.ManageConnectors, siteId))
            {
                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"connectors/{connectorId}/scans/{scanId}")
                    .ReturnsJson(scan);
                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Patch, $"connectors/{connectorId}/scans/{scanId}/stop")
                    .ReturnsResponse(HttpStatusCode.NoContent);
                var response = await client.PostAsync($"sites/{siteId}/connectors/{connectorId}/scans/{scanId}/stop", null);
                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }

    }
}