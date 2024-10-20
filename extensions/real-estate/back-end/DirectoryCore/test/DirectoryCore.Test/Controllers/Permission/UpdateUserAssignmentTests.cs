using System;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AutoFixture;
using DirectoryCore.Controllers.Requests;
using DirectoryCore.Entities;
using DirectoryCore.Entities.Permission;
using DirectoryCore.Enums;
using FluentAssertions;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace DirectoryCore.Test.Controllers.Permission
{
    public class UpdateUserAssignmentTests : BaseInMemoryTest
    {
        public UpdateUserAssignmentTests(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public async Task AssignmentDoesNotExist_UpdateUserAssignment_AssignmentCreatedAndReturnNoContent()
        {
            var userId = Guid.NewGuid();
            var request = Fixture.Create<UpdateUserAssignmentRequest>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var role = Fixture.Build<RoleEntity>().With(x => x.Id, request.RoleId).Create();
                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Roles.Add(role);
                dbContext.SaveChanges();

                var response = await client.PutAsJsonAsync(
                    $"users/{userId}/permissionAssignments",
                    request
                );

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
                dbContext = server.Assert().GetDirectoryDbContext();
                dbContext.Assignments.Should().HaveCount(1);
                dbContext
                    .Assignments.First()
                    .Should()
                    .BeEquivalentTo(
                        new AssignmentEntity
                        {
                            PrincipalId = userId,
                            PrincipalType = PrincipalType.User,
                            RoleId = request.RoleId,
                            ResourceId = request.ResourceId,
                            ResourceType = request.ResourceType
                        }
                    );
            }
        }

        [Fact]
        public async Task AssignmentExist_UpdateUserAssignment_AssignmentUpdatedAndReturnNoContent()
        {
            var userId = Guid.NewGuid();
            var request = Fixture.Create<UpdateUserAssignmentRequest>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var role = Fixture.Create<RoleEntity>();
                var newRole = Fixture.Build<RoleEntity>().With(x => x.Id, request.RoleId).Create();
                var assignment = new AssignmentEntity
                {
                    PrincipalId = userId,
                    PrincipalType = PrincipalType.User,
                    RoleId = role.Id,
                    ResourceId = request.ResourceId,
                    ResourceType = request.ResourceType
                };
                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Roles.Add(role);
                dbContext.Roles.Add(newRole);
                dbContext.SaveChanges();
                dbContext.Assignments.Add(assignment);
                dbContext.SaveChanges();

                var response = await client.PutAsJsonAsync(
                    $"users/{userId}/permissionAssignments",
                    request
                );

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
                dbContext = server.Assert().GetDirectoryDbContext();
                dbContext.Assignments.Should().HaveCount(1);
                dbContext
                    .Assignments.First()
                    .Should()
                    .BeEquivalentTo(
                        new AssignmentEntity
                        {
                            PrincipalId = userId,
                            PrincipalType = PrincipalType.User,
                            RoleId = request.RoleId,
                            ResourceId = request.ResourceId,
                            ResourceType = request.ResourceType
                        }
                    );
            }
        }
    }
}
