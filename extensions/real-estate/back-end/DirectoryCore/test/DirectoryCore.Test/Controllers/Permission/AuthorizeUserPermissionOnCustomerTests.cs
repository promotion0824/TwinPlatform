using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using DirectoryCore.Dto;
using DirectoryCore.Entities;
using DirectoryCore.Entities.Permission;
using FluentAssertions;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace DirectoryCore.Test.Controllers.Permission
{
    public class AuthorizeUserPermissionOnCustomerTests : BaseInMemoryTest
    {
        public AuthorizeUserPermissionOnCustomerTests(ITestOutputHelper output)
            : base(output)
        {
            Fixture
                .Behaviors.OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => Fixture.Behaviors.Remove(b));
            Fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        [Fact]
        public async Task AuthorizeCustomerResourcePermission_PermissionNotExists_ReturnNotAuthorized()
        {
            var user = new UserDto { Id = Guid.NewGuid(), Auth0UserId = "auth0 user id" };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null, user.Id, user.Auth0UserId))
            {
                var role = Fixture.Build<RoleEntity>().Create();

                var permission = Fixture
                    .Build<PermissionEntity>()
                    .With(p => p.Id, "MangeCustomer")
                    .Create();

                var rolePermission = Fixture
                    .Build<RolePermissionEntity>()
                    .With(r => r.RoleId, role.Id)
                    .With(r => r.PermissionId, permission.Id)
                    .Create();

                var customer = Fixture.Build<CustomerEntity>().Create();

                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Roles.Add(role);
                dbContext.Permissions.Add(permission);
                dbContext.SaveChanges();
                dbContext.RolePermission.Add(rolePermission);
                dbContext.Customers.Add(customer);
                dbContext.SaveChanges();

                var response = await client.GetAsync(
                    $"users/{user.Id}/permissions/{permission.Id}/eligibility?customerId={customer.Id}"
                );
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<AuthorizationInfo>();
                result.IsAuthorized.Should().Be(false);
            }
        }

        [Fact]
        public async Task AuthorizeCustomerResourcePermission_CustomerPermissionExists_ReturnAuthorized()
        {
            var user = new UserDto { Id = Guid.NewGuid(), Auth0UserId = "auth0 user id" };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null, user.Id, user.Auth0UserId))
            {
                var role = Fixture.Build<RoleEntity>().Create();

                var permission = Fixture
                    .Build<PermissionEntity>()
                    .With(p => p.Id, "MangeCustomer")
                    .Create();

                var rolePermission = Fixture
                    .Build<RolePermissionEntity>()
                    .With(r => r.RoleId, role.Id)
                    .With(r => r.PermissionId, permission.Id)
                    .Create();

                var customer = Fixture.Build<CustomerEntity>().Create();

                var assignment = Fixture
                    .Build<AssignmentEntity>()
                    .With(a => a.PrincipalId, user.Id)
                    .With(a => a.RoleId, role.Id)
                    .With(a => a.ResourceId, customer.Id)
                    .Create();

                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Customers.Add(customer);
                dbContext.Roles.Add(role);
                dbContext.Permissions.Add(permission);
                dbContext.SaveChanges();
                dbContext.RolePermission.Add(rolePermission);
                dbContext.Assignments.Add(assignment);
                dbContext.SaveChanges();

                var response = await client.GetAsync(
                    $"users/{user.Id}/permissions/{permission.Id}/eligibility?customerId={customer.Id}"
                );
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<AuthorizationInfo>();
                result.IsAuthorized.Should().Be(true);
            }
        }
    }
}
