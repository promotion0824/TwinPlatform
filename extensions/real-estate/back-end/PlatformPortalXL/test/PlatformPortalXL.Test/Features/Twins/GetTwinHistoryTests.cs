using FluentAssertions;
using PlatformPortalXL.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using Moq.Contrib.HttpClient;
using PlatformPortalXL.Dto;

namespace PlatformPortalXL.Test.Features.Twins
{
    public class GetTwinHistoryTests : BaseInMemoryTest
    {
        public GetTwinHistoryTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task InvalidTwinId_GetTwinHistory_ReturnsNotFound()
        {
            var siteId = Guid.NewGuid();
            var twinId = "dummy";

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"admin/sites/{siteId}/twins/{twinId}/history")
                    .ReturnsResponse(HttpStatusCode.NotFound);

                var response = await client.GetAsync($"sites/{siteId}/twins/{twinId}/history");
                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task ValidInput_GetTwinHistory_ReturnsTwinHistory()
        {
            var siteId = Guid.NewGuid();
            var twinId = "twin123";

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionsOnSite(null, new[] { Permissions.ViewSites, Permissions.ManageSites }, siteId))
            {
                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"admin/sites/{siteId}/twins/{twinId}/history")
                    .ReturnsJson(new TwinHistoryDto());

                var response = await client.GetAsync($"sites/{siteId}/twins/{twinId}/history");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }
    }
}
