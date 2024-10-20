using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Willow.Platform.Models;
using Willow.Platform.Users;
using Willow.Tests.Infrastructure;
using Willow.Workflow;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Inspections
{
    public class GetAllCheckHistoryTests : BaseInMemoryTest
    {
        public GetAllCheckHistoryTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_GetAllCheckHistoryTests_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            var inspectionId = Guid.NewGuid();
            var checkId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var response = await client.GetAsync($"inspections/{inspectionId}/checks/history?siteId={siteId}&checkId={checkId}");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task InvalidDateRange_GetAllCheckHistoryTests_ReturnsBadRequest()
        {
            var siteId = Guid.NewGuid();
            var inspectionId = Guid.NewGuid();
            var checkId = Guid.NewGuid();
            var startDate = DateTime.Now;
            var endDate = startDate.AddDays(-1);
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var response = await client.GetAsync($"inspections/{inspectionId}/checks/history?siteId={siteId}&checkId={checkId}&startDate={startDate:MM/dd/yyyy}&endDate={endDate:MM/dd/yyyy}");

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task InspectionCheckExist_GetAllCheckHistoryTests_ReturnsThoseChecks()
        {
            var siteId = Guid.NewGuid();
            var inspectionId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var utcNow = DateTime.UtcNow;
            var start = HttpUtility.UrlEncode(DateTime.UtcNow.AddDays(-10).ToString("O", CultureInfo.InvariantCulture));
            var end = HttpUtility.UrlEncode(DateTime.UtcNow.AddDays(-1).ToString("O", CultureInfo.InvariantCulture));
            var users = Fixture.Build<User>()
                            .With(x => x.Id, userId)
                            .With(x => x.FirstName, "Test")
                            .With(x => x.LastName, "User")
                            .CreateMany(1)
                            .ToList();
            var userDictionary = users.ToDictionary(k => k.Id, v => $"{v.FirstName} {v.LastName}");
            var checkRecords = Fixture.Build<CheckRecordReport>()
                                    .With(x => x.SubmittedUserId, userId)
                                    .CreateMany(3)
                                    .ToList();
            var site = Fixture.Build<Site>()
                .With(x => x.Id, siteId)
                .Create();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}")
                    .ReturnsJson(site);
                server.Arrange()
                    .GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"inspections/{inspectionId}/checks/history?siteId={siteId}&customerId={site.CustomerId}&startDate={start:MM/dd/yyyy}&endDate={end:MM/dd/yyyy}")
                    .ReturnsJson(checkRecords);
                server.Arrange()
                    .GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/users")
                    .ReturnsJson(users);
                var response = await client.GetAsync($"inspections/{inspectionId}/checks/history?siteId={siteId}&startDate={start:MM/dd/yyyy}&endDate={end:MM/dd/yyyy}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<List<CheckRecordReportDto>>();

                var expectedCheckRecordsDto = CheckRecordReportDto.MapFromModels(checkRecords, server.Assert().GetImageUrlHelper(), userDictionary);

                result.Should().BeEquivalentTo(expectedCheckRecordsDto);
            }
        }
    }
}
