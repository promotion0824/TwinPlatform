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

namespace DirectoryCore.Test.Controllers.Portfolios
{
    public class GetUserTests : BaseInMemoryTest
    {
        public GetUserTests(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public async Task PortfolioNotExist_GetPortfolioUsers_ReturnEmptyList()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.GetAsync($"portfolios/{Guid.NewGuid()}/users");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<IList<UserDto>>();
                result.Should().BeEmpty();
            }
        }

        [Fact]
        public async Task PortfolioWithoutUsersAssignedToPortfolio_GetPortfolioUsers_ReturnEmptyList()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var portfolioId = Guid.NewGuid();
                var customer = Fixture.Build<CustomerEntity>().Create();

                var user = Fixture
                    .Build<UserEntity>()
                    .With(u => u.CustomerId, customer.Id)
                    .Create();

                var role = Fixture.Create<RoleEntity>();
                var site = Fixture
                    .Build<Site>()
                    .With(s => s.CustomerId, customer.Id)
                    .With(s => s.PortfolioId, portfolioId)
                    .Create();
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

                var response = await client.GetAsync($"portfolios/{Guid.NewGuid()}/users");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<IList<UserDto>>();
                result.Should().BeEmpty();
            }
        }

        [Fact]
        public async Task PortfolioWithUsersAssignedToSite_GetPortfolioUsers_ReturnUsersList()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var portfolioId = Guid.NewGuid();
                var customer = Fixture.Build<CustomerEntity>().Create();
                var users = Fixture
                    .Build<UserEntity>()
                    .With(u => u.CustomerId, customer.Id)
                    .CreateMany(2)
                    .ToList();
                var sites = Fixture
                    .Build<Site>()
                    .With(s => s.CustomerId, customer.Id)
                    .With(s => s.PortfolioId, portfolioId)
                    .CreateMany(2)
                    .ToList();
                var role = Fixture.Create<RoleEntity>();
                var assignments = new List<AssignmentEntity>();

                // assign user to site
                assignments.Add(
                    Fixture
                        .Build<AssignmentEntity>()
                        .With(a => a.PrincipalId, users[0].Id)
                        .With(a => a.PrincipalType, PrincipalType.User)
                        .With(a => a.ResourceId, sites[0].Id)
                        .With(a => a.ResourceType, RoleResourceType.Site)
                        .With(a => a.RoleId, role.Id)
                        .Create()
                );

                // assign user to portfolio
                assignments.Add(
                    Fixture
                        .Build<AssignmentEntity>()
                        .With(a => a.PrincipalId, users[1].Id)
                        .With(a => a.PrincipalType, PrincipalType.User)
                        .With(a => a.ResourceId, portfolioId)
                        .With(a => a.ResourceType, RoleResourceType.Portfolio)
                        .With(a => a.RoleId, role.Id)
                        .Create()
                );

                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Customers.Add(customer);
                dbContext.Users.AddRange(users);
                dbContext.Roles.Add(role);
                await dbContext.SaveChangesAsync();
                dbContext.Assignments.AddRange(assignments);
                await dbContext.SaveChangesAsync();

                var response = await client.GetAsync($"portfolios/{portfolioId}/users");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<IList<UserDto>>();
                result.Should().HaveCount(1);
                assignments
                    .Where(a => a.PrincipalId == users[1].Id)
                    .Select(a => a.PrincipalId)
                    .ToList()
                    .Should()
                    .BeEquivalentTo(result.Select(x => x.Id).ToList());
            }
        }
    }
}
