using System;
using System.Collections.Generic;
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
using Willow.Directory.Models;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace DirectoryCore.Test.Controllers.Permission
{
    public class CreateUserAssignmentTests : BaseInMemoryTest
    {
        public CreateUserAssignmentTests(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public async Task AssignmentDoesNotExist_CreateUserAssignment_ReturnNoContent()
        {
            var userId = Guid.NewGuid();
            var request = Fixture.Create<CreateUserAssignmentRequest>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var role = Fixture.Build<RoleEntity>().With(x => x.Id, request.RoleId).Create();
                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Roles.Add(role);
                dbContext.SaveChanges();

                var response = await client.PostAsJsonAsync(
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
        public async Task AssignmentDoesNotExist_CreateUserAssignments_ReturnNoContent()
        {
            var userId = Guid.NewGuid();
            var resourceId1 = Guid.NewGuid();
            var resourceId2 = Guid.NewGuid();
            var resourceId3 = Guid.NewGuid();
            var requestedRoles = new List<RoleAssignment>
            {
                new RoleAssignment
                {
                    PrincipalId = userId,
                    ResourceId = resourceId1,
                    ResourceType = RoleResourceType.Site,
                    RoleId = WellKnownRoleIds.SiteViewer
                },
                new RoleAssignment
                {
                    PrincipalId = userId,
                    ResourceId = resourceId2,
                    ResourceType = RoleResourceType.Site,
                    RoleId = WellKnownRoleIds.SiteViewer
                },
                new RoleAssignment
                {
                    PrincipalId = userId,
                    ResourceId = resourceId3,
                    ResourceType = RoleResourceType.Site,
                    RoleId = WellKnownRoleIds.SiteViewer
                }
            };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Roles.Add(
                    new RoleEntity { Id = WellKnownRoleIds.SiteViewer, Name = "SiteViewer" }
                );
                dbContext.Assignments.Add(
                    new AssignmentEntity
                    {
                        PrincipalId = userId,
                        PrincipalType = PrincipalType.User,
                        ResourceId = Guid.NewGuid(),
                        ResourceType = RoleResourceType.Site,
                        RoleId = WellKnownRoleIds.SiteViewer
                    }
                );
                dbContext.Assignments.Add(
                    new AssignmentEntity
                    {
                        PrincipalId = userId,
                        PrincipalType = PrincipalType.User,
                        ResourceId = Guid.NewGuid(),
                        ResourceType = RoleResourceType.Site,
                        RoleId = WellKnownRoleIds.SiteViewer
                    }
                );
                dbContext.SaveChanges();

                var response = await client.PostAsJsonAsync(
                    $"users/{userId}/permissionAssignments/list",
                    requestedRoles
                );

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
                dbContext = server.Assert().GetDirectoryDbContext();
                dbContext.Assignments.Should().HaveCount(3);
                dbContext
                    .Assignments.Should()
                    .BeEquivalentTo(
                        new List<AssignmentEntity>
                        {
                            new AssignmentEntity
                            {
                                PrincipalId = userId,
                                PrincipalType = PrincipalType.User,
                                RoleId = WellKnownRoleIds.SiteViewer,
                                ResourceId = resourceId1,
                                ResourceType = RoleResourceType.Site
                            },
                            new AssignmentEntity
                            {
                                PrincipalId = userId,
                                PrincipalType = PrincipalType.User,
                                RoleId = WellKnownRoleIds.SiteViewer,
                                ResourceId = resourceId2,
                                ResourceType = RoleResourceType.Site
                            },
                            new AssignmentEntity
                            {
                                PrincipalId = userId,
                                PrincipalType = PrincipalType.User,
                                RoleId = WellKnownRoleIds.SiteViewer,
                                ResourceId = resourceId3,
                                ResourceType = RoleResourceType.Site
                            }
                        }
                    );
            }
        }

        [Fact]
        public async Task AssignmentExist_CreateUserAssignment_ReturnNoContentAndNoAssignmentIsCreated()
        {
            var userId = Guid.NewGuid();
            var request = Fixture.Create<CreateUserAssignmentRequest>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var role = Fixture.Build<RoleEntity>().With(x => x.Id, request.RoleId).Create();
                var assignment = new AssignmentEntity
                {
                    PrincipalId = userId,
                    PrincipalType = PrincipalType.User,
                    RoleId = request.RoleId,
                    ResourceId = request.ResourceId,
                    ResourceType = request.ResourceType
                };
                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Roles.Add(role);
                dbContext.SaveChanges();
                dbContext.Assignments.Add(assignment);
                dbContext.SaveChanges();

                var response = await client.PostAsJsonAsync(
                    $"users/{userId}/permissionAssignments",
                    request
                );

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
                dbContext = server.Assert().GetDirectoryDbContext();
                dbContext.Assignments.Should().HaveCount(1);
            }
        }
    }
}
