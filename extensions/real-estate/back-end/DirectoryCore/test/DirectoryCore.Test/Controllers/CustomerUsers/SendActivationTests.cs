using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using DirectoryCore.Dto;
using DirectoryCore.Entities;
using DirectoryCore.Enums;
using FluentAssertions;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace DirectoryCore.Test.Controllers.CustomerUsers
{
    public class SendActivationTests : BaseInMemoryTest
    {
        public SendActivationTests(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public async Task PendingCustomerUserExist_SendActivationEmail_ReturnNoContent()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var customer = Fixture.Build<CustomerEntity>().Create();
                var user = Fixture
                    .Build<UserEntity>()
                    .With(u => u.CustomerId, customer.Id)
                    .With(u => u.Status, UserStatus.Pending)
                    .Create();
                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Customers.Add(customer);
                dbContext.Users.Add(user);
                await dbContext.SaveChangesAsync();

                var response = await client.PostAsync(
                    $"customers/{customer.Id}/users/{user.Id}/sendActivation",
                    null
                );
                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }
    }
}
