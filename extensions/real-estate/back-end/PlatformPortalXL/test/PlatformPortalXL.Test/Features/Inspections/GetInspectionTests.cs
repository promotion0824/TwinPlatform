using FluentAssertions;
using PlatformPortalXL.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using AutoFixture;
using PlatformPortalXL.Dto;
using System.Linq;
using Willow.Platform.Users;
using Willow.Workflow;
using Moq.Contrib.HttpClient;
using PlatformPortalXL.Features.Pilot;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;
using PlatformPortalXL.ServicesApi.DirectoryApi;
using Willow.Platform.Models;

namespace PlatformPortalXL.Test.Features.Inspections
{
    public class GetInspectionTests : BaseInMemoryTest
    {
        public GetInspectionTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_GetInspection_ReturnsForbidden()
        {
            var userId = Guid.NewGuid();
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(userId, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(new List<Site>());
                var response = await client.GetAsync($"inspections/{Guid.NewGuid()}");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task InspectionNotExist_GetInspection_ReturnsNotFound()
        {
            var userId = Guid.NewGuid();
            var siteId = Guid.NewGuid();
            var inspectionId = Guid.NewGuid();
            var userSites = Fixture.Build<Site>()
                .With(x => x.Features, new SiteFeatures { IsInspectionEnabled = true })
                .CreateMany(2).ToList();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(userSites);
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"inspections/{inspectionId}")
                    .ReturnsJson((Inspection)null);

                var response = await client.GetAsync($"inspections/{inspectionId}");

                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task InspectionExist_GetInspection_ReturnsInspection()
        {
            var userId = Guid.NewGuid();
            var utcNow = DateTime.UtcNow;

            var inspectionId = Guid.NewGuid();
            var userSites = Fixture.Build<Site>()
                .With(x => x.Features, new SiteFeatures { IsInspectionEnabled = true })
                .CreateMany(2).ToList();
            var asset = Fixture.Build<DigitalTwinAsset>()
                                .Create();

            var inspection = Fixture.Build<Inspection>()
                                            .With(i => i.AssetId, asset.Id)
                                            .With(c=>c.SiteId,userSites.First().Id)
                                            .Create();
            var workgroups = Fixture.Build<Workgroup>()
                                            .With(w => w.Id, inspection.AssignedWorkgroupId)
                                            .CreateMany(1);

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, userSites.First().Id))
            {
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(userSites);
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"inspections/{inspectionId}")
                    .ReturnsJson(inspection);

                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{inspection.SiteId}/workgroups")
                    .ReturnsJson(workgroups);

                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{inspection.SiteId}/assets/{asset.Id}")
                    .ReturnsJson(asset);

                var response = await client.GetAsync($"inspections/{inspectionId}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InspectionDto>();

                var expectedInspectionDto = InspectionDto.MapFromModel(inspection);
                expectedInspectionDto.AssignedWorkgroupName = workgroups.First().Name;
                expectedInspectionDto.AssetName = asset.Name;
                expectedInspectionDto.Checks.ForEach(c => c.IsPaused =
                                                    c.PauseStartDate?.CompareTo(c.PauseEndDate) < 0 &&
                                                    utcNow.CompareTo(c.PauseEndDate) <= 0);

                result.Should().BeEquivalentTo(expectedInspectionDto);
            }
        }

        [Fact]
        public async Task InspectionExist_ButAssetNotExist_GetInspection_ReturnsInspectionWithEmptyAssetName()
        {

            var userId = Guid.NewGuid();
            var utcNow = DateTime.UtcNow;

            var inspectionId = Guid.NewGuid();
            var userSites = Fixture.Build<Site>()
                .With(x => x.Features, new SiteFeatures { IsInspectionEnabled = true })
                .CreateMany(2).ToList();

            var inspection = Fixture.Build<Inspection>()
                .With(c => c.SiteId, userSites.First().Id)
                .Create();
            var workgroups = Fixture.Build<Workgroup>()
                                            .With(w => w.Id, inspection.AssignedWorkgroupId)
                                            .CreateMany(1);

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, inspection.SiteId))
            {
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(userSites);
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"inspections/{inspectionId}")
                    .ReturnsJson(inspection);

                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{inspection.SiteId}/workgroups")
                    .ReturnsJson(workgroups);

                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{inspection.SiteId}/assets/{inspection.AssetId}")
                    .ReturnsResponse(HttpStatusCode.NotFound);

                var response = await client.GetAsync($"inspections/{inspectionId}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InspectionDto>();

                var expectedInspectionDto = InspectionDto.MapFromModel(inspection);
                expectedInspectionDto.AssignedWorkgroupName = workgroups.First().Name;
                expectedInspectionDto.Checks.ForEach(c => c.IsPaused =
                                                    c.PauseStartDate?.CompareTo(c.PauseEndDate) < 0 &&
                                                    utcNow.CompareTo(c.PauseEndDate) <= 0);

                result.Should().BeEquivalentTo(expectedInspectionDto);
            }
        }

        [Fact]
        public async Task InspectionExist_GetInspection_ByScopeId_ReturnsInspection()
        {
            var userId = Guid.NewGuid();
            var scopeId= Guid.NewGuid();
            var utcNow = DateTime.UtcNow;

            var inspectionId = Guid.NewGuid();
            var userSites = Fixture.Build<Site>()
                .With(x => x.Features, new SiteFeatures { IsInspectionEnabled = true })
                .CreateMany(2).ToList();
            var expectedTwinDto = userSites.Select(x => Fixture.Build<TwinDto>().With(y => y.SiteId, x.Id).Create()).ToList();
            expectedTwinDto[0].SiteId = userSites[0].Id;
            var asset = Fixture.Build<DigitalTwinAsset>()
                                .Create();

            var inspection = Fixture.Build<Inspection>()
                                            .With(i => i.AssetId, asset.Id)
                                            .With(c => c.SiteId, userSites.First().Id)
                                            .Create();
            var workgroups = Fixture.Build<Workgroup>()
                                            .With(w => w.Id, inspection.AssignedWorkgroupId)
                                            .CreateMany(1);

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, userSites.First().Id))
            {
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(userSites);

                server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
                    .ReturnsJson(expectedTwinDto);
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"inspections/{inspectionId}")
                    .ReturnsJson(inspection);

                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{inspection.SiteId}/workgroups")
                    .ReturnsJson(workgroups);

                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{inspection.SiteId}/assets/{asset.Id}")
                    .ReturnsJson(asset);

                var response = await client.GetAsync($"inspections/{inspectionId}?scopeId={scopeId}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InspectionDto>();

                var expectedInspectionDto = InspectionDto.MapFromModel(inspection);
                expectedInspectionDto.AssignedWorkgroupName = workgroups.First().Name;
                expectedInspectionDto.AssetName = asset.Name;
                expectedInspectionDto.Checks.ForEach(c => c.IsPaused =
                                                    c.PauseStartDate?.CompareTo(c.PauseEndDate) < 0 &&
                                                    utcNow.CompareTo(c.PauseEndDate) <= 0);

                result.Should().BeEquivalentTo(expectedInspectionDto);
            }
        }

        [Fact]
        public async Task InspectionExist_UserHasNoPermission_ReturnsForbidden()
        {
            var userId = Guid.NewGuid();

            var inspectionSiteId = Guid.NewGuid();
            var inspectionId = Guid.NewGuid();
            var userSites = Fixture.Build<Site>()
                .With(x => x.Features, new SiteFeatures { IsInspectionEnabled = true })
                .CreateMany(2).ToList();
            var asset = Fixture.Build<Asset>()
                                .Without(a => a.EquipmentId)
                                .Create();

            var inspection = Fixture.Build<Inspection>()
                                            .With(i => i.AssetId, asset.Id)
                                            .With(c => c.SiteId, inspectionSiteId)
                                            .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, userSites.First().Id))
            {
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(userSites);
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"inspections/{inspectionId}")
                    .ReturnsJson(inspection);


                var response = await client.GetAsync($"inspections/{inspectionId}");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            }
        }
    }
}
