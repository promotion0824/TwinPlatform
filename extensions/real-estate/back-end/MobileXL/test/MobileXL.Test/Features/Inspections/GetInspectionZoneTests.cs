using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using MobileXL.Dto;
using MobileXL.Models;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace MobileXL.Test.Features.Zones
{
    public class GetInspectionZoneTests : BaseInMemoryTest
    {
        public GetInspectionZoneTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task ZonesExist_GetInspectionZone_ReturnsZones()
        {
            var siteId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var expectedZone = Fixture.Create<InspectionZone>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithCustomerUserPermissionOnSite(userId, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/users/{userId}/zones/{expectedZone.Id}?includeStatistics=true")
                    .ReturnsJson(expectedZone);

                var response = await client.GetAsync($"sites/{siteId}/inspectionZones/{expectedZone.Id}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InspectionZoneDto>();

                result.Should().BeEquivalentTo(InspectionZoneDto.Map(expectedZone));
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_GetInspectionZone_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithCustomerUserDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var response = await client.GetAsync($"sites/{siteId}/inspectionZones/{Guid.NewGuid()}");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }
    }
}