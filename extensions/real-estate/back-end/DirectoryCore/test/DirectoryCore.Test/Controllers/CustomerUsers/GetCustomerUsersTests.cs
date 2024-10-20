using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using DirectoryCore.Dto;
using DirectoryCore.Entities;
using FluentAssertions;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace DirectoryCore.Test.Controllers.CustomerUsers
{
    public class GetCustomerUsersTests : BaseInMemoryTest
    {
        public GetCustomerUsersTests(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public async Task CustomerNotExist_GetCustomerUsers_ReturnEmptyList()
        {
            var customerId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.GetAsync($"customers/{Guid.NewGuid()}/users");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<IList<UserDto>>();
                result.Should().BeEmpty();
            }
        }

        [Fact]
        public async Task CustomerUsersExist_GetCustomerUsers_ReturnCustomerUsers()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var customer = Fixture.Build<CustomerEntity>().Create();

                var users = Fixture
                    .Build<UserEntity>()
                    .With(u => u.CustomerId, customer.Id)
                    .CreateMany(5);

                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Customers.Add(customer);
                dbContext.Users.AddRange(users);
                await dbContext.SaveChangesAsync();

                var response = await client.GetAsync($"customers/{customer.Id}/users");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<IList<UserDto>>();
                result.Should().HaveCount(5);
                result.Should().BeEquivalentTo(UserDto.MapFrom(UserEntity.MapTo(users.ToList())));
            }
        }
    }
}
