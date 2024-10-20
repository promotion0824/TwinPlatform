using System;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AutoFixture;
using DirectoryCore.Dto.Requests;
using DirectoryCore.Entities;
using FluentAssertions;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace DirectoryCore.Test.Controllers.CustomerUsers
{
    public class UpdateCustomerUserTests : BaseInMemoryTest
    {
        public UpdateCustomerUserTests(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public async Task UserNotExist_UpdateCustomerUser_ReturnNotFound()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var customer = Fixture.Build<CustomerEntity>().Create();

                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Customers.Add(customer);
                await dbContext.SaveChangesAsync();

                var userRequest = new UpdateCustomerUserRequest
                {
                    FirstName = "firstName",
                    LastName = "lastName",
                    Mobile = "0411222333",
                    Company = "company"
                };

                var response = await client.PutAsJsonAsync(
                    $"customers/{customer.Id}/users/{Guid.NewGuid()}",
                    userRequest
                );
                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task UserExist_UpdateCustomerUser_CustomerUserUpdatedAndReturnNoContent()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var customer = Fixture.Build<CustomerEntity>().Create();

                var existingCustomerUser = Fixture
                    .Build<UserEntity>()
                    .With(u => u.CustomerId, customer.Id)
                    .Create();

                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Customers.Add(customer);
                dbContext.Users.Add(existingCustomerUser);
                await dbContext.SaveChangesAsync();

                var userRequest = new UpdateCustomerUserRequest
                {
                    FirstName = "firstName",
                    LastName = "lastName",
                    Mobile = "0411222333",
                    Company = "company"
                };

                var response = await client.PutAsJsonAsync(
                    $"customers/{customer.Id}/users/{existingCustomerUser.Id}",
                    userRequest
                );
                response.StatusCode.Should().Be(HttpStatusCode.NoContent);

                var dbContextInServer = server.Arrange().CreateDbContext<DirectoryDbContext>();
                var updatedUser = dbContextInServer.Users.FirstOrDefault(
                    u => u.Id == existingCustomerUser.Id
                );
                updatedUser.FirstName.Should().Be(userRequest.FirstName);
                updatedUser.LastName.Should().Be(userRequest.LastName);
                updatedUser.Mobile.Should().Be(userRequest.Mobile);
                updatedUser.Company.Should().Be(userRequest.Company);
            }
        }
    }
}
