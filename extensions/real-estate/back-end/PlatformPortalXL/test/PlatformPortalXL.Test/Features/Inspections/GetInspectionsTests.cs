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
using Willow.Platform.Users;
using Willow.Tests.Infrastructure;
using Willow.Workflow;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Inspections
{
    public class GetInspectionsTests : BaseInMemoryTest
    {
        public GetInspectionsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_GetInspections_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var response = await client.GetAsync($"sites/{siteId}/inspectionZones/{Guid.NewGuid()}/inspections");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task InspectionZoneNotExist_GetInspections_ReturnsNotFound()
        {
            var userId = Guid.NewGuid();
            var siteId = Guid.NewGuid();

            var userSites = Fixture.Build<Site>()
               .With(x => x.Id, siteId)
               .With(x => x.Features, new SiteFeatures() { IsInspectionEnabled = true })
               .CreateMany(1).ToList();

            var inspectionZoneId = Guid.NewGuid();
            var inspectionZones = Fixture.CreateMany<InspectionZone>(3);

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, userId))
            {
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(userSites);

                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Post, $"zones/bySiteIds")
                    .ReturnsJson(inspectionZones);

                var response = await client.GetAsync($"sites/{siteId}/inspectionZones/{inspectionZoneId}/inspections");

                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task InspectionsExist_GetInspections_ReturnsZoneInspectionsList()
        {
            var utcNow = DateTime.UtcNow;
            var userId = Guid.NewGuid();
            var siteId = Guid.NewGuid();

            var userSites = Fixture.Build<Site>()
               .With(x => x.Id, siteId)
               .With(x => x.Features, new SiteFeatures() { IsInspectionEnabled = true })
               .CreateMany(1).ToList();

            var zoneId = Guid.NewGuid();
            var inspectionZones = Fixture.Build<InspectionZone>()
                                         .With(z => z.Id, zoneId)
                                         .With(z=>z.SiteId,siteId)
                                         .CreateMany(1);

            var assetTree = Fixture.Build<AssetCategory>()
                                            .Without(a => a.Categories)
                                            .CreateMany(1)
                                            .ToList();

            var assetListFromTree = assetTree.SelectMany(a => a.Assets).ToList();

            var inspections = assetListFromTree.Select(a =>
                                    Fixture.Build<Inspection>()
                                            .With(i => i.ZoneId, zoneId)
                                            .With(i => i.AssetId, a.Id)
                                            .Create())
                                            .ToList();
            var expectedAssetName = assetListFromTree.Select(a => Fixture.Build<AssetMinimum>().With(i => i.Id, a.Id).With(i => i.Name, a.Name).Create())
                .ToList();
            var workgroups = inspections.Select(i =>
                                            Fixture.Build<Workgroup>()
                                                    .With(w => w.Id, i.AssignedWorkgroupId)
                                                    .Create())
                                                    .ToList();
            var users = inspections.SelectMany(i =>
                            i.Checks.Select(c => Fixture.Build<User>()
                                                        .With(u => u.Id, c.Statistics.LastCheckSubmittedUserId)
                                                        .Create()))
                                                        .ToList();


            var expectedEquipments = assetListFromTree.Select(a => new Equipment
            {
                Id = a.EquipmentId.Value,
                FloorId = a.FloorId
            }).ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, userId))
            {
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(userSites);

                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/zones/{zoneId}/inspections")
                    .ReturnsJson(inspections);
                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Post, $"sites/{siteId}/assets/names")
                    .ReturnsJson(expectedAssetName);
                foreach (var workgroup in workgroups)
                    server.Arrange().GetWorkflowApi()
                        .SetupRequest(HttpMethod.Get, $"sites/{siteId}/workgroups/{workgroup.Id}")
                        .ReturnsJson(workgroup);

                foreach(var user in users)
                    server.Arrange().GetDirectoryApi()
                        .SetupRequest(HttpMethod.Get, $"users/{user.Id}")
                            .ReturnsJson(user);

                server.Arrange().GetWorkflowApi()
                    .SetupRequestSequence(HttpMethod.Get, $"sites/{siteId}/zones?includeStatistics=False")
                    .ReturnsJson(inspectionZones)
                    .ReturnsJson(inspectionZones);
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Post, $"zones/bySiteIds")
                    .ReturnsJson(inspectionZones);


                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/allEquipmentsWithCategory")
                    .ReturnsJson(expectedEquipments);

                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/equipments/categories")
                    .ReturnsJson(new List<Category>());

                var response = await client.GetAsync($"sites/{siteId}/inspectionZones/{zoneId}/inspections");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InspectionZoneDto>();
                result.Name.Should().Be(inspectionZones.First().Name);

                var expectedInspectionDtos = InspectionDto.MapFromModels(inspections);
                for (var i = 0; i < 3; i++)
                {
                    expectedInspectionDtos[i].ZoneName = inspectionZones.First().Name;
                    expectedInspectionDtos[i].AssignedWorkgroupName = workgroups[i].Name;
                    expectedInspectionDtos[i].AssetName = assetListFromTree[i].Name;
                    for (var j = 0; j < 3; j++)
                    {
                        expectedInspectionDtos[i].Checks[j].Statistics.LastCheckSubmittedUserName =
                            $"{users[i * 3 + j].FirstName} {users[i * 3 + j].LastName}";
                        var startDate = expectedInspectionDtos[i].Checks[j].PauseStartDate;
                        var endDate = expectedInspectionDtos[i].Checks[j].PauseEndDate;
                        expectedInspectionDtos[i].Checks[j].IsPaused = startDate?.CompareTo(endDate) < 0 && utcNow.CompareTo(endDate) <= 0;
                    }
                }

                result.Inspections.Should().BeEquivalentTo(expectedInspectionDtos);
            }
        }

        [Fact]
        public async Task InspectionsExist_GetInspections_ByScopeId_ReturnsZoneInspectionsList()
        {
            var utcNow = DateTime.UtcNow;
            var siteId = Guid.NewGuid();
            var zoneId = Guid.NewGuid();
            var scopeId = Guid.NewGuid().ToString();
            var userId = Guid.NewGuid();
            var userSites = Fixture.Build<Site>()
                .With(x => x.Features, new SiteFeatures { IsInspectionEnabled = true })
                .CreateMany(2).ToList();
            var expectedTwinDto = userSites.Select(x => Fixture.Build<TwinDto>().With(y => y.SiteId, x.Id).Create()).ToList().ToArray();

            var inspectionZones = Fixture.Build<SimpleInspectionZone>()
                                         .With(z => z.Id, zoneId)
                                         .With(z=>z.SiteId,expectedTwinDto[0].SiteId)
                                         .CreateMany(1);

            var assetTree = Fixture.Build<AssetCategory>()
                                            .Without(a => a.Categories)
                                            .CreateMany(1)
                                            .ToList();

            var assetListFromTree = assetTree.SelectMany(a => a.Assets).ToList();

            var inspections = assetListFromTree.Select(a =>
                                    Fixture.Build<Inspection>()
                                            .With(i => i.ZoneId, zoneId)
                                            .With(i => i.AssetId, a.Id)
                                            .Create())
                                            .ToList();
            var expectedAssetName = assetListFromTree.Select(a => Fixture.Build<AssetMinimum>().With(i => i.Id, a.Id).With(i => i.Name, a.Name).Create())
                .ToList();
            var workgroups = inspections.Select(i =>
                                            Fixture.Build<Workgroup>()
                                                    .With(w => w.Id, i.AssignedWorkgroupId)
                                                    .Create())
                                                    .ToList();
            var users = inspections.SelectMany(i =>
                            i.Checks.Select(c => Fixture.Build<User>()
                                                        .With(u => u.Id, c.Statistics.LastCheckSubmittedUserId)
                                                        .Create()))
                                                        .ToList();


            var expectedEquipments = assetListFromTree.Select(a => new Equipment
            {
                Id = a.EquipmentId.Value,
                FloorId = a.FloorId
            }).ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, userId))
            {
                server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
                    .ReturnsJson(expectedTwinDto);
                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Post, $"sites/{expectedTwinDto[0].SiteId}/assets/names")
                    .ReturnsJson(expectedAssetName);
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(userSites);

                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{expectedTwinDto[0].SiteId}/zones/{zoneId}/inspections")
                    .ReturnsJson(inspections);

                server.Arrange().GetWorkflowApi()
                    .SetupRequestSequence(HttpMethod.Get, $"sites/{expectedTwinDto[0].SiteId}/zones?includeStatistics=False")
                    .ReturnsJson(inspectionZones)
                    .ReturnsJson(inspectionZones);

                foreach (var workgroup in workgroups)
                    server.Arrange().GetWorkflowApi()
                        .SetupRequest(HttpMethod.Get, $"sites/{expectedTwinDto[0].SiteId}/workgroups/{workgroup.Id}")
                        .ReturnsJson(workgroup);

                foreach (var user in users)
                    server.Arrange().GetDirectoryApi()
                        .SetupRequest(HttpMethod.Get, $"users/{user.Id}")
                            .ReturnsJson(user);

                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Post, $"zones/bySiteIds")
                    .ReturnsJson(inspectionZones);


                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{expectedTwinDto[0].SiteId}/allEquipmentsWithCategory")
                    .ReturnsJson(expectedEquipments);

                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{expectedTwinDto[0].SiteId}/equipments/categories")
                    .ReturnsJson(new List<Category>());

                var response = await client.GetAsync($"sites/{siteId}/inspectionZones/{zoneId}/inspections?scopeId={scopeId}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InspectionZoneDto>();
                result.Name.Should().Be(inspectionZones.First().Name);

                var expectedInspectionDtos = InspectionDto.MapFromModels(inspections);
                for (var i = 0; i < 3; i++)
                {
                    expectedInspectionDtos[i].ZoneName = inspectionZones.First().Name;
                    expectedInspectionDtos[i].AssignedWorkgroupName = workgroups[i].Name;
                    expectedInspectionDtos[i].AssetName = assetListFromTree[i].Name;
                    for (var j = 0; j < 3; j++)
                    {
                        expectedInspectionDtos[i].Checks[j].Statistics.LastCheckSubmittedUserName =
                            $"{users[i * 3 + j].FirstName} {users[i * 3 + j].LastName}";
                        var startDate = expectedInspectionDtos[i].Checks[j].PauseStartDate;
                        var endDate = expectedInspectionDtos[i].Checks[j].PauseEndDate;
                        expectedInspectionDtos[i].Checks[j].IsPaused = startDate?.CompareTo(endDate) < 0 && utcNow.CompareTo(endDate) <= 0;
                    }
                }

                result.Inspections.Should().BeEquivalentTo(expectedInspectionDtos);
            }
        }

        [Fact]
        public async Task InspectionsExist_GetInspections_ByInvalidScopeId_ReturnsForbidden()
        {

            var siteId = Guid.NewGuid();
            var zoneId = Guid.NewGuid();
            var scopeId = Guid.NewGuid().ToString();
            var userId = Guid.NewGuid();
            var userSites = Fixture.Build<Site>()
                .With(x => x.Features, new SiteFeatures { IsInspectionEnabled = true })
                .CreateMany(2).ToList();
            var expectedTwinDto = new List<TwinDto>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, userId))
            {
                server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
                    .ReturnsJson(expectedTwinDto);

                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(userSites);

                var response = await client.GetAsync($"sites/{siteId}/inspectionZones/{zoneId}/inspections?scopeId={scopeId}");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            }
        }
        [Fact]
        public async Task InspectionsExist_GetInspections_ByScopeId_UserHasNoAccess_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            var zoneId = Guid.NewGuid();
            var scopeId = Guid.NewGuid().ToString();
            var userId = Guid.NewGuid();
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

                var response = await client.GetAsync($"sites/{siteId}/inspectionZones/{zoneId}/inspections?scopeId={scopeId}");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            }
        }
    }
}
