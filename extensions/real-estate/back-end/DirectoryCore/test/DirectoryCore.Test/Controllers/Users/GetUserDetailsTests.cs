using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using DirectoryCore.Data;
using DirectoryCore.Dto;
using DirectoryCore.Entities;
using DirectoryCore.Entities.Permission;
using DirectoryCore.Enums;
using DirectoryCore.Services;
using FluentAssertions;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace DirectoryCore.Test.Controllers.Users;

public class GetUserDetailsTests : BaseInMemoryTest
{
    public GetUserDetailsTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task Unauthorized_GetUserDetails_ReturnUnauthorized()
    {
        using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);
        using var client = server.CreateClient();
        var response = await client.GetAsync($"users/{Guid.NewGuid()}/userDetails");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UserNotExist_GetUserDetails_ReturnNotFound()
    {
        using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);
        using var client = server.CreateClient(null);
        var response = await client.GetAsync($"users/{Guid.NewGuid()}/userDetails");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UserExist_GetUserDetails_ReturnUserDetails()
    {
        var existingCustomer = Fixture
            .Build<CustomerEntity>()
            .With(x => x.Status, CustomerStatus.Active)
            .Create();
        var existingUser = Fixture
            .Build<UserEntity>()
            .With(x => x.CustomerId, existingCustomer.Id)
            .With(x => x.Status, UserStatus.Active)
            .Create();

        var permissionIds = new string[]
        {
            Permissions.ViewSites,
            Permissions.ManageSites,
            Permissions.ManageUsers,
            Permissions.ManageFloors
        };

        var rolePermissionsList = new List<RolePermissionEntity>();
        var assignmentEntityList = new List<AssignmentEntity>();
        foreach (var permission in permissionIds)
        {
            var rolePermission = Fixture
                .Build<RolePermissionEntity>()
                .With(x => x.PermissionId, permission)
                .Create();
            rolePermissionsList.Add(rolePermission);

            assignmentEntityList.Add(
                Fixture
                    .Build<AssignmentEntity>()
                    .With(x => x.RoleId, rolePermission.RoleId)
                    .With(x => x.PrincipalId, existingUser.Id)
                    .Create()
            );
        }

        foreach (var permission in permissionIds)
        {
            var rolePermission = Fixture
                .Build<RolePermissionEntity>()
                .With(x => x.PermissionId, permission)
                .Create();
            rolePermissionsList.Add(rolePermission);

            assignmentEntityList.Add(
                Fixture
                    .Build<AssignmentEntity>()
                    .With(x => x.RoleId, rolePermission.RoleId)
                    .With(x => x.PrincipalId, Guid.NewGuid)
                    .Create()
            );
        }

        using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);
        using var client = server.CreateClient(null);

        var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
        dbContext.Customers.Add(existingCustomer);
        dbContext.Users.Add(existingUser);
        dbContext.RolePermission.AddRange(rolePermissionsList);
        dbContext.Assignments.AddRange(assignmentEntityList);
        await dbContext.SaveChangesAsync();

        var response = await client.GetAsync($"users/{existingUser.Id}/userDetails");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadAsAsync<UserDetailsDto>();

        var expectedUserDetails = UserDetailsDto.MapFrom(
            existingUser,
            existingCustomer,
            new ImagePathHelper()
        );

        var expectedAssignments = new List<UserAssignment>
        {
            new UserAssignment(
                Permissions.ViewSites,
                assignmentEntityList
                    .Where(
                        x =>
                            x.RoleId
                            == rolePermissionsList
                                .Where(x => x.PermissionId == Permissions.ViewSites)
                                .FirstOrDefault()
                                .RoleId
                    )
                    .FirstOrDefault()
                    .ResourceId
            ),
            new UserAssignment(
                Permissions.ManageFloors,
                assignmentEntityList
                    .Where(
                        x =>
                            x.RoleId
                            == rolePermissionsList
                                .Where(x => x.PermissionId == Permissions.ManageFloors)
                                .FirstOrDefault()
                                .RoleId
                    )
                    .FirstOrDefault()
                    .ResourceId
            )
        };
        expectedUserDetails.UserAssignments = expectedAssignments;
        result.Should().BeEquivalentTo(expectedUserDetails);

        result.Should().NotBeNull();
    }
}
