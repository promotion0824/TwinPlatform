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
using MobileXL.Services.Apis.DigitalTwinApi;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace MobileXL.Test.Features.Zones
{
    public class GetInspectionsByZoneIdTests : BaseInMemoryTest
    {
        public GetInspectionsByZoneIdTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task InspectionsExist_GetInspectionsByZoneId_ReturnsInspections()
        {
            var siteId = Guid.NewGuid();
            var inspectionZoneId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var expectedInspections = Fixture
                .Build<Inspection>()
                .Without(x => x.LastRecord)
                .Without(x => x.Checks)
                .With(x => x.ZoneId, inspectionZoneId)
                .CreateMany(3)
                .ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithCustomerUserPermissionOnSite(userId, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/users/{userId}/zones/{inspectionZoneId}/inspections")
                    .ReturnsJson(expectedInspections);

                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Post, $"sites/assets/names")
                    .ReturnsJson(Fixture.Build<TwinSimpleResponse>().CreateMany(3));

                var response = await client.GetAsync($"sites/{siteId}/inspectionZones/{inspectionZoneId}/inspections");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<InspectionDto>>();

                result.Should().BeEquivalentTo(InspectionDto.Map(expectedInspections, server.Arrange().GetImageUrlHelper()));
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_GetInspectionsByZoneId_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            var inspectionZoneId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithCustomerUserDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var response = await client.GetAsync($"sites/{siteId}/inspectionZones/{inspectionZoneId}/inspections");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }
    }
}
