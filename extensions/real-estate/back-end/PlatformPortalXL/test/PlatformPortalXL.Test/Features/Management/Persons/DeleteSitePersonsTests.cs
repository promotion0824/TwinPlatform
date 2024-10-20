using System;
using Willow.Platform.Users;
using Willow.Tests.Infrastructure;
using AutoFixture;
using Xunit.Abstractions;
using Xunit;
using System.Threading.Tasks;
using PlatformPortalXL.Models;
using System.Net;
using FluentAssertions;
using System.Net.Http;
using Moq.Contrib.HttpClient;
using PlatformPortalXL.Features.Management;
using Willow.Platform.Models;
using Willow.Workflow;

namespace PlatformPortalXL.Test.Features.Management.Persons
{
    public class DeleteSitePersonsTests : BaseInMemoryTest
    {
        public DeleteSitePersonsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_DeleteSiteUsers_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            var personId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ManageUsers, siteId))
            {
                var response = await client.DeleteAsync($"sites/{siteId}/persons/{personId}?personType=0");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task ValidInput_DeleteCustomerUser_ReturnsNoContent()
        {
            var site = Fixture.Create<Site>();
            var customerUser = Fixture.Create<User>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageUsers, site.Id))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
                    .ReturnsJson(site);
                server.Arrange().GetDirectoryApi()
                    .SetupRequestWithExpectedBody(HttpMethod.Put, $"customers/{site.CustomerId}/users/{customerUser.Id}/status",
                                                    new UpdateUserStatusRequest { Status = UserStatus.Inactive })
                    .ReturnsResponse(HttpStatusCode.NoContent);

                var response = await client.DeleteAsync($"sites/{site.Id}/persons/{customerUser.Id}?personType=0");

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }  

        [Fact]
        public async Task ValidInput_DeleteReporter_ReturnsNoContent()
        {
            var site = Fixture.Create<Site>();
            var reporter = Fixture.Create<Reporter>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageUsers, site.Id))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
                    .ReturnsJson(site);
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Delete, $"sites/{site.Id}/reporters/{reporter.Id}")
                    .ReturnsResponse(HttpStatusCode.NoContent);

                var response = await client.DeleteAsync($"sites/{site.Id}/persons/{reporter.Id}?personType=2");

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }

        [Fact]
        public async Task SiteDoesNotExist_DeleteReporter_ReturnsNoContent()
        {
            var site = Fixture.Create<Site>();
            var reporter = Fixture.Create<Reporter>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageUsers, site.Id))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
                    .ReturnsJson((Site)null);

                var response = await client.DeleteAsync($"sites/{site.Id}/persons/{reporter.Id}?personType=2");

                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }
    }
}
