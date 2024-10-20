using System;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using AutoFixture;
using DirectoryCore.Dto.Requests;
using DirectoryCore.Entities;
using DirectoryCore.Enums;
using FluentAssertions;
using Willow.Infrastructure;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace DirectoryCore.Test.Controllers.CustomerUserPreferences
{
    public class CreateOrUpdateCustomerUserPreferencesTests : BaseInMemoryTest
    {
        public CreateOrUpdateCustomerUserPreferencesTests(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public async Task ThereAreNoCustomerUserPreferences_CreateCustomerUserPreferences_ReturnNoContent()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var customerId = Guid.NewGuid();
                var customerUserId = Guid.NewGuid();

                var req = new CustomerUserPreferencesRequest
                {
                    Profile = JsonSerializerExtensions.Deserialize<JsonElement>("{}")
                };
                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                var response = await client.PutAsJsonAsync(
                    $"customers/{customerId}/users/{customerUserId}/preferences",
                    req
                );
                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
                dbContext.CustomerUserPreferences.Should().HaveCount(1);
            }
        }

        [Fact]
        public async Task CustomerUserPreferencesExist_CreateCustomerUserPreferences_ReturnNoContent()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var customerId = Guid.NewGuid();
                var customerUserId = Guid.NewGuid();
                var expectedCustUserPref = Fixture
                    .Build<CustomerUserPreferencesEntity>()
                    .Without(x => x.User)
                    .With(x => x.CustomerUserId, customerUserId)
                    .Create();

                var req = new CustomerUserPreferencesRequest
                {
                    MobileNotificationEnabled = false,
                    Profile = JsonSerializerExtensions.Deserialize<JsonElement>("{}")
                };

                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.CustomerUserPreferences.Add(expectedCustUserPref);
                await dbContext.SaveChangesAsync();

                var response = await client.PutAsJsonAsync(
                    $"customers/{customerId}/users/{customerUserId}/preferences",
                    req
                );
                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
                var customerUserPreferences = dbContext.CustomerUserPreferences.FirstOrDefault();
                customerUserPreferences.MobileNotificationEnabled.Should().Be(false);
                customerUserPreferences.CustomerUserId.Should().Be(customerUserId);
            }
        }

        [Theory]
        [InlineData("en")]
        [InlineData("fr")]
        [InlineData("en-US")]
        [InlineData("fr-CA")]
        [InlineData(null)]
        public async Task ValidLanguage_CreateCustomerUserPreferences_ReturnNoContent(
            string language
        )
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var customerId = Guid.NewGuid();
                var customerUserId = Guid.NewGuid();

                var req = new CustomerUserPreferencesRequest
                {
                    Language = language,
                    Profile = JsonSerializerExtensions.Deserialize<JsonElement>("{}")
                };
                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                var response = await client.PutAsJsonAsync(
                    $"customers/{customerId}/users/{customerUserId}/preferences",
                    req
                );
                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
                dbContext.CustomerUserPreferences.Should().HaveCount(1);
            }
        }

        [Theory]
        [InlineData("zz")]
        [InlineData("zz-ZZ")]
        [InlineData("1")]
        [InlineData("1-1")]
        public async Task InvalidLanguage_CreateCustomerUserPreferences_ReturnBadRequest(
            string language
        )
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var customerId = Guid.NewGuid();
                var customerUserId = Guid.NewGuid();

                var req = new CustomerUserPreferencesRequest
                {
                    Language = language,
                    Profile = JsonSerializerExtensions.Deserialize<JsonElement>("{}")
                };
                var response = await client.PutAsJsonAsync(
                    $"customers/{customerId}/users/{customerUserId}/preferences",
                    req
                );
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }
    }
}
