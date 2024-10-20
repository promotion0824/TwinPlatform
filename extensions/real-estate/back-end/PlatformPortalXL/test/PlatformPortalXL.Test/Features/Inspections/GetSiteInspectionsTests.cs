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
using Xunit;
using Xunit.Abstractions;
using Willow.Workflow;

namespace PlatformPortalXL.Test.Features.Inspections
{
    public class GetSiteInspectionsTests : BaseInMemoryTest
    {
        public GetSiteInspectionsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_GetSitenspections_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var response = await client.GetAsync($"sites/{siteId}/inspections");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task InspectonsExist_GetSiteInspections_ReturnsInspectionsList()
        {
            var utcNow = DateTime.UtcNow;
            var userId = Guid.NewGuid();
            var siteId = Guid.NewGuid();

            var userSites = Fixture.Build<Site>()
               .With(x => x.Id, siteId)
               .With(x => x.Features, new SiteFeatures() { IsInspectionEnabled = true })
               .CreateMany(1).ToList();

            var assetTree = Fixture.Build<AssetCategory>()
                                            .Without(a => a.Categories)
                                            .CreateMany(1)
                                            .ToList();

            var assetListFromTree = assetTree.SelectMany(a => a.Assets).ToList();

            var inspections = assetListFromTree.Select(a =>
                                    Fixture.Build<Inspection>()
                                            .With(i => i.AssetId, a.Id)
                                            .Create())
                                            .ToList();
            var workgroups = inspections.Select(i =>
                                            Fixture.Build<Workgroup>()
                                                    .With(w => w.Id, i.AssignedWorkgroupId)
                                                    .With(w => w.Name, "The Flintstones")
                                                    .Create())
                                                    .ToList();
            var users = inspections.SelectMany(i =>
                            i.Checks.Select(c => Fixture.Build<User>()
                                                        .With(u => u.Id, c.Statistics.LastCheckSubmittedUserId)
                                                        .Create()))
                                                        .ToList();
            var zones = inspections.Select(i =>
                                            Fixture.Build<InspectionZone>()
                                                    .With(z => z.Id, i.ZoneId)
                                                    .Create())
                                                    .ToList();

            var expectedEquipments = assetListFromTree.Select(a => new Equipment
            {
                Id = a.EquipmentId.Value,
                FloorId = a.FloorId,
                Tags = Fixture.CreateMany<Tag>(3).ToList(),
                PointTags = Fixture.CreateMany<Tag>(3).ToList()
            }).ToList();
            var expectedAssetName = assetListFromTree.Select(a => Fixture.Build<AssetMinimum>().With(i => i.Id, a.Id).With(i => i.Name, a.Name).Create())
                .ToList();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, userId))
            {
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(userSites);
                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Post, $"sites/{siteId}/assets/names")
                    .ReturnsJson(expectedAssetName);
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/inspections")
                    .ReturnsJson(inspections);

                foreach(var workgroup in workgroups)
                    server.Arrange().GetWorkflowApi()
                        .SetupRequest(HttpMethod.Get, $"sites/{siteId}/workgroups/{workgroup.Id}")
                        .ReturnsJson(workgroup);

                foreach(var user in users)
                    server.Arrange().GetDirectoryApi()
                        .SetupRequest(HttpMethod.Get, $"users/{user.Id}")
                            .ReturnsJson(user);

                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/zones?includeStatistics=False")
                    .ReturnsJson(zones);

                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/allEquipmentsWithCategory")
                    .ReturnsJson(expectedEquipments);

                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/equipments/categories")
                    .ReturnsJson(new List<Category>());

                var response = await client.GetAsync($"sites/{siteId}/inspections");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<InspectionDto>>();

                var expectedInspections = InspectionDto.MapFromModels(inspections);
                var pausedInspections = new List<InspectionDto>();
                for (var i = 0; i < 3; i++)
                {
                    expectedInspections[i].ZoneName = zones[i].Name;
                    expectedInspections[i].AssignedWorkgroupName = workgroups[i].Name;
                    expectedInspections[i].AssetName = assetListFromTree[i].Name;
                    expectedInspections[i].Checks = expectedInspections[i].Checks.Where(c => c.Statistics.WorkableCheckStatus != CheckRecordStatus.NotRequired).ToList();
                    for (var j = 0; j < expectedInspections[i].Checks.Count; j++)
                    {
                        if (expectedInspections[i].Checks[j].Statistics.WorkableCheckStatus != CheckRecordStatus.Completed)
                        {
                            expectedInspections[i].Checks[j].Statistics.LastCheckSubmittedEntry = string.Empty;
                            expectedInspections[i].Checks[j].Statistics.LastCheckSubmittedUserId = default;
                        }
                        else
                        {
                            expectedInspections[i].Checks[j].Statistics.LastCheckSubmittedUserName =
                                $"{users[i * 3 + j].FirstName} {users[i * 3 + j].LastName}";
                        }
                        var startDate = expectedInspections[i].Checks[j].PauseStartDate;
                        var endDate = expectedInspections[i].Checks[j].PauseEndDate;
                        expectedInspections[i].Checks[j].IsPaused = startDate?.CompareTo(endDate) < 0 && utcNow.CompareTo(endDate) <= 0;
                    }
                    if (expectedInspections[i].Checks.Count == expectedInspections[i].Checks.Count(c => c.IsPaused))
                    {
                        pausedInspections.Add(expectedInspections[i]);
                    }
                }
                foreach (var pi in pausedInspections)
                {
                    expectedInspections.Remove(pi);
                }

                result.Should().BeEquivalentTo(expectedInspections);
            }
        }

        [Fact]
        public async Task InspectionsExist_GetSiteInspections_byScopeId_ReturnsInspectionsList()
        {
            var utcNow = DateTime.UtcNow;
            var siteId = Guid.NewGuid();
            var scopeId = Guid.NewGuid().ToString();
            var userId = Guid.NewGuid();

            var userSites = Fixture.Build<Site>()
                .With(x => x.Features, new SiteFeatures { IsInspectionEnabled = true })
                .CreateMany(2).ToList();

            var expectedTwinDto = Fixture.Build<TwinDto>().CreateMany(2).ToList();
            expectedTwinDto[0].SiteId = userSites[0].Id;

            var assetTree = Fixture.Build<AssetCategory>()
                                            .Without(a => a.Categories)
                                            .CreateMany(1)
                                            .ToList();

            var assetListFromTree = assetTree.SelectMany(a => a.Assets).ToList();

            var inspections = assetListFromTree.Select(a =>
                                    Fixture.Build<Inspection>()
                                            .With(i => i.AssetId, a.Id)
                                            .Create())
                                            .ToList();
            var expectedAssetName = assetListFromTree.Select(a => Fixture.Build<AssetMinimum>().With(i => i.Id, a.Id).With(i=>i.Name,a.Name).Create())
                .ToList();
            var workgroups = inspections.Select(i =>
                                            Fixture.Build<Workgroup>()
                                                    .With(w => w.Id, i.AssignedWorkgroupId)
                                                    .With(w => w.Name, "The Flintstones")
                                                    .Create())
                                                    .ToList();
            var users = inspections.SelectMany(i =>
                            i.Checks.Select(c => Fixture.Build<User>()
                                                        .With(u => u.Id, c.Statistics.LastCheckSubmittedUserId)
                                                        .Create()))
                                                        .ToList();
            var zones = inspections.Select(i =>
                                            Fixture.Build<InspectionZone>()
                                                    .With(z => z.Id, i.ZoneId)
                                                    .Create())
                                                    .ToList();

            var expectedEquipments = assetListFromTree.Select(a => new Equipment
            {
                Id = a.EquipmentId.Value,
                FloorId = a.FloorId,
                Tags = Fixture.CreateMany<Tag>(3).ToList(),
                PointTags = Fixture.CreateMany<Tag>(3).ToList()
            }).ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, siteId))
            {

                server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
                    .ReturnsJson(expectedTwinDto);
                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Post, $"sites/{expectedTwinDto[0].SiteId}/assets/names")
                    .ReturnsJson(expectedAssetName);
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(userSites);

                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{expectedTwinDto[0].SiteId}/inspections")
                    .ReturnsJson(inspections);

                foreach (var workgroup in workgroups)
                    server.Arrange().GetWorkflowApi()
                        .SetupRequest(HttpMethod.Get, $"sites/{expectedTwinDto[0].SiteId}/workgroups/{workgroup.Id}")
                        .ReturnsJson(workgroup);

                foreach (var user in users)
                    server.Arrange().GetDirectoryApi()
                        .SetupRequest(HttpMethod.Get, $"users/{user.Id}")
                            .ReturnsJson(user);

                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{expectedTwinDto[0].SiteId}/zones?includeStatistics=False")
                    .ReturnsJson(zones);

                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{expectedTwinDto[0].SiteId}/allEquipmentsWithCategory")
                    .ReturnsJson(expectedEquipments);

                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{expectedTwinDto[0].SiteId}/equipments/categories")
                    .ReturnsJson(new List<Category>());

                var response = await client.GetAsync($"sites/{siteId}/inspections?scopeId={scopeId}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<InspectionDto>>();

                var expectedInspections = InspectionDto.MapFromModels(inspections);
                var pausedInspections = new List<InspectionDto>();
                for (var i = 0; i < 3; i++)
                {
                    expectedInspections[i].ZoneName = zones[i].Name;
                    expectedInspections[i].AssignedWorkgroupName = workgroups[i].Name;
                    expectedInspections[i].AssetName = assetListFromTree[i].Name;
                    expectedInspections[i].Checks = expectedInspections[i].Checks.Where(c => c.Statistics.WorkableCheckStatus != CheckRecordStatus.NotRequired).ToList();
                    for (var j = 0; j < expectedInspections[i].Checks.Count; j++)
                    {
                        if (expectedInspections[i].Checks[j].Statistics.WorkableCheckStatus != CheckRecordStatus.Completed)
                        {
                            expectedInspections[i].Checks[j].Statistics.LastCheckSubmittedEntry = string.Empty;
                            expectedInspections[i].Checks[j].Statistics.LastCheckSubmittedUserId = default;
                        }
                        else
                        {
                            expectedInspections[i].Checks[j].Statistics.LastCheckSubmittedUserName =
                                $"{users[i * 3 + j].FirstName} {users[i * 3 + j].LastName}";
                        }
                        var startDate = expectedInspections[i].Checks[j].PauseStartDate;
                        var endDate = expectedInspections[i].Checks[j].PauseEndDate;
                        expectedInspections[i].Checks[j].IsPaused = startDate?.CompareTo(endDate) < 0 && utcNow.CompareTo(endDate) <= 0;
                    }
                    if (expectedInspections[i].Checks.Count == expectedInspections[i].Checks.Count(c => c.IsPaused))
                    {
                        pausedInspections.Add(expectedInspections[i]);
                    }
                }
                foreach (var pi in pausedInspections)
                {
                    expectedInspections.Remove(pi);
                }

                result.Should().BeEquivalentTo(expectedInspections);
            }
        }


        [Fact]
        public async Task InspectionsExist_GetSiteInspections_byScopeId_UserHasNoAccess_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            var scopeId = Guid.NewGuid().ToString();
            var userId = Guid.NewGuid();

            var expectedTwinDto = Fixture.Build<TwinDto>().CreateMany(2).ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
                    .ReturnsJson(expectedTwinDto);

                var response = await client.GetAsync($"sites/{siteId}/inspections?scopeId={scopeId}");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            }
        }
        [Fact]
        public async Task InspectionsExist_GetSiteInspections_byInvalidScopeId_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            var scopeId = Guid.NewGuid().ToString();
            var userId = Guid.NewGuid();
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

                var response = await client.GetAsync($"sites/{siteId}/inspections?scopeId={scopeId}");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            }
        }
    }
}
