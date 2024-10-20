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
    public class GetAllSitesInspectionsTests : BaseInMemoryTest
    {
        public GetAllSitesInspectionsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermissionForSites_ReturnsForbidden()
        {
            var userId = Guid.NewGuid();
            var customerId = Guid.NewGuid();

            var expectedUser = Fixture.Build<User>()
                .With(x => x.Id, userId)
                .With(x => x.CustomerId, customerId)
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, userId))
            {
                var directoryApiHandler = server.Arrange().GetDirectoryApi();
                var siteApiHandler = server.Arrange().GetSiteApi();

                directoryApiHandler.SetupRequest(HttpMethod.Get, $"users/{userId}")
                    .ReturnsJson(expectedUser);
                directoryApiHandler.SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(new List<Site> { });

                var response = await client.GetAsync($"inspections");
                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task SitesInspectionsExist_GetAllSitesInspections_ReturnsInspections()
        {
            var utcNow = DateTime.UtcNow;
            var userId = Guid.NewGuid();
            var customerId = Guid.NewGuid();

            var expectedDigitalTiwnAsset = Fixture.CreateMany<AssetMinimum>(1);

            var expectedUser = Fixture.Build<User>()
                .With(x => x.Id, userId)
                .With(x => x.CustomerId, customerId)
                .Create();

            var expectedSites = Fixture.Build<Site>()
                                       .With(x => x.CustomerId, customerId)
                                       .With(x => x.Features, new SiteFeatures { IsInspectionEnabled = true })
                                       .CreateMany().ToList();

            var expectedInspections = new Dictionary<Guid, List<Inspection>>();
            var expectedInspectionZones = new Dictionary<Guid, List<InspectionZone>>();

            foreach (var site in expectedSites)
            {
                expectedInspections[site.Id] = Fixture.Build<Inspection>()
                    .With(x => x.AssetId, expectedDigitalTiwnAsset.First().Id)
                    .CreateMany()
                    .ToList();

                expectedInspectionZones[site.Id] = expectedInspections[site.Id].Select(i =>
                                                Fixture.Build<InspectionZone>()
                                                        .With(z => z.Id, i.ZoneId)
                                                        .Create())
                                                        .ToList();
            }


            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, userId))
            {
                var directoryApiHandler = server.Arrange().GetDirectoryApi();
                var siteApiHandler = server.Arrange().GetSiteApi();

                directoryApiHandler.SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(expectedSites);

                foreach (var site in expectedSites)
                {
                    server.Arrange().GetWorkflowApi()
                        .SetupRequest(HttpMethod.Get, $"sites/{site.Id}/inspections")
                        .ReturnsJson(expectedInspections[site.Id]);

                    server.Arrange().GetWorkflowApi()
                        .SetupRequest(HttpMethod.Get, $"sites/{site.Id}/zones?includeStatistics=False")
                        .ReturnsJson(expectedInspectionZones[site.Id]);

                    server.Arrange().GetDigitalTwinApi()
                    .SetupRequestWithExpectedBody(HttpMethod.Post, $"sites/{site.Id}/assets/names", expectedDigitalTiwnAsset.Select(t => t.Id).Distinct())
                    .ReturnsJson(expectedDigitalTiwnAsset);
                }

                var response = await client.GetAsync($"inspections");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<InspectionDto>>();
                var expectedInspectionDtos = InspectionDto.MapFromModels(expectedInspections.Values.SelectMany(i => i).ToList());
                var expectedAllZones = expectedInspectionZones.SelectMany(kv => kv.Value);
                foreach (var inspectionDto in expectedInspectionDtos)
                {
                    inspectionDto.Checks = inspectionDto.Checks.Where(c => c.Statistics.WorkableCheckStatus != CheckRecordStatus.NotRequired).ToList();
                    inspectionDto.ZoneName = expectedAllZones.First(z => z.Id == inspectionDto.ZoneId)?.Name;
                    inspectionDto.AssetName = expectedDigitalTiwnAsset.First().Name;
                    foreach (var check in inspectionDto.Checks)
                    {
                        if (check.Statistics.WorkableCheckStatus != CheckRecordStatus.Completed)
                        {
                            check.Statistics.LastCheckSubmittedEntry = string.Empty;
                            check.Statistics.LastCheckSubmittedUserId = default;
                        }
                        var startDate = check.PauseStartDate;
                        var endDate = check.PauseEndDate;
                        check.IsPaused = startDate?.CompareTo(endDate) < 0 && utcNow.CompareTo(endDate) <= 0;
                    }
                }
                expectedInspectionDtos.RemoveAll(i => i.Checks.All(c => c.IsPaused));
                result.Should().BeEquivalentTo(expectedInspectionDtos);
            }
        }

        [Fact]
        public async Task SitesInspectionsExist_GetAllSitesInspections_ByScopeId_ReturnsInspections()
        {
            var utcNow = DateTime.UtcNow;
            var userId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var scopeId = Guid.NewGuid().ToString();
            var expectedSites = Fixture.Build<Site>()
                .With(x => x.CustomerId, customerId)
                .With(x => x.Features, new SiteFeatures { IsInspectionEnabled = true })
                .CreateMany(4).ToList();
            var expectedTwinDto = expectedSites.Skip(2).Select(c=>Fixture.Build<TwinDto>().With(x=>x.SiteId,c.Id).Create()).ToList();

            var expectedDigitalTiwnAsset = Fixture.CreateMany<AssetMinimum>(1);





            var expectedInspections = new Dictionary<Guid, List<Inspection>>();
            var expectedInspectionZones = new Dictionary<Guid, List<InspectionZone>>();

            foreach (var twin in expectedTwinDto)
            {
                expectedInspections[twin.SiteId.Value] = Fixture.Build<Inspection>()
                    .With(x => x.AssetId, expectedDigitalTiwnAsset.First().Id)
                    .CreateMany()
                    .ToList();

                expectedInspectionZones[twin.SiteId.Value] = expectedInspections[twin.SiteId.Value].Select(i =>
                                                Fixture.Build<InspectionZone>()
                                                        .With(z => z.Id, i.ZoneId)
                                                        .Create())
                                                        .ToList();
            }


            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, userId))
            {
                server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
                    .ReturnsJson(expectedTwinDto);

                var directoryApiHandler = server.Arrange().GetDirectoryApi();

                directoryApiHandler.SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(expectedSites);

                foreach (var twin in expectedTwinDto)
                {
                    server.Arrange().GetWorkflowApi()
                        .SetupRequest(HttpMethod.Get, $"sites/{twin.SiteId}/inspections")
                        .ReturnsJson(expectedInspections[twin.SiteId.Value]);

                    server.Arrange().GetWorkflowApi()
                        .SetupRequest(HttpMethod.Get, $"sites/{twin.SiteId}/zones?includeStatistics=False")
                        .ReturnsJson(expectedInspectionZones[twin.SiteId.Value]);

                    server.Arrange().GetDigitalTwinApi()
                    .SetupRequestWithExpectedBody(HttpMethod.Post, $"sites/{twin.SiteId}/assets/names", expectedDigitalTiwnAsset.Select(t => t.Id).Distinct())
                    .ReturnsJson(expectedDigitalTiwnAsset);
                }

                var response = await client.GetAsync($"inspections?scopeId={scopeId}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<InspectionDto>>();
                var expectedInspectionDtos = InspectionDto.MapFromModels(expectedInspections.Values.SelectMany(i => i).ToList());
                var expectedAllZones = expectedInspectionZones.SelectMany(kv => kv.Value);
                foreach (var inspectionDto in expectedInspectionDtos)
                {
                    inspectionDto.Checks = inspectionDto.Checks.Where(c => c.Statistics.WorkableCheckStatus != CheckRecordStatus.NotRequired).ToList();
                    inspectionDto.ZoneName = expectedAllZones.First(z => z.Id == inspectionDto.ZoneId)?.Name;
                    inspectionDto.AssetName = expectedDigitalTiwnAsset.First().Name;
                    foreach (var check in inspectionDto.Checks)
                    {
                        if (check.Statistics.WorkableCheckStatus != CheckRecordStatus.Completed)
                        {
                            check.Statistics.LastCheckSubmittedEntry = string.Empty;
                            check.Statistics.LastCheckSubmittedUserId = default;
                        }
                        var startDate = check.PauseStartDate;
                        var endDate = check.PauseEndDate;
                        check.IsPaused = startDate?.CompareTo(endDate) < 0 && utcNow.CompareTo(endDate) <= 0;
                    }
                }
                expectedInspectionDtos.RemoveAll(i => i.Checks.All(c => c.IsPaused));
                result.Should().BeEquivalentTo(expectedInspectionDtos);
            }
        }

        [Fact]
        public async Task SitesInspectionsExist_GetAllSitesInspections_ByScopeId_userHasNoAccess_ReturnsForbidden()
        {
            var userId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var scopeId = Guid.NewGuid().ToString();
            var expectedSites = Fixture.Build<Site>()
                .With(x => x.CustomerId, customerId)
                .With(x => x.Features, new SiteFeatures { IsInspectionEnabled = true })
                .CreateMany(4).ToList();
            var expectedTwinDto = Fixture.Build<TwinDto>().CreateMany(2).ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, userId))
            {
                server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
                    .ReturnsJson(expectedTwinDto);

                var directoryApiHandler = server.Arrange().GetDirectoryApi();

                directoryApiHandler.SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(expectedSites);

                var response = await client.GetAsync($"inspections?scopeId={scopeId}");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            }
        }

        [Fact]
        public async Task SitesInspectionsExist_GetAllSitesInspections_ByInvalidScopeId_ReturnsForbidden()
        {
            var utcNow = DateTime.UtcNow;
            var userId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var scopeId = Guid.NewGuid().ToString();
            var expectedSites = Fixture.Build<Site>()
                .With(x => x.CustomerId, customerId)
                .With(x => x.Features, new SiteFeatures { IsInspectionEnabled = true })
                .CreateMany(4)
                .ToList();
            var expectedTwinDto =new List<TwinDto>();


            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, userId))
            {
                server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
                    .ReturnsJson(expectedTwinDto);

                var directoryApiHandler = server.Arrange().GetDirectoryApi();

                directoryApiHandler.SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(expectedSites);

                var response = await client.GetAsync($"inspections?scopeId={scopeId}");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            }
        }
    }
}
