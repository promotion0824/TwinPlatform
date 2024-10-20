using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using PlatformPortalXL.Features.Commands.Requests;

namespace PlatformPortalXL.Test.Features.Commands.Commands
{
    public class DeleteCommandTests : BaseInMemoryTest
    {
        public DeleteCommandTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task DeleteCommand_ReturnsNoContent()
        {
            var siteId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var setPointCommandId = Guid.NewGuid();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ManageSites, siteId))
            {
                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Delete, $"sites/{siteId}/setpointcommands/{setPointCommandId}")
                    .ReturnsJson<SetPointCommand>(HttpStatusCode.NoContent, null);

                var response = await client.DeleteAsync($"sites/{siteId}/commands/{setPointCommandId}");

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }

        [Fact]
        public async Task DeleteCommand_DoesNotExist_ReturnsNotFound()
        {
            var siteId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var setPointCommandId = Guid.NewGuid();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ManageSites, siteId))
            {
                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Delete, $"sites/{siteId}/setpointcommands/{setPointCommandId}")
                    .ReturnsJson<SetPointCommand>(HttpStatusCode.NotFound, null);

                var response = await client.DeleteAsync($"sites/{siteId}/commands/{setPointCommandId}");

                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermissionForSite_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var expectedRequest = Fixture.Build<UpdateCommandRequest>()
                .With(x => x.DesiredDurationMinutes, 60)
                .Create();


            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(userId, Permissions.ManageSites, siteId))
            {
                var response = await client.DeleteAsync($"sites/{siteId}/commands/{Guid.NewGuid()}");
                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }
    }
}
