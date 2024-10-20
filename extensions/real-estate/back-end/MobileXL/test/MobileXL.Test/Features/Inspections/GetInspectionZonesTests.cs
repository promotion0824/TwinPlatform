using System;
using System.Collections.Generic;
using System.Linq;
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
    public class GetInspectionZonesTests : BaseInMemoryTest
    {
        public GetInspectionZonesTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task ZonesExist_GetInspectionZones_ReturnsZones()
        {
            var siteId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var expectedZones = Fixture
                .Build<InspectionZone>()
                .CreateMany(3)
                .ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithCustomerUserPermissionOnSite(userId, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/users/{userId}/zones?includeStatistics=true")
                    .ReturnsJson(expectedZones);

                var response = await client.GetAsync($"sites/{siteId}/inspectionZones");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<InspectionZoneDto>>();

                result.Should().BeEquivalentTo(InspectionZoneDto.Map(expectedZones));
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_GetInspectionZones_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithCustomerUserDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var response = await client.GetAsync($"sites/{siteId}/inspectionZones");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }
    }
}