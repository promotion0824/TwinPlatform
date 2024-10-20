using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using DirectoryCore.Domain;
using DirectoryCore.Dto;
using DirectoryCore.Entities;
using DirectoryCore.Entities.Permission;
using DirectoryCore.Enums;
using FluentAssertions;
using Willow.Directory.Models;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace DirectoryCore.Test.Controllers.Sites
{
    public class GetUserTests : BaseInMemoryTest
    {
        public GetUserTests(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public async Task SiteNotExist_GetSiteUsers_ReturnEmptyList()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.GetAsync($"sites/{Guid.NewGuid()}/users");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<IList<UserDto>>();
                result.Should().BeEmpty();
            }
        }

        [Fact]
        public async Task NoUsersAssignedToSite_GetSiteUsers_ReturnEmptyList()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var customer = Fixture.Build<CustomerEntity>().Create();

                var user = Fixture
                    .Build<UserEntity>()
                    .With(u => u.CustomerId, customer.Id)
                    .Create();

                var role = Fixture.Create<RoleEntity>();
                var site = Fixture.Build<Site>().With(s => s.CustomerId, customer.Id).Create();
                var assignment = Fixture
                    .Build<AssignmentEntity>()
                    .With(a => a.PrincipalId, user.Id)
                    .With(a => a.PrincipalType, PrincipalType.User)
                    .With(a => a.ResourceId, site.Id)
                    .With(a => a.ResourceType, RoleResourceType.Site)
                    .With(a => a.RoleId, role.Id)
                    .Create();

                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Customers.Add(customer);
                dbContext.Users.Add(user);
                dbContext.Roles.Add(role);
                await dbContext.SaveChangesAsync();
                dbContext.Assignments.Add(assignment);
                await dbContext.SaveChangesAsync();

                var response = await client.GetAsync($"sites/{Guid.NewGuid()}/users");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<IList<UserDto>>();
                result.Should().BeEmpty();
            }
        }

        [Fact]
        public async Task UsersAssignedToSite_GetSiteUsers_ReturnUsersList()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var customer = Fixture.Build<CustomerEntity>().Create();
                var users = Fixture
                    .Build<UserEntity>()
                    .With(u => u.CustomerId, customer.Id)
                    .CreateMany(2);
                var site = Fixture.Build<Site>().With(s => s.CustomerId, customer.Id).Create();
                var role = Fixture.Create<RoleEntity>();
                var assignments = new List<AssignmentEntity>();
                foreach (var u in users)
                {
                    assignments.Add(
                        Fixture
                            .Build<AssignmentEntity>()
                            .With(a => a.PrincipalId, u.Id)
                            .With(a => a.PrincipalType, PrincipalType.User)
                            .With(a => a.ResourceId, site.Id)
                            .With(a => a.ResourceType, RoleResourceType.Site)
                            .With(a => a.RoleId, role.Id)
                            .Create()
                    );
                }

                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Customers.Add(customer);
                dbContext.Users.AddRange(users);
                dbContext.Roles.Add(role);
                await dbContext.SaveChangesAsync();
                dbContext.Assignments.AddRange(assignments);
                await dbContext.SaveChangesAsync();

                var response = await client.GetAsync($"sites/{site.Id}/users");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<IList<UserDto>>();
                result.Should().HaveCount(2);
                assignments
                    .Select(a => a.PrincipalId)
                    .ToList()
                    .Should()
                    .BeEquivalentTo(result.Select(x => x.Id).ToList());
            }
        }
    }
}
