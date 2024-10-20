using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using AutoFixture;
using DirectoryCore.Dto;
using DirectoryCore.Dto.Requests;
using DirectoryCore.Entities;
using FluentAssertions;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace DirectoryCore.Test.Controllers.CustomerUsers
{
    public class CreateCustomerUserTests : BaseInMemoryTest
    {
        public CreateCustomerUserTests(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public async Task GivenInvalidInput_CreateCustomerUser_ReturnNewUser()
        {
            var createUserRequest = Fixture.Create<CreateCustomerUserRequest>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var customer = Fixture.Build<CustomerEntity>().Create();
                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Customers.Add(customer);
                await dbContext.SaveChangesAsync();

                var response = await client.PostAsJsonAsync(
                    $"customers/{customer.Id}/users",
                    createUserRequest
                );

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<UserDto>();
                result.FirstName.Should().Be(createUserRequest.FirstName);
                result.LastName.Should().Be(createUserRequest.LastName);
                result.Email.Should().Be(createUserRequest.Email);
                dbContext = server.Assert().GetDirectoryDbContext();
                dbContext.Users.Should().HaveCount(1);
                var createdUser = dbContext.Users.First();
                createdUser.CustomerId.Should().Be(customer.Id);
                createdUser.FirstName.Should().Be(createUserRequest.FirstName);
                createdUser.LastName.Should().Be(createUserRequest.LastName);
                createdUser.Email.Should().Be(createUserRequest.Email);
                createdUser.Company.Should().Be(createUserRequest.Company);
            }
        }

        [Fact]
        public async Task GivenExistingCustomerUser_CreateCustomerUser_ReturnBadRequest()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var customer = Fixture.Build<CustomerEntity>().Create();

                var existingCustomerUserEmail = "customer@email.com";
                var existingCustomerUser = Fixture
                    .Build<UserEntity>()
                    .With(u => u.Email, existingCustomerUserEmail)
                    .Create();
                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Customers.Add(customer);
                dbContext.Users.Add(existingCustomerUser);
                await dbContext.SaveChangesAsync();

                var userRequest = new CreateCustomerUserRequest
                {
                    Email = existingCustomerUserEmail,
                    FirstName = "firstName",
                    LastName = "lastName",
                };

                var response = await client.PostAsJsonAsync(
                    $"customers/{customer.Id}/users",
                    userRequest
                );
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task GivenNotExistCustomerUser_CreateCustomerUser_ReturnNotFound()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var existingCustomerUserEmail = "customer@email.com";
                var existingCustomerUser = Fixture
                    .Build<UserEntity>()
                    .With(u => u.Email, existingCustomerUserEmail)
                    .Create();
                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Users.Add(existingCustomerUser);
                await dbContext.SaveChangesAsync();

                var userRequest = new CreateCustomerUserRequest
                {
                    Email = existingCustomerUserEmail,
                    FirstName = "firstName",
                    LastName = "lastName",
                };

                var response = await client.PostAsJsonAsync(
                    $"customers/{Guid.NewGuid()}/users",
                    userRequest
                );
                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        private static string GetToken(ServerArrangement arrangement, string email)
        {
            var emailsList = arrangement.GetEmailService().GetEmails();
            var emailReceivedForNewUser = emailsList[email];

            var url = Regex.Matches(emailReceivedForNewUser, @"\?t\=(.*?)\'").FirstOrDefault();
            url.Should().NotBeNull();
            var uri = new Uri("http://localhost" + url.Value.Replace("'", string.Empty));
            var token = HttpUtility.ParseQueryString(uri.Query).Get("t");

            return token;
        }
    }
}
