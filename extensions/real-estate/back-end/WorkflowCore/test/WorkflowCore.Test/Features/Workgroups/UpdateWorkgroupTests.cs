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
    public class UpdateWorkgroupTests : BaseInMemoryTest
    {
        public UpdateWorkgroupTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task ValidInput_UpdateWorkgroup_ReturnsUpdatedWorkgroup()
        {
            var siteId = Guid.NewGuid();
            var workgroupEntity = Fixture.Build<WorkgroupEntity>().With(w => w.SiteId, siteId).Create();
            var request = Fixture.Create<UpdateWorkgroupRequest>();
            var toBeDeletedMemberId = Guid.NewGuid();

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Workgroups.Add(workgroupEntity);
                db.SaveChanges();
                db.WorkgroupMembers.Add(new WorkgroupMemberEntity { WorkgroupId = workgroupEntity.Id, MemberId = toBeDeletedMemberId});
                db.SaveChanges();

                var response = await client.PutAsJsonAsync($"sites/{siteId}/workgroups/{workgroupEntity.Id}", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<WorkgroupDto>();
                result.Name.Should().Be(request.Name);
                db = server.Assert().GetDbContext<WorkflowContext>();
                db.Workgroups.Should().HaveCount(1);
                var entity = db.Workgroups.First();
                entity.Id.Should().Be(result.Id);
                entity.Name.Should().Be(request.Name);
                var memberIds = db.WorkgroupMembers.Where(x => x.WorkgroupId == result.Id).Select(x => x.MemberId);
                memberIds.Should().NotContain(toBeDeletedMemberId);
                memberIds.Should().BeEquivalentTo(request.MemberIds);
            }
        }
        
        [Fact]
        public async Task WorkgroupDoesNotExist_UpdateWorkgroup_ReturnsNotFound()
        {
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.PutAsJsonAsync($"sites/{Guid.NewGuid()}/workgroups/{Guid.NewGuid()}", new UpdateWorkgroupRequest());

                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }
    }
}
