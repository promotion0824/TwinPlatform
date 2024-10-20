using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AutoFixture;
using Azure.Core;
using DirectoryCore.Domain;
using DirectoryCore.Dto;
using DirectoryCore.Entities;
using DirectoryCore.Entities.Permission;
using FluentAssertions;
using OpenTelemetry;
using Willow.Batch;
using Willow.Directory.Models;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace DirectoryCore.Test.Controllers.Sites
{
    public class GetUserPermittedSitesPagedTests : BaseInMemoryTest
    {
        public GetUserPermittedSitesPagedTests(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public async Task WithInvalidUserId_GetUserPermittedSitesPagedTest_ReturnsNotFound()
        {
            var userId = Guid.NewGuid();
            var user = Fixture.Create<UserEntity>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Users.AddRange(user);
                await dbContext.SaveChangesAsync();

                var response = await client.PostAsJsonAsync(
                    $"users/{userId}/sites/paged",
                    new BatchRequestDto()
                );
                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task SitesExist_GetUserPermittedSitesPagedTest_ReturnsThoseSites()
        {
            var page = 1;
            var pageSize = 2;
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
            var expectedRolePermissions = Fixture
                .Build<RolePermissionEntity>()
                .With(x => x.PermissionId, "view-sites")
                .Create();
            var assignments = expectedSites
                .Select(
                    s =>
                        Fixture
                            .Build<AssignmentEntity>()
                            .With(x => x.ResourceId, s.Id)
                            .With(x => x.ResourceType, RoleResourceType.Site)
                            .With(x => x.PrincipalId, user.Id)
                            .With(x => x.RoleId, expectedRolePermissions.RoleId)
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

                var response = await client.PostAsJsonAsync(
                    $"users/{userId}/sites/paged",
                    new BatchRequestDto() { Page = page, PageSize = pageSize }
                );
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<BatchDto<SiteMiniDto>>();
                result
                    .Should()
                    .BeEquivalentTo(expectedSites.Paginate(page, pageSize, SiteMiniDto.MapFrom));
            }
        }
    }
}
