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

namespace PlatformPortalXL.Test.Features.Inspections
{
    public class ArchiveInspectionTests : BaseInMemoryTest
    {
        public ArchiveInspectionTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_ArchiveInspection_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ManageSites, siteId))
            {
                var response = await client.PostAsync($"sites/{siteId}/inspections/{Guid.NewGuid()}/archive", null);

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task InspectionExist_ArchiveInspection_ReturnsNoContent()
        {
            var siteId = Guid.NewGuid();
            var inspectionId = Guid.NewGuid();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageSites, siteId))
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Post, $"sites/{siteId}/inspections/{inspectionId}/archive?isArchived={true}")
                    .ReturnsResponse(HttpStatusCode.NoContent);

                var response = await client.PostAsync($"sites/{siteId}/inspections/{inspectionId}/archive?isArchived={true}", null);

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }
    }
}