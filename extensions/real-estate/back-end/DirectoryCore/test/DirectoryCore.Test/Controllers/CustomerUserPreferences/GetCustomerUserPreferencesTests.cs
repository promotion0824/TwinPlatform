using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AutoFixture;
using DirectoryCore.Entities;
using DirectoryCore.Enums;
using FluentAssertions;
using Willow.Infrastructure;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace DirectoryCore.Test.Controllers.CustomerUserPreferences
{
    public class GetCustomerUserPreferencesTests : BaseInMemoryTest
    {
        public GetCustomerUserPreferencesTests(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public async Task NoCustomerUserPreferences_GetCustomerUserPreferences_ReturnDefaultPreferences()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var customerUserId = Guid.NewGuid();
                var customerId = Guid.NewGuid();
                var defaultCustomerUserPreferences = new Domain.CustomerUserPreferences
                {
                    MobileNotificationEnabled = true,
                    Profile = JsonSerializerExtensions.Deserialize<JsonElement>("{}")
                };

                var response = await client.GetAsync(
                    $"customers/{customerId}/users/{customerUserId}/preferences"
                );
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<Domain.CustomerUserPreferences>();
                result
                    .MobileNotificationEnabled.Should()
                    .Be(defaultCustomerUserPreferences.MobileNotificationEnabled);
                result
                    .Profile.ValueKind.Should()
                    .Be(defaultCustomerUserPreferences.Profile.ValueKind);
            }
        }

        [Fact]
        public async Task CustomerUserPreferenceExist_GetCustomerUserPreferences_ReturnCustomerUserPreferences()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var customerId = Guid.NewGuid();
                var customerUserId = Guid.NewGuid();
                var customerUserPreferences = Fixture
                    .Build<CustomerUserPreferencesEntity>()
                    .With(x => x.CustomerUserId, customerUserId)
                    .With(x => x.Profile, "{}")
                    .Create();
                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.CustomerUserPreferences.Add(customerUserPreferences);
                await dbContext.SaveChangesAsync();

                var response = await client.GetAsync(
                    $"customers/{customerId}/users/{customerUserId}/preferences"
                );
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<Domain.CustomerUserPreferences>();
                var userPreferences = CustomerUserPreferencesEntity.MapTo(customerUserPreferences);
                result
                    .MobileNotificationEnabled.Should()
                    .Be(userPreferences.MobileNotificationEnabled);
                result.Profile.ValueKind.Should().Be(userPreferences.Profile.ValueKind);
            }
        }
    }
}
