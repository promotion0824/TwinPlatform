using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Moq.Contrib.HttpClient;
using PlatformPortalXL.Features.Management;
using PlatformPortalXL.Models;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Management.Connectors
{
    public class UpdateConnectorScanTests : BaseInMemoryTest
    {
        public UpdateConnectorScanTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public async Task UserHasAccess_UpdateConnectorScan_Update()
        {
            var siteId = Guid.NewGuid();
            var connectorId = Guid.NewGuid();
            var scanId = Guid.NewGuid();
            var request = new UpdateConnectorScanRequest
            {
                Started = DateTime.Parse("2020-11-01"),
                Finished = DateTime.Parse("2020-11-03"),
                Status = ScanStatus.Failed,
                ErrorCount = 2,
                ErrorMessage = "Something bad happened"
            };
            
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite( Guid.NewGuid(), Permissions.ManageConnectors, siteId))
            {
                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Patch,
                        $"connectors/{connectorId}/scans/{scanId}?status={request.Status}" +
                        $"&errorMessage={request.ErrorMessage}" +
                        $"&errorCount={request.ErrorCount}" +
                        $"&startTime={request.Started:yyyy-MM-dd'T'HH:mm:ss'Z'}" +
                        $"&endTime={request.Finished:yyyy-MM-dd'T'HH:mm:ss'Z'}")
                    .ReturnsResponse(HttpStatusCode.NoContent);
                    
                var response = await client.PutAsJsonAsync($"sites/{siteId}/connectors/{connectorId}/scans/{scanId}", request);
                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }

    }
}