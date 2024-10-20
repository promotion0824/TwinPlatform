using AutoFixture;
using FluentAssertions;
using Moq.Contrib.HttpClient;
using PlatformPortalXL.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Management.Workgroups
{
    public class DeleteWorkgroupTests : BaseInMemoryTest
    {
        public DeleteWorkgroupTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_DeleteWorkgroup_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            var workgroupId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ManageUsers, siteId))
            {
                var response = await client.DeleteAsync($"management/sites/{siteId}/workgroups/{workgroupId}");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task WorkgroupExist_DeleteWorkgroup_ReturnsNoContent()
        {
            var siteId = Guid.NewGuid();
            var workgroupId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageUsers, siteId))
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Delete, $"sites/{siteId}/workgroups/{workgroupId}")
                    .ReturnsResponse(HttpStatusCode.NoContent);

                var response = await client.DeleteAsync($"management/sites/{siteId}/workgroups/{workgroupId}");

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }
    }
}