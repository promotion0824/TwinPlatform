using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Models;
using System;
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
    public class ExportAllCheckHistoryTests : BaseInMemoryTest
    {
        public ExportAllCheckHistoryTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_ExportCheckHistory_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            var inspectionId = Guid.NewGuid();
            var checkId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var response = await client.GetAsync($"inspections/{inspectionId}/checks/history/export?siteId={siteId}");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task InvalidDateRange_ExportCheckHistory_ReturnsBadRequest()
        {
            var siteId = Guid.NewGuid();
            var inspectionId = Guid.NewGuid();
            var checkId = Guid.NewGuid();
            var startString = HttpUtility.UrlEncode(DateTime.Now.ToString("O", CultureInfo.InvariantCulture));
            var endString = HttpUtility.UrlEncode(DateTime.Now.AddDays(-1).ToString("O", CultureInfo.InvariantCulture));
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var response = await client.GetAsync($"inspections/{inspectionId}/checks/history/export?siteId={siteId}&startDate={startString}&endDate={endString}");

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task InspectionCheckExist_GetCheckHistory_ReturnsCsvFile()
        {
            var siteId = Guid.NewGuid();
            var inspectionId = Guid.NewGuid();
            var checkId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var start = HttpUtility.UrlEncode(DateTime.UtcNow.AddDays(-10).ToString("O", CultureInfo.InvariantCulture));
            var end = HttpUtility.UrlEncode(DateTime.UtcNow.AddDays(-1).ToString("O", CultureInfo.InvariantCulture));
            var site = Fixture.Build<Site>()
                .With(x => x.Id, siteId)
                .Create();
            var users = Fixture.Build<User>()
                .With(x => x.Id, userId)
                .CreateMany(1)
                .ToList();
            var check = Fixture.Build<Check>()
                .With(x => x.Id, checkId)
                .With(x => x.InspectionId, inspectionId)
                .CreateMany(1)
                .ToList();
            var inspection = Fixture.Build<Inspection>()
                .With(x => x.Id, inspectionId)
                .With(x => x.Checks, check)
                .Create();
            var checkRecords = Fixture.Build<CheckRecordReport>()
                .With(x => x.CheckId, checkId)
                .With(x => x.SubmittedUserId, userId)
                .CreateMany(3)
                .ToList();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}")
                    .ReturnsJson(site);
                server.Arrange()
                    .GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"inspections/{inspectionId}/checks/history?siteId={siteId}&customerId={site.CustomerId}&startDate={start}&endDate={end}")
                    .ReturnsJson(checkRecords);

                server.Arrange()
                    .GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/users")
                    .ReturnsJson(users);

                var response = await client.GetAsync($"inspections/{inspectionId}/checks/history/export?siteid={siteId}&startDate={start}&endDate={end}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                response.Content.Headers.ContentType.MediaType.Should().Be("application/octet-stream");
            }
        }
    }
}
