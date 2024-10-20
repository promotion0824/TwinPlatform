using System;
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
using Newtonsoft.Json;
using Willow.Directory.Models;
using Willow.Infrastructure;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace DirectoryCore.Test.Controllers.Permission
{
    public class AuthorizeUserPermissionOnSiteTests : BaseInMemoryTest
    {
        public AuthorizeUserPermissionOnSiteTests(ITestOutputHelper output)
            : base(output)
        {
            Fixture
                .Behaviors.OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => Fixture.Behaviors.Remove(b));
            Fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        [Fact]
        public async Task AuthorizeSiteResourcePermission_PermissionNotExists_ReturnNotAuthorized()
        {
            var user = new UserDto { Id = Guid.NewGuid(), Auth0UserId = "auth0 user id" };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null, user.Id, user.Auth0UserId))
            {
                var customer = Fixture.Build<CustomerEntity>().Create();

                var role = Fixture.Build<RoleEntity>().Create();

                var permission = Fixture
                    .Build<PermissionEntity>()
                    .With(p => p.Id, "MangeBuilding")
                    .Create();

                var rolePermission = Fixture
                    .Build<RolePermissionEntity>()
                    .With(r => r.RoleId, role.Id)
                    .With(r => r.PermissionId, permission.Id)
                    .Create();

                var site = Fixture.Build<Site>().With(s => s.CustomerId, customer.Id).Create();

                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Customers.Add(customer);
                dbContext.Roles.Add(role);
                dbContext.Permissions.Add(permission);
                dbContext.SaveChanges();
                dbContext.RolePermission.Add(rolePermission);
                dbContext.SaveChanges();

                var url = $"sites/{site.Id}/extend";
                server
                    .Arrange()
                    .GetSiteCoreApi()
                    .SetupRequest(HttpMethod.Get, url)
                    .ReturnsJson(site);

                var response = await client.GetAsync(
                    $"users/{user.Id}/permissions/{permission.Id}/eligibility?siteId={site.Id}"
                );
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<AuthorizationInfo>();
                result.IsAuthorized.Should().Be(false);
            }
        }

        [Fact]
        public async Task AuthorizeSiteResourcePermission_SitePermissionExists_ReturnAuthorized()
        {
            var user = new UserDto { Id = Guid.NewGuid(), Auth0UserId = "auth0 user id" };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null, user.Id, user.Auth0UserId))
            {
                var role = Fixture.Build<RoleEntity>().Create();

                var permission = Fixture
                    .Build<PermissionEntity>()
                    .With(p => p.Id, "MangeBuilding")
                    .Create();

                var rolePermission = Fixture
                    .Build<RolePermissionEntity>()
                    .With(r => r.RoleId, role.Id)
                    .With(r => r.PermissionId, permission.Id)
                    .Create();

                var site = Fixture.Build<Site>().Create();

                var assignment = Fixture
                    .Build<AssignmentEntity>()
                    .With(a => a.PrincipalId, user.Id)
                    .With(a => a.RoleId, role.Id)
                    .With(a => a.ResourceId, site.Id)
                    .Create();

                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Roles.Add(role);
                dbContext.Permissions.Add(permission);
                dbContext.SaveChanges();
                dbContext.RolePermission.Add(rolePermission);
                dbContext.Assignments.Add(assignment);
                dbContext.SaveChanges();

                var url = $"sites/{site.Id}/extend";
                server
                    .Arrange()
                    .GetSiteCoreApi()
                    .SetupRequest(HttpMethod.Get, url)
                    .ReturnsJson(site);

                var response = await client.GetAsync(
                    $"users/{user.Id}/permissions/{permission.Id}/eligibility?siteId={site.Id}"
                );
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<AuthorizationInfo>();
                result.IsAuthorized.Should().Be(true);
            }
        }

        [Fact]
        public async Task AuthorizeSiteResourcePermission_PortfolioPermissionExists_ReturnAuthorized()
        {
            var user = new UserDto { Id = Guid.NewGuid(), Auth0UserId = "auth0 user id" };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null, user.Id, user.Auth0UserId))
            {
                var customer = Fixture.Build<CustomerEntity>().Create();

                var role = Fixture.Build<RoleEntity>().Create();

                var permission = Fixture
                    .Build<PermissionEntity>()
                    .With(p => p.Id, "MangeBuilding")
                    .Create();

                var rolePermission = Fixture
                    .Build<RolePermissionEntity>()
                    .With(r => r.RoleId, role.Id)
                    .With(r => r.PermissionId, permission.Id)
                    .Create();

                var portfolio = Fixture
                    .Build<PortfolioEntity>()
                    .With(p => p.CustomerId, customer.Id)
                    .Create();

                var site = Fixture
                    .Build<Site>()
                    .With(s => s.PortfolioId, portfolio.Id)
                    .With(s => s.CustomerId, customer.Id)
                    .Create();

                var assignment = Fixture
                    .Build<AssignmentEntity>()
                    .With(a => a.PrincipalId, user.Id)
                    .With(a => a.RoleId, role.Id)
                    .With(a => a.ResourceId, portfolio.Id)
                    .Create();

                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Customers.Add(customer);
                dbContext.Roles.Add(role);
                dbContext.Permissions.Add(permission);
                dbContext.SaveChanges();
                dbContext.RolePermission.Add(rolePermission);
                dbContext.Assignments.Add(assignment);
                dbContext.Portfolios.Add(portfolio);
                dbContext.SaveChanges();

                var url = $"sites/{site.Id}/extend";
                server
                    .Arrange()
                    .GetSiteCoreApi()
                    .SetupRequest(HttpMethod.Get, url)
                    .ReturnsJson(site);

                var response = await client.GetAsync(
                    $"users/{user.Id}/permissions/{permission.Id}/eligibility?siteId={site.Id}"
                );
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<AuthorizationInfo>();
                result.IsAuthorized.Should().Be(true);
            }
        }

        [Fact]
        public async Task AuthorizeSiteResourcePermission_CustomerPermissionExists_ReturnAuthorized()
        {
            var user = new UserDto { Id = Guid.NewGuid(), Auth0UserId = "auth0 user id" };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null, user.Id, user.Auth0UserId))
            {
                var role = Fixture.Build<RoleEntity>().Create();

                var permission = Fixture
                    .Build<PermissionEntity>()
                    .With(p => p.Id, "MangeBuilding")
                    .Create();

                var rolePermission = Fixture
                    .Build<RolePermissionEntity>()
                    .With(r => r.RoleId, role.Id)
                    .With(r => r.PermissionId, permission.Id)
                    .Create();
                var customer = Fixture.Build<CustomerEntity>().Create();

                var portfolio = Fixture
                    .Build<PortfolioEntity>()
                    .With(p => p.CustomerId, customer.Id)
                    .Create();

                var site = Fixture
                    .Build<Site>()
                    .With(s => s.CustomerId, customer.Id)
                    .With(s => s.PortfolioId, portfolio.Id)
                    .Create();

                var assignment = Fixture
                    .Build<AssignmentEntity>()
                    .With(a => a.PrincipalId, user.Id)
                    .With(a => a.RoleId, role.Id)
                    .With(a => a.ResourceType, RoleResourceType.Customer)
                    .With(a => a.ResourceId, customer.Id)
                    .Create();

                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Roles.Add(role);
                dbContext.Permissions.Add(permission);
                dbContext.SaveChanges();
                dbContext.RolePermission.Add(rolePermission);
                dbContext.Assignments.Add(assignment);
                dbContext.Customers.Add(customer);
                dbContext.Portfolios.Add(portfolio);
                dbContext.SaveChanges();

                var url = $"sites/{site.Id}/extend";
                server
                    .Arrange()
                    .GetSiteCoreApi()
                    .SetupRequest(HttpMethod.Get, url)
                    .ReturnsJson(site);

                var response = await client.GetAsync(
                    $"users/{user.Id}/permissions/{permission.Id}/eligibility?siteId={site.Id}"
                );

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<AuthorizationInfo>();
                result.IsAuthorized.Should().Be(true);
            }
        }

        [Fact]
        public async Task SiteDoesNotExist_CheckPermission_ReturnNotFound()
        {
            var nonExistingSiteId = Guid.NewGuid();
            Site site = null;
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var url = $"sites/{nonExistingSiteId}/extend";
                server
                    .Arrange()
                    .GetSiteCoreApi()
                    .SetupRequest(HttpMethod.Get, url)
                    .ReturnsJson(site);

                var response = await client.GetAsync(
                    $"users/{Guid.NewGuid()}/permissions/ManageBuilding/eligibility?siteId={nonExistingSiteId}"
                );

                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
                var resultJson = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ErrorResponse>(resultJson);
                result.Message.Should().Contain("site");
            }
        }
    }
}
