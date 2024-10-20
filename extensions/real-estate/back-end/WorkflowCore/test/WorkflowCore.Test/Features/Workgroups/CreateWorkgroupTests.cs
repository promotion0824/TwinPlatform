using AutoFixture;
using FluentAssertions;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using WorkflowCore.Controllers.Request;
using WorkflowCore.Dto;
using WorkflowCore.Entities;
using Xunit;
using Xunit.Abstractions;

namespace WorkflowCore.Test.Features.Workgroups
{
    public class CreateWorkgroupTests : BaseInMemoryTest
    {
        public CreateWorkgroupTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task ValidInput_CreateWorkgroup_ReturnsCreatedWorkgroup()
        {
            var siteId = Guid.NewGuid();
            var request = Fixture.Create<CreateWorkgroupRequest>();

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.PostAsJsonAsync($"sites/{siteId}/workgroups", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<WorkgroupDto>();
                result.Name.Should().Be(request.Name);
                var db = server.Assert().GetDbContext<WorkflowContext>();
                db.Workgroups.Should().HaveCount(1);
                var entity = db.Workgroups.First();
                entity.Id.Should().Be(result.Id);
                entity.Name.Should().Be(request.Name);
                var memberIds = db.WorkgroupMembers.Where(x => x.WorkgroupId == result.Id).Select(x => x.MemberId);
                memberIds.Should().BeEquivalentTo(request.MemberIds);
            }
        }
    }
}
