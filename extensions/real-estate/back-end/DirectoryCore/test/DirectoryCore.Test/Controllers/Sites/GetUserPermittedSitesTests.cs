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
using FluentAssertions;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace DirectoryCore.Test.Controllers.Sites
{
    public class GetUserPermittedSitesTests : BaseInMemoryTest
    {
        public GetUserPermittedSitesTests(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public async Task WithInvalidUserId_GetUserPermittedSitesTest_ReturnsNotFound()
        {
            var userId = Guid.NewGuid();
            var customer = Fixture.Build<CustomerEntity>().Create();
            var user = Fixture.Build<UserEntity>().With(u => u.CustomerId, customer.Id).Create();
            var expectedSites = Fixture
                .Build<Site>()
                .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                .With(x => x.Features)
                .With(x => x.CustomerId, customer.Id)
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                //dbContext.Sites.AddRange(expectedSites);
                dbContext.Customers.AddRange(customer);
                dbContext.Users.AddRange(user);
                await dbContext.SaveChangesAsync();

                var response = await client.GetAsync(
                    $"users/{userId}/sites?permissionId=view-sites"
                );
                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task WithEmptyPermissionId_GetUserPermittedSitesTest_ReturnsBadRequest()
        {
            var userId = Guid.NewGuid();
            var customer = Fixture.Build<CustomerEntity>().Create();
            var user = Fixture.Build<UserEntity>().With(u => u.CustomerId, customer.Id).Create();
            var expectedSites = Fixture
                .Build<Site>()
                .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                .With(x => x.Features)
                .With(x => x.CustomerId, customer.Id)
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                //dbContext.Sites.AddRange(expectedSites);
                dbContext.Customers.AddRange(customer);
                dbContext.Users.AddRange(user);
                await dbContext.SaveChangesAsync();

                var response = await client.GetAsync($"users/{userId}/sites");
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Theory]
        [InlineData("view-sites")]
        [InlineData("manage-sites")]
        public async Task SitesWithValidInputData_GetUserPermittedSitesTest_ReturnsThoseSites(
            string permissionId
        )
        {
            var userId = Guid.NewGuid();
            var customer = Fixture.Build<CustomerEntity>().Create();
            var user = Fixture
                .Build<UserEntity>()
                .With(u => u.CustomerId, customer.Id)
                .With(u => u.Id, userId)
                .Create();
            var expectedSites = Fixture
                .Build<Site>()
                .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                .With(x => x.Features)
                .With(x => x.CustomerId, user.Id)
                .With(x => x.CustomerId, customer.Id)
                .CreateMany(3)
                .ToList();
            var otherSites = Fixture
                .Build<Site>()
                .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                .CreateMany(2);
            var expectedRolePermissions = Fixture
                .Build<RolePermissionEntity>()
                .With(x => x.PermissionId, permissionId)
                .CreateMany(3)
                .ToList();
            var i = 0;
            foreach (var expectedRole in expectedRolePermissions)
            {
                expectedRole.RoleId = expectedSites[i].PortfolioId.Value;
                i++;
            }
            var assignments = expectedRolePermissions
                .Select(
                    a =>
                        Fixture
                            .Build<AssignmentEntity>()
                            .With(x => x.ResourceId, customer.Id)
                            .With(x => x.PrincipalId, user.Id)
                            .With(x => x.RoleId, a.RoleId)
                            .Create()
                )
                .ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Customers.AddRange(customer);
                dbContext.Assignments.AddRange(assignments);
                dbContext.Users.AddRange(user);
                dbContext.RolePermission.AddRange(expectedRolePermissions);
                await dbContext.SaveChangesAsync();

                var url = $"sites/customer/{customer.Id}/extend";
                server
                    .Arrange()
                    .GetSiteCoreApi()
                    .SetupRequest(HttpMethod.Get, url)
                    .ReturnsJson(expectedSites);

                var response = await client.GetAsync(
                    $"users/{userId}/sites?permissionId={permissionId}"
                );
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<SiteDto>>();
                result.Should().BeEquivalentTo(SiteDto.MapFrom(expectedSites));
            }
        }
    }
}
