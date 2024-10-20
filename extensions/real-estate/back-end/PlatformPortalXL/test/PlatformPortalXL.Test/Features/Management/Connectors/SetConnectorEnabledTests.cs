using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Moq.Contrib.HttpClient;
using PlatformPortalXL.Models;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Management.Connectors
{
    public class SetConnectorEnabledTests: BaseInMemoryTest
    {
        public SetConnectorEnabledTests(ITestOutputHelper output) : base(output) { }
        
        [Fact]
        public async Task UserHasNoAccess_SetConnectorEnabled_ReturnForbidden()
        {
            var siteId = Guid.NewGuid();
            var connectorId = Guid.NewGuid();
            
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite( Guid.NewGuid(), Permissions.ManageConnectors, siteId))
            {
                var response = await client.PutAsync($"sites/{siteId}/connectors/{connectorId}/isEnabled?enabled=true", null);
                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }
        [Fact]
        public async Task ParameterMissed_SetConnectorEnabled_ReturnBadRequest()
        {
            var siteId = Guid.NewGuid();
            var connectorId = Guid.NewGuid();
            
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite( Guid.NewGuid(), Permissions.ManageConnectors, siteId))
            {
                var response = await client.PutAsync($"sites/{siteId}/connectors/{connectorId}/isEnabled", null);
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }
        
        [Fact]
        public async Task EnableConnector_SetConnectorEnabled_EnablesConnector()
        {
            var siteId = Guid.NewGuid();
            var connectorId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite( Guid.NewGuid(), Permissions.ManageConnectors, siteId))
            {
                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Post, $"connectors/{connectorId}/enable")
                    .ReturnsResponse(HttpStatusCode.NoContent);
                var response = await client.PutAsync($"sites/{siteId}/connectors/{connectorId}/isEnabled?enabled=true", null);
                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }
        
        [Fact]
        public async Task DisableConnector_SetConnectorEnabled_DisablesConnector()
        {
            var siteId = Guid.NewGuid();
            var connectorId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite( Guid.NewGuid(), Permissions.ManageConnectors, siteId))
            {
                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Post, $"connectors/{connectorId}/disable")
                    .ReturnsResponse(HttpStatusCode.NoContent);
                var response = await client.PutAsync($"sites/{siteId}/connectors/{connectorId}/isEnabled?enabled=false", null);
                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }
    }
}