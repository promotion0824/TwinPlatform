using WorkflowCore.Dto;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using WorkflowCore.Entities;
using AutoFixture;

namespace WorkflowCore.Test.Features.Reporters
{
    public class GetReportersTests : BaseInMemoryTest
    {
        public GetReportersTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task TokenIsNotGiven_GetSiteReporters_ReturnsUnauthorized()
        {
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient())
            {
                var result = await client.GetAsync($"sites/{Guid.NewGuid()}/reporters");
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        [Fact]
        public async Task GivenSiteHasReporters_GetSiteReporters_ReturnsSiteReporters()
        {
            var siteId = Guid.NewGuid();
            var reporterEntity = Fixture.Build<ReporterEntity>()
                                      .With(x => x.SiteId, siteId)
                                      .CreateMany(10);
            var expectedReports = ReporterDto.MapFromModels(ReporterEntity.MapToModels(reporterEntity));

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Reporters.AddRange(reporterEntity);
                db.SaveChanges();

                var response = await client.GetAsync($"sites/{siteId}/reporters");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<ReporterDto>>();
                result.Should().BeEquivalentTo(expectedReports);
            }
        }
    }
}
