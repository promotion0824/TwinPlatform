using FluentAssertions;
using System;
using System.Net;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using WorkflowCore.Entities;
using AutoFixture;
using Willow.Infrastructure;

namespace WorkflowCore.Test.Features.Reporters
{
    public class DeleteReporterTests : BaseInMemoryTest
    {
        public DeleteReporterTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task TokenIsNotGiven_DeleteReporter_ReturnsUnauthorized()
        {
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient())
            {
                var result = await client.DeleteAsync($"sites/{Guid.NewGuid()}/reporters/{Guid.NewGuid()}");
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        [Fact]
        public async Task ReporterExists_DeleteReporter_ReturnsNoContent()
        {
            var siteId = Guid.NewGuid();
            var reporterEntity = Fixture.Build<ReporterEntity>()
                                      .With(x => x.SiteId, siteId)
                                      .Create();

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Reporters.Add(reporterEntity);
                db.SaveChanges();

                var response = await client.DeleteAsync($"sites/{siteId}/reporters/{reporterEntity.Id}");

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
                db = server.Assert().GetDbContext<WorkflowContext>();
                db.Reporters.Should().BeEmpty();
            }
        }

        [Fact]
        public async Task ReporterDoesNotExist_DeleteReporter_ReturnsNotFound()
        {
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.DeleteAsync($"sites/{Guid.NewGuid()}/reporters/{Guid.NewGuid()}");

                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }
    }
}
