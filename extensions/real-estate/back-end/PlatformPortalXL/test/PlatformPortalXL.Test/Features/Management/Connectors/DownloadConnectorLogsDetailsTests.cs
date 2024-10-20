using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using FluentAssertions;
using PlatformPortalXL.Models;
using PlatformPortalXL.ServicesApi.ConnectorApi;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Management.Connectors
{
    public class DownloadConnectorLogsDetailsTests : BaseInMemoryTest
    {
        public DownloadConnectorLogsDetailsTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public async Task UserHasNoAccess_DownloadConnectorLogsDetailsTests_ReturnForbidden()
        {
            var siteId = Guid.NewGuid();
            var connectorId = Guid.NewGuid();
            var logId = 5;
            
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite( Guid.NewGuid(), Permissions.ManageConnectors, siteId))
            {
                var response = await client.GetAsync($"sites/{siteId}/connectors/{connectorId}/logs/{logId}/content");
                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task UserHasAccess_DownloadConnectorLogsDetailsTests_ReturnCsvFile()
        {
            var siteId = Guid.NewGuid();
            var connectorId = Guid.NewGuid();
            var logId = 5;
            var errors = new LogErrorsCore {Errors = "First\r\nSecond"};
            
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite( Guid.NewGuid(), Permissions.ManageConnectors, siteId))
            {
                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"connectors/{connectorId}/logs/latest/{logId}/errors")
                    .ReturnsJson(errors);
                    
                var response = await client.GetAsync($"sites/{siteId}/connectors/{connectorId}/logs/{logId}/content");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                response.Content.Headers.ContentType.Should().Be(new MediaTypeHeaderValue("application/octet-stream"));
                var responseStream = await response.Content.ReadAsStreamAsync();
                using (var reader = new StreamReader(responseStream))
                {
                    var header = await reader.ReadLineAsync();
                    header.Should().Be("Messages");
                    var firstRow = await reader.ReadLineAsync();
                    firstRow.Should().Be("'First'");
                    var secondRow = await reader.ReadLineAsync();
                    secondRow.Should().Be("'Second'");
                }
            }
        }
    }
}