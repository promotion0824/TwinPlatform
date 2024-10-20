using System;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AutoFixture;
using DirectoryCore.Dto.Requests;
using DirectoryCore.Entities;
using DirectoryCore.Entities.Permission;
using DirectoryCore.Enums;
using FluentAssertions;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace DirectoryCore.Test.Controllers.CustomerUsers
{
    public class InactivateCustomerUserTests : BaseInMemoryTest
    {
        public InactivateCustomerUserTests(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public async Task UserNotExist_InactivateCustomerUser_ReturnNotFound()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var customer = Fixture.Create<CustomerEntity>();
                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Customers.Add(customer);
                await dbContext.SaveChangesAsync();

                var response = await client.PutAsJsonAsync(
                    $"customers/{customer.Id}/users/{Guid.NewGuid()}/status",
                    new UpdateUserStatusRequest { Status = UserStatus.Inactive }
                );
                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task CustomerUserExist_NotInactiveStatusRequested_InactivateCustomerUser_ReturnBadRequest()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var customer = Fixture.Create<CustomerEntity>();
                var existingCustomerUser = Fixture.Create<UserEntity>();

                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Customers.Add(customer);
                dbContext.Users.Add(existingCustomerUser);
                await dbContext.SaveChangesAsync();

                var response = await client.PutAsJsonAsync(
                    $"customers/{customer.Id}/users/{existingCustomerUser.Id}/status",
                    new UpdateUserStatusRequest { Status = UserStatus.Active }
                );
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task CustomerUserExist_InactivateCustomerUser_IsCustomerUserFlagChangedToFalseAndReturnNoContent()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var customer = Fixture.Create<CustomerEntity>();

                var existingCustomerUser = Fixture.Create<UserEntity>();

                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Customers.Add(customer);
                dbContext.Users.Add(existingCustomerUser);
                await dbContext.SaveChangesAsync();

                var response = await client.PutAsJsonAsync(
                    $"customers/{customer.Id}/users/{existingCustomerUser.Id}/status",
                    new UpdateUserStatusRequest { Status = UserStatus.Inactive }
                );
                response.StatusCode.Should().Be(HttpStatusCode.NoContent);

                var dbContextInServer = server.Arrange().CreateDbContext<DirectoryDbContext>();
                var deletedMobileUser = dbContextInServer.Users.FirstOrDefault(
                    u => u.Id == existingCustomerUser.Id
                );
            }
        }

        [Fact]
        public async Task NonMobileUserExist_InactivateCustomerUser_UserInactivatedAndReturnNoContent()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var customer = Fixture.Create<CustomerEntity>();
                var existingCustomerUser = Fixture.Create<UserEntity>();

                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Users.Add(existingCustomerUser);
                await dbContext.SaveChangesAsync();

                var response = await client.PutAsJsonAsync(
                    $"customers/{customer.Id}/users/{existingCustomerUser.Id}/status",
                    new UpdateUserStatusRequest { Status = UserStatus.Inactive }
                );
                response.StatusCode.Should().Be(HttpStatusCode.NoContent);

                var dbContextInServer = server.Arrange().CreateDbContext<DirectoryDbContext>();
                var deletedMobileUser = dbContextInServer.Users.FirstOrDefault(
                    u => u.Id == existingCustomerUser.Id
                );
                deletedMobileUser
                    .LastName.Should()
                    .BeEquivalentTo($"{existingCustomerUser.LastName}(Inactive)");
                deletedMobileUser.Status.Should().BeEquivalentTo(UserStatus.Inactive);
            }
        }

        [Fact]
        public async Task UserExist_AssignmentsExist_InactivateCustomerUser_UserInactivatedAndReturnNoContent()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var customer = Fixture.Create<CustomerEntity>();
                var role = Fixture.Create<RoleEntity>();

                var existingCustomerUser = Fixture.Create<UserEntity>();
                var existingAssignment = Fixture
                    .Build<AssignmentEntity>()
                    .With(a => a.PrincipalId, existingCustomerUser.Id)
                    .With(a => a.RoleId, role.Id)
                    .Create();

                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Customers.Add(customer);
                dbContext.Roles.Add(role);
                await dbContext.SaveChangesAsync();
                dbContext.Users.Add(existingCustomerUser);
                dbContext.Assignments.Add(existingAssignment);
                await dbContext.SaveChangesAsync();

                var response = await client.PutAsJsonAsync(
                    $"customers/{customer.Id}/users/{existingCustomerUser.Id}/status",
                    new UpdateUserStatusRequest { Status = UserStatus.Inactive }
                );
                response.StatusCode.Should().Be(HttpStatusCode.NoContent);

                var dbContextInServer = server.Arrange().CreateDbContext<DirectoryDbContext>();
                var deletedCustomerUser = dbContextInServer.Users.FirstOrDefault(
                    c => c.Id == existingCustomerUser.Id
                );
                var assignment = dbContextInServer.Assignments.FirstOrDefault(
                    a => a.PrincipalId == existingCustomerUser.Id
                );
                deletedCustomerUser
                    .LastName.Should()
                    .BeEquivalentTo($"{existingCustomerUser.LastName}(Inactive)");
                deletedCustomerUser.Status.Should().BeEquivalentTo(UserStatus.Inactive);
                assignment.Should().BeEquivalentTo(existingAssignment);
                var blockedAuth0User = server
                    .Assert()
                    .GetAuth0ManagementService()
                    .BlockedAuth0UserIds;
                blockedAuth0User.Should().HaveCount(1);
            }
        }
    }
}
