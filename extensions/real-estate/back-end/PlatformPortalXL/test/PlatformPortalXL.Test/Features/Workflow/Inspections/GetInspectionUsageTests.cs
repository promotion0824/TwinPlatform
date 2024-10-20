using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Models;
using Willow.Platform.Users;
using Willow.Tests.Infrastructure;
using Willow.Workflow;
using Xunit;
using Xunit.Abstractions;
using System.Linq;

namespace PlatformPortalXL.Test.Features.Inspections
{
    public class GetInspectionUsageTests : BaseInMemoryTest
    {
        public GetInspectionUsageTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task InspectionCheckExists_GetInspectionUsage_ReturnsThisInspectionUsage()
        {
            var siteId = Guid.NewGuid();
            var period = Fixture.Create<InspectionUsagePeriod>();
            var userId = Guid.NewGuid();
            var userIds = new List<Guid>() { userId };
            var users = Fixture.Build<User>().With(x => x.Id, userId).CreateMany(1);
            var expectedInspectionUsage = Fixture.Build<InspectionUsage>().With(x => x.UserIds, userIds).Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/inspectionUsage?inspectionUsagePeriod={period}")
                    .ReturnsJson(expectedInspectionUsage);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{userId}")
                    .ReturnsJson(users.First());

                var response = await client.GetAsync($"sites/{siteId}/inspectionUsage?period={period}");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }
    }
}