using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq.Contrib.HttpClient;
using PlatformPortalXL.Features.Management;
using PlatformPortalXL.Models;
using PlatformPortalXL.Requests.DirectoryCore;
using Willow.Platform.Models;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Management.Connectors
{
    public class GeneratePasswordForConnectorTests: BaseInMemoryTest
    {
        public GeneratePasswordForConnectorTests(ITestOutputHelper output) : base(output) { }
        
        [Fact]
        public async Task UserHasNoAccess_GeneratePasswordForConnector_ReturnForbidden()
        {
            var siteId = Guid.NewGuid();
            
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite( Guid.NewGuid(), Permissions.ManageConnectors, siteId))
            {
                var response = await client.PostAsJsonAsync($"sites/{siteId}/connectors/{Guid.NewGuid()}/password", new CreateConnectorRequest());
                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task UserHasAccess_GeneratePasswordForConnector_ReturnsNewPassword()
        {
            var siteId = Guid.NewGuid();
            var connectorId = Guid.NewGuid();
            Guid customerId = Guid.NewGuid();
            var site = Fixture.Build<Site>()
                .With(x => x.Id, siteId)
                .With(x => x.CustomerId, customerId)
                .Create();
            string generatedPassword = null;
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite( Guid.NewGuid(), Permissions.ManageConnectors, siteId))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}")
                    .ReturnsJson(site);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Put, $"customers/{customerId:D}/sites/{siteId:D}/connectors/{connectorId:D}/account", async message =>
                    {
                        var passwordRequest = await message.Content.ReadFromJsonAsync<CreateConnectorAccountRequest>();
                        generatedPassword = passwordRequest.Password;
                        return true;
                    })
                    .ReturnsResponse(HttpStatusCode.NoContent);

                var response = await client.PostAsJsonAsync($"sites/{siteId}/connectors/{connectorId}/password", new CreateConnectorRequest());
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<ConnectorAccountPasswordDto>();
                result.Password.Should().Be(generatedPassword);
            }
        }

    }
}