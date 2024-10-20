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

namespace PlatformPortalXL.Test.Features.Workflow.Reporters
{
    public class DeleteReporterTests : BaseInMemoryTest
    {
        public DeleteReporterTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task ReporterExists_DeleteReporter_ReturnsSuccess()
        {
            var siteId = Guid.NewGuid();
            var reporterId = Guid.NewGuid();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Delete, $"sites/{siteId}/reporters/{reporterId}")
                    .ReturnsResponse(HttpStatusCode.NoContent);

                var response = await client.DeleteAsync($"sites/{siteId}/reporters/{reporterId}");

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_DeleteReporter_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var response = await client.DeleteAsync($"sites/{siteId}/reporters/{Guid.NewGuid()}");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

    }
}