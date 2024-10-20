using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting.Server;
using MobileXL.Dto;
using MobileXL.Models;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace MobileXL.Test.Features.Zones
{
    public class GetInspectionsTests : BaseInMemoryTest
    {
        public GetInspectionsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GetInspections_ReturnsInspections()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            {
                var userId = Guid.NewGuid();
                var expectedZones = Fixture
                    .Build<InspectionZone>()
                    .Without(x => x.Statistics)
                    .CreateMany(1)
                    .ToList();
                var expectedInspectionRecords = Fixture
                    .Build<InspectionRecord>()
                    .Without(x => x.Inspection)
                    .CreateMany(1)
                    .ToList();
                var expectedInspections = Fixture
                    .Build<Inspection>()
                    .Without(x => x.LastRecord)
                    .CreateMany(1)
                    .ToList();

                var expectedZoneDtos = ZoneDto.Map(expectedZones);
                expectedZoneDtos[0].Inspections = InspectionDto.Map(expectedInspections, server.Arrange().GetImageUrlHelper()).ToList();
                expectedInspectionRecords[0].Inspection = expectedInspections[0];
                expectedZoneDtos[0].Inspections[0].InspectionRecords = InspectionRecordDto.Map(expectedInspectionRecords, server.Arrange().GetImageUrlHelper()).ToList();

                var expectedSites = Fixture
                    .Build<Models.Site>()
                    .Without(x => x.Customer)
                    .Without(x => x.Features)
                    .CreateMany(1)
                    .ToList();
                var expectedSiteDtos = SiteDto.Map(expectedSites);
                expectedSiteDtos[0].InspectionZones = expectedZoneDtos;

                var expectedInspectionsDto = Fixture
                    .Build<InspectionsDto>()
                    .With(x => x.Sites, expectedSiteDtos)
                    .Create();

                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(expectedSites);

                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{expectedSites[0].Id}/users/{userId}/zones?includeStatistics=true")
                    .ReturnsJson(expectedZones);

                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{expectedSites[0].Id}/users/{userId}/zones/{expectedZones[0].Id}/inspections")
                    .ReturnsJson(expectedInspections);

                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{expectedSites[0].Id}/inspections/{expectedInspections[0].Id}/lastRecord")
                    .ReturnsJson(expectedInspectionRecords[0]);
                
                using (var client = server.CreateClientWithCustomerUserPermissionOnSite(userId, Permissions.ViewSites, expectedSites[0].Id))
                {
                    var response = await client.GetAsync($"inspections");

                    response.StatusCode.Should().Be(HttpStatusCode.OK);
                    var result = await response.Content.ReadAsAsync<InspectionsDto>();

                    result.Sites[0].InspectionZones[0].Inspections[0].InspectionRecords[0].Id
                        .Should().Be(expectedInspectionsDto.Sites[0].InspectionZones[0].Inspections[0].InspectionRecords[0].Id);
                }
            }
        }

        [Fact]
        public async Task UserDoesNotHavePermissionToSites_GetInspections_ReturnsNoSites()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            {
                var userId = Guid.NewGuid();
                var sites = new List<Models.Site>();
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(sites);

                using (var client = server.CreateClientWithCustomerUserDeniedPermissionOnSite(userId, Permissions.ViewSites, Guid.NewGuid()))
                {
                    var response = await client.GetAsync($"inspections");
                    response.StatusCode.Should().Be(HttpStatusCode.OK);
                    var result = await response.Content.ReadAsAsync<InspectionsDto>();
                    result.Sites.Should().BeEquivalentTo(sites);
                }
            }
        }
    }
}