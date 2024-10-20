using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using PlatformPortalXL.Features.Pilot;
using Willow.Platform.Models;
using Willow.Tests.Infrastructure;
using Willow.Workflow;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Inspections
{
    public class GetInspectionZonesTests : BaseInMemoryTest
    {
        public GetInspectionZonesTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_GetInspectionZones_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var response = await client.GetAsync($"sites/{siteId}/inspectionZones");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task InspectonZonesExist_GetInspectionZones_ReturnsInspectionZonesList()
        {
            var userId = Guid.NewGuid();
            var siteId = Guid.NewGuid();
            var userSites = Fixture.Build<Site>()
               .With(x => x.Id, siteId)
               .With(x => x.Features, new SiteFeatures() { IsInspectionEnabled = true })
               .CreateMany(1).ToList();
            var expectedInspectionZones = Fixture.CreateMany<InspectionZone>().ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, userId))
            {
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(userSites);

                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/zones?includeStatistics=True")
                    .ReturnsJson(expectedInspectionZones);

                var response = await client.GetAsync($"sites/{siteId}/inspectionZones");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<InspectionZoneDto>>();

                result.Should().BeEquivalentTo(expectedInspectionZones);
            }
        }

        [Fact]
        public async Task GetInspectionZones_ByScopeId_ReturnsInspectionZonesList()
        {
            var userId = Guid.NewGuid();
            var expectedInspectionZones = Fixture.CreateMany<InspectionZone>().ToList();
            var scopeId = Guid.NewGuid().ToString();

            var userSites = Fixture.Build<Site>()
                .With(x => x.Features, new SiteFeatures{ IsInspectionEnabled = true })
                .CreateMany(2).ToList();

            var expectedTwinDto = Fixture.Build<TwinDto>().CreateMany(2).ToList();
            expectedTwinDto[0].SiteId = userSites[0].Id;

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, userId))
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{expectedTwinDto[0].SiteId}/zones?includeStatistics=True")
                    .ReturnsJson(expectedInspectionZones);

                server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
                    .ReturnsJson(expectedTwinDto);

                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(userSites);

                var response = await client.GetAsync($"sites/{expectedTwinDto[0].SiteId}/inspectionZones?scopeId={scopeId}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<InspectionZoneDto>>();

                result.Should().BeEquivalentTo(expectedInspectionZones);
            }
        }

        [Fact]
        public async Task GetInspectionZones_ByScopeId_UserHasNoAccess_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var scopeId = Guid.NewGuid().ToString();

            var userSites = Fixture.Build<Site>()
                .With(x => x.Features, new SiteFeatures { IsInspectionEnabled = true })
                .CreateMany(2).ToList();

            var expectedTwinDto = Fixture.Build<TwinDto>().CreateMany(2).ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
                    .ReturnsJson(expectedTwinDto);

                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(userSites);

                var response = await client.GetAsync($"sites/{siteId}/inspectionZones?scopeId={scopeId}");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task GetInspectionZones_ByInvalidScopeId_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var scopeId = Guid.NewGuid().ToString();
            var userSites = Fixture.Build<Site>()
                .With(x => x.Features, new SiteFeatures { IsInspectionEnabled = true })
                .CreateMany(2).ToList();
            var expectedTwinDto = new List<TwinDto>();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
                    .ReturnsJson(expectedTwinDto);
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(userSites);
                var response = await client.GetAsync($"sites/{siteId}/inspectionZones?scopeId={scopeId}");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

    }
}
