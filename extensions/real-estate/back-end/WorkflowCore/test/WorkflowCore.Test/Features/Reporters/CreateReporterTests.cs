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
using System.Net.Http.Json;

namespace WorkflowCore.Test.Features.Reporters
{
    public class CreateReporterTests : BaseInMemoryTest
    {
        public CreateReporterTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task TokenIsNotGiven_CreateReporter_ReturnsUnauthorized()
        {
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient())
            {
                var result = await client.PostAsJsonAsync($"sites/{Guid.NewGuid()}/reporters", new object());
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        [Fact]
        public async Task ValidInput_CreateReporter_ReturnsCreatedReporter()
        {
            var siteId = Guid.NewGuid();
            var request = Fixture.Create<CreateReporterRequest>();

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.PostAsJsonAsync($"sites/{siteId}/reporters", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<ReporterDto>();
                result.SiteId.Should().Be(siteId);
                result.Name.Should().Be(request.Name);
                result.Phone.Should().Be(request.Phone);
                result.Email.Should().Be(request.Email);
                result.Company.Should().Be(request.Company);
                var db = server.Assert().GetDbContext<WorkflowContext>();
                db.Reporters.Should().HaveCount(1);
                var entity = db.Reporters.First();
                entity.Id.Should().Be(result.Id);
                entity.CustomerId.Should().Be(request.CustomerId);
                entity.SiteId.Should().Be(siteId);
                entity.Name.Should().Be(request.Name);
                entity.Phone.Should().Be(request.Phone);
                entity.Email.Should().Be(request.Email);
                entity.Company.Should().Be(request.Company);
            }
        }
    }
}
