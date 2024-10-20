using AutoFixture;
using FluentAssertions;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Willow.Infrastructure;
using Willow.Tests.Infrastructure;
using WorkflowCore.Entities;
using Xunit;
using Xunit.Abstractions;

namespace WorkflowCore.Test.Features.Workgroups
{
    public class DeleteWorkgroupTests : BaseInMemoryTest
    {
        public DeleteWorkgroupTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task WorkgroupExist_DeleteWorkgroup_ReturnNoContent()
        {
            var workgroupEntity = Fixture.Create<WorkgroupEntity>();
            
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Workgroups.Add(workgroupEntity);
                db.SaveChanges();

                var response = await client.DeleteAsync($"/sites/{workgroupEntity.SiteId}/workgroups/{workgroupEntity.Id}");

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
                db.Workgroups.Count().Should().Be(0);
            }
        }

        [Fact]
        public async Task WorkgroupNotExist_DeleteWorkgroup_ReturnNotFound()
        {
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.DeleteAsync($"/sites/{Guid.NewGuid()}/workgroups/{Guid.NewGuid()}");

                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }
    }
}
