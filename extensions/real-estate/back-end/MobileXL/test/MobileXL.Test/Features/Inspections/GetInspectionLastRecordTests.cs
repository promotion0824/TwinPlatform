using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
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
    public class GetInspectionLastRecordTests : BaseInMemoryTest
    {
        public GetInspectionLastRecordTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task InspectionRecordExists_GetInspectionLastRecord_ReturnsThisInspectionRecord()
        {
            var siteId = Guid.NewGuid();
            var inspectionId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var checks = Fixture.Build<Check>()
                                .Without(x => x.PauseStartDate)
                                .Without(x => x.PauseEndDate)
                                .Without(x => x.LastSubmittedRecord)
                                .CreateMany()
                                .ToList();
            var inspection = Fixture.Build<Inspection>()
                                    .Without(x => x.LastRecord)
                                    .With(x => x.Checks, checks)
                                    .Create();
            var checkRecords = Fixture.Build<CheckRecord>()
                                .With(x => x.InspectionId, inspectionId)
                                .CreateMany(3)
                                .ToList();
            var expectedInspectionRecord = Fixture.Build<InspectionRecord>()
                                                  .With(x => x.Inspection, inspection)
                                                  .With(x => x.CheckRecords, checkRecords)
                                                  .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithCustomerUserPermissionOnSite(userId, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/inspections/{inspectionId}/lastRecord")
                    .ReturnsJson(expectedInspectionRecord);

                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Post, $"sites/assets/names")
                    .ReturnsJson(Fixture.Build<TwinSimpleResponse>().CreateMany(1));

                var response = await client.GetAsync($"sites/{siteId}/inspections/{inspectionId}/lastRecord");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InspectionRecordDto>();
                result.Should().BeEquivalentTo(InspectionRecordDto.Map(expectedInspectionRecord, server.Arrange().GetImageUrlHelper()));
            }
        }

        [Fact]
        public async Task SomeChecksArePaused_GetInspectionLastRecord_PausedChecksAndCheckRecordsShouldNotBeReturned()
        {
            var utcNow = DateTime.UtcNow;
            var siteId = Guid.NewGuid();
            var inspectionId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var checks = Fixture.Build<Check>()
                                .With(x => x.PauseStartDate, utcNow.AddDays(-1))
                                .Without(x => x.PauseEndDate)
                                .CreateMany()
                                .ToList();
            var inspection = Fixture.Build<Inspection>()
                                    .Without(x => x.LastRecord)
                                    .With(x => x.Checks, checks)
                                    .Create();
            var checkRecords = checks.Select(c => Fixture.Build<CheckRecord>()
                                                         .With(x => x.CheckId, c.Id)
                                                         .Without(x => x.Attachments)
                                                         .Create()
                                            )
                                     .ToList();
            var inspectionRecord = Fixture.Build<InspectionRecord>()
                                          .With(x => x.Inspection, inspection)
                                          .With(x => x.CheckRecords, checkRecords)
                                          .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithCustomerUserPermissionOnSite(userId, Permissions.ViewSites, siteId))
            {
                server.Arrange().SetCurrentDateTime(utcNow);
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/inspections/{inspectionId}/lastRecord")
                    .ReturnsJson(inspectionRecord);

                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Post, $"sites/assets/names")
                    .ReturnsJson(Fixture.Build<TwinSimpleResponse>().CreateMany(1));

                var response = await client.GetAsync($"sites/{siteId}/inspections/{inspectionId}/lastRecord");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InspectionRecordDto>();
                result.Inspection.Checks.Should().BeEmpty();
                result.CheckRecords.Should().BeEmpty();
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_GetInspectionLastRecord_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            var inspectionId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithCustomerUserDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var response = await client.GetAsync($"sites/{siteId}/inspections/{inspectionId}/lastRecord");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }
    }
}
