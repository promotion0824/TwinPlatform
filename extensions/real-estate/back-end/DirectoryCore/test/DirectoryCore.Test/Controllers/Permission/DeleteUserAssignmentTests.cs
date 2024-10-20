using System;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using DirectoryCore.Entities;
using DirectoryCore.Entities.Permission;
using DirectoryCore.Enums;
using FluentAssertions;
using Willow.Directory.Models;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace DirectoryCore.Test.Controllers.Permission
{
    public class DeleteUserAssignmentTests : BaseInMemoryTest
    {
        public DeleteUserAssignmentTests(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public async Task AssignmentExist_DeleteUserAssignmentByResource_AssignmentDeletedAndReturnNoContent()
        {
            var userId = Guid.NewGuid();
            var siteId = Guid.NewGuid();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var role = Fixture.Create<RoleEntity>();
                var assignment = new AssignmentEntity
                {
                    PrincipalId = userId,
                    PrincipalType = PrincipalType.User,
                    RoleId = role.Id,
                    ResourceId = siteId,
                    ResourceType = RoleResourceType.Site
                };
                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Roles.Add(role);
                dbContext.Assignments.Add(assignment);
                dbContext.SaveChanges();

                var response = await client.DeleteAsync(
                    $"users/{userId}/permissionAssignments?resourceId={siteId}"
                );

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
                dbContext = server.Assert().GetDirectoryDbContext();
                dbContext.Assignments.Should().HaveCount(0);
            }
        }
    }
}
