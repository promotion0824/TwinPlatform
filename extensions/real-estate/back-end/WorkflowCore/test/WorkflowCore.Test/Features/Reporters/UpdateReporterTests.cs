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
using WorkflowCore.Controllers.Request;
using System.Linq;
using Willow.Infrastructure;
using System.Net.Http.Json;

namespace WorkflowCore.Test.Features.Reporters
{
    public class UpdateReporterTests : BaseInMemoryTest
    {
        public UpdateReporterTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task TokenIsNotGiven_UpdateReporter_ReturnsUnauthorized()
        {
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient())
            {
                var result = await client.PutAsJsonAsync($"sites/{Guid.NewGuid()}/reporters/{Guid.NewGuid()}", new object());
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        [Fact]
        public async Task ValidInput_UpdateReporter_ReturnsReporter()
        {
            var siteId = Guid.NewGuid();
            var reporterEntity = Fixture.Build<ReporterEntity>()
                                      .With(x => x.SiteId, siteId)
                                      .Create();
            var request = Fixture.Create<UpdateReporterRequest>();

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Reporters.Add(reporterEntity);
                db.SaveChanges();

                var response = await client.PutAsJsonAsync($"sites/{siteId}/reporters/{reporterEntity.Id}", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<ReporterDto>();
                result.Name.Should().Be(request.Name);
                result.Phone.Should().Be(request.Phone);
                result.Email.Should().Be(request.Email);
                result.Company.Should().Be(request.Company);
                db = server.Assert().GetDbContext<WorkflowContext>();
                db.Reporters.Should().HaveCount(1);
                var entity = db.Reporters.First();
                entity.Id.Should().Be(result.Id);
                entity.SiteId.Should().Be(siteId);
                entity.Name.Should().Be(request.Name);
                entity.Phone.Should().Be(request.Phone);
                entity.Email.Should().Be(request.Email);
                entity.Company.Should().Be(request.Company);
            }
        }

        [Fact]
        public async Task ReporterDoesNotExist_UpdateReporter_ReturnsNotFound()
        {
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.PutAsJsonAsync($"sites/{Guid.NewGuid()}/reporters/{Guid.NewGuid()}", new UpdateReporterRequest());

                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }
    }
}
