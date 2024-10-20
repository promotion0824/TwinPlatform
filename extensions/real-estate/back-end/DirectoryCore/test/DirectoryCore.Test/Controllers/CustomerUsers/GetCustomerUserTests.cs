using System;
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
    public class GetCustomerUserTests : BaseInMemoryTest
    {
        public GetCustomerUserTests(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public async Task UserNotExist_GetCustomerUser_ReturnNotFound()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var customer = Fixture.Build<CustomerEntity>().Create();

                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Customers.Add(customer);
                await dbContext.SaveChangesAsync();

                var response = await client.GetAsync(
                    $"customers/{customer.Id}/users/{Guid.NewGuid()}"
                );
                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task CustomerNotExist_GetCustomerUser_ReturnNotFound()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var user = Fixture.Build<UserEntity>().Create();

                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Users.Add(user);
                await dbContext.SaveChangesAsync();

                var response = await client.GetAsync($"customers/{Guid.NewGuid()}/users/{user.Id}");
                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task CustomerUserExist_GetCustomerUser_ReturnUser()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var customer = Fixture.Build<CustomerEntity>().Create();

                var user = Fixture
                    .Build<UserEntity>()
                    .With(u => u.CustomerId, customer.Id)
                    .Create();

                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Customers.Add(customer);
                dbContext.Users.Add(user);
                await dbContext.SaveChangesAsync();

                var response = await client.GetAsync($"customers/{customer.Id}/users/{user.Id}");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<UserDto>();
                result
                    .Should()
                    .BeEquivalentTo(
                        user,
                        config =>
                        {
                            config.Excluding(p => p.EmailConfirmationTokenExpiry);
                            config.Excluding(p => p.EmailConfirmationToken);
                            config.Excluding(p => p.EmailConfirmed);
                            config.Excluding(p => p.Preferences);
                            config.Excluding(p => p.TimeSeries);
                            return config;
                        }
                    );
            }
        }
    }
}
