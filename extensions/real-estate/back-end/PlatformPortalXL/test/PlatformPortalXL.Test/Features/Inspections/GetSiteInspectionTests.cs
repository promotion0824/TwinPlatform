using FluentAssertions;
using PlatformPortalXL.Models;
using System;
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
using PlatformPortalXL.ServicesApi.DirectoryApi;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;

namespace PlatformPortalXL.Test.Features.Inspections
{
    public class GetSiteInspectionTests : BaseInMemoryTest
    {
        public GetSiteInspectionTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_GetInspection_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var response = await client.GetAsync($"sites/{siteId}/inspections/{Guid.NewGuid()}");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task InspectionNotExist_GetInspection_ReturnsNotFound()
        {
            var siteId = Guid.NewGuid();
            var inspectionId = Guid.NewGuid();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/inspections/{inspectionId}")
                    .ReturnsJson((Inspection)null);

                var response = await client.GetAsync($"sites/{siteId}/inspections/{inspectionId}");

                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task InspectionExist_GetInspection_ReturnsInspection()
        {
            var utcNow = DateTime.UtcNow;
            var siteId = Guid.NewGuid();
            var inspectionId = Guid.NewGuid();

            var asset = Fixture.Build<DigitalTwinAsset>()
                                .Create();

            var inspection = Fixture.Build<Inspection>()
                                            .With(i => i.AssetId, asset.Id)
                                            .With(i=>i.TwinId,asset.TwinId)
                                            .Create();
            var workgroups = Fixture.Build<Workgroup>()
                                            .With(w => w.Id, inspection.AssignedWorkgroupId)
                                            .CreateMany(1);

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/inspections/{inspectionId}")
                    .ReturnsJson(inspection);

                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/workgroups")
                    .ReturnsJson(workgroups);

                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/assets/{asset.Id}")
                    .ReturnsJson(asset);

                var response = await client.GetAsync($"sites/{siteId}/inspections/{inspectionId}");

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
            var utcNow = DateTime.UtcNow;
            var siteId = Guid.NewGuid();
            var inspectionId = Guid.NewGuid();

            var inspection = Fixture.Create<Inspection>();
            var workgroups = Fixture.Build<Workgroup>()
                                            .With(w => w.Id, inspection.AssignedWorkgroupId)
                                            .CreateMany(1);

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/inspections/{inspectionId}")
                    .ReturnsJson(inspection);

                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/workgroups")
                    .ReturnsJson(workgroups);

                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/assets/{inspection.AssetId}")
                    .ReturnsResponse(HttpStatusCode.NotFound);

                var response = await client.GetAsync($"sites/{siteId}/inspections/{inspectionId}");

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
    }
}
