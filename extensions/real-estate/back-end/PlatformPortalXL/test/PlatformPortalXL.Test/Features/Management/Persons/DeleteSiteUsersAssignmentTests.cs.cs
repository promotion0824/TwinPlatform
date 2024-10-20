using AutoFixture;
using FluentAssertions;
using Moq.Contrib.HttpClient;
using PlatformPortalXL.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Platform.Models;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Management.Persons
{
    public class DeleteSiteUsersAssignmentTests : BaseInMemoryTest
    {
        public DeleteSiteUsersAssignmentTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_DeleteSiteUsersAssignment_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            var personId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ManageUsers, siteId))
            {
                var response = await client.DeleteAsync($"sites/{siteId}/persons/{personId}/assignments");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task SiteDoesNotExist_DeleteSiteUsersAssignment_ReturnsNoContent()
        {
            var siteId = Guid.NewGuid();
            var personId = Guid.NewGuid();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageUsers, siteId))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}")
                    .ReturnsJson((Site)null);

                var response = await client.DeleteAsync($"sites/{siteId}/persons/{personId}/assignments");

                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task ValidInput_DeleteCustomerUser_ReturnsNoContent()
        {
            var site = Fixture.Create<Site>();
            var personId = Guid.NewGuid();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageUsers, site.Id))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
                    .ReturnsJson(site);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Delete, $"users/{personId}/permissionAssignments?resourceId={site.Id}")
                    .ReturnsResponse(HttpStatusCode.NoContent);

                var response = await client.DeleteAsync($"sites/{site.Id}/persons/{personId}/assignments");

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }
    }
}
