using AutoFixture;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using WorkflowCore.Dto;
using WorkflowCore.Entities;
using WorkflowCore.Models;
using Xunit;
using Xunit.Abstractions;
using static WorkflowCore.CommSvc.Templates.Email;

namespace WorkflowCore.Test.Features.Workgroups
{
    public class GetWorkgroupsTests : BaseInMemoryTest
    {
        public GetWorkgroupsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task WorkgroupsExist_GetWorkgroups_ReturnWorkgroups()
        {
            var workgroupEntities = Fixture.Build<WorkgroupEntity>().CreateMany(10);

            var siteId = workgroupEntities.First().SiteId;

            var expectedTwinId = "TestTwinId";
            var expectedTwinName = "TestTwinName";
            var expectedTwinIds = new List<TwinIdDto>()
            {
                Fixture.Build<TwinIdDto>()
                    .With(c => c.UniqueId, workgroupEntities.First().SiteId.ToString())
                    .With(x => x.Id, expectedTwinId)
                    .With(x => x.Name, expectedTwinName)
                .Create(),
            };

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Workgroups.AddRange(workgroupEntities);
                db.SaveChanges();

                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"admin/sites/{siteId}/twins/byUniqueId/batch?uniqueIds={siteId}")
                    .ReturnsJson(expectedTwinIds);

                var response = await client.GetAsync($"sites/{siteId}/workgroups");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<WorkgroupDto>>();

                result.First().Name.Should().StartWith(expectedTwinName);
            }
        }

        [Theory]
        [InlineData("TestSiteName")]
        [InlineData("")]
        [InlineData(null)]
        public async Task WorkgroupsExistNoSiteId_GetWorkgroups_ReturnWorkgroups(string siteName)
        {
            var siteId = string.IsNullOrEmpty(siteName) ? Guid.NewGuid() : Guid.Empty;

            var expectedTwinId = "TestTwinId";
            var expectedTwinName = siteName ?? "TestTwinName";
            var expectedTwinIds = new List<TwinIdDto>()
            {
                Fixture.Build<TwinIdDto>()
                    .With(c => c.UniqueId, siteId.ToString())
                    .With(x => x.Id, expectedTwinId)
                    .With(x => x.Name, expectedTwinName)
                .Create(),
            };

            var workgroupName = "TestWorkgroupName";
            var prefixedWorkgroupName = $"{expectedTwinName} - TestWorkgroupName";

            var workgroupEntities = Fixture.Build<WorkgroupEntity>()
                .With(x => x.SiteId, siteId)
                .With(x => x.Name, siteId == Guid.Empty ? prefixedWorkgroupName : workgroupName)
                .CreateMany(1);

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Workgroups.AddRange(workgroupEntities);
                db.SaveChanges();

                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"admin/sites/{siteId}/twins/byUniqueId/batch?uniqueIds={siteId}")
                    .ReturnsJson(expectedTwinIds);

                var response = await client.GetAsync($"workgroups/all/{siteName}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<WorkgroupDto>>();

                result.First().Name.Should().StartWith(prefixedWorkgroupName);
            }
        }
    }
}
