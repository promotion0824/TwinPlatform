using WorkflowCore.Dto;
using FluentAssertions;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using WorkflowCore.Entities;
using AutoFixture;
using System.Linq;
using WorkflowCore.Controllers.Request;
using System.Net.Http.Json;

namespace WorkflowCore.Test.Features.Settings
{
    public class UpsertSiteSettingsTests : BaseInMemoryTest
    {
        public UpsertSiteSettingsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task TokenIsNotGiven_CreateOrUpdateSiteExtensions_RequiresAuthorization()
        {
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient())
            {
                var result = await client.PutAsync($"sites/{Guid.NewGuid()}/settings", null);
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        [Fact]
        public async Task GivenValidInput_GetSiteExtensions_ReturnSiteExtensions()
        {
            var siteExtensions = Fixture.Build<SiteExtensionEntity>().Create();
            var request = new UpsertSiteSettingsRequest
            {
                InspectionDailyReportWorkgroupId = Guid.NewGuid()
            };
            
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.PutAsJsonAsync($"sites/{siteExtensions.SiteId}/settings", request);
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<SiteExtensionsDto>();
                result.InspectionDailyReportWorkgroupId.Should().Be(request.InspectionDailyReportWorkgroupId);
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.SiteExtensions.Count().Should().Be(1);
            }
        }

    }
}
