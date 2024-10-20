using System;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using AutoFixture;
using DirectoryCore.Dto.Requests;
using DirectoryCore.Entities;
using FluentAssertions;
using Willow.Infrastructure;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace DirectoryCore.Test.Controllers.CustomerUserPreferences
{
    public class CreateOrUpdateCustomerUserTimeSeriesTests : BaseInMemoryTest
    {
        public CreateOrUpdateCustomerUserTimeSeriesTests(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public async Task ThereAreNoCustomerUserTimeSeries_CreateCustomerUserTimeSeries_ReturnNoContent()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var customerId = Guid.NewGuid();
                var customerUserId = Guid.NewGuid();

                var request = new CustomerUserTimeSeriesRequest
                {
                    State = JsonSerializerExtensions.Deserialize<JsonElement>("{}"),
                    RecentAssets = JsonSerializerExtensions.Deserialize<JsonElement>("{}"),
                    ExportedCsvs = JsonSerializerExtensions.Deserialize<JsonElement>("[]"),
                    Favorites = JsonSerializerExtensions.Deserialize<JsonElement>("[]")
                };
                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                var response = await client.PutAsJsonAsync(
                    $"customers/{customerId}/users/{customerUserId}/preferences/timeSeries",
                    request
                );
                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
                dbContext.CustomerUserTimeSeries.Should().HaveCount(1);
            }
        }

        [Fact]
        public async Task CustomerUserTimeSeriesExist_CreateCustomerUserTimeSeries_ReturnNoContent()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var customerId = Guid.NewGuid();
                var customerUserId = Guid.NewGuid();
                var expectedCustUserTimeSeries = Fixture
                    .Build<CustomerUserTimeSeriesEntity>()
                    .Without(x => x.User)
                    .With(x => x.CustomerUserId, customerUserId)
                    .Create();

                var request = new CustomerUserTimeSeriesRequest
                {
                    State = JsonSerializerExtensions.Deserialize<JsonElement>("{}"),
                    RecentAssets = JsonSerializerExtensions.Deserialize<JsonElement>("{}"),
                    ExportedCsvs = JsonSerializerExtensions.Deserialize<JsonElement>("[]"),
                    Favorites = JsonSerializerExtensions.Deserialize<JsonElement>("[]")
                };

                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.CustomerUserTimeSeries.Add(expectedCustUserTimeSeries);
                await dbContext.SaveChangesAsync();

                var response = await client.PutAsJsonAsync(
                    $"customers/{customerId}/users/{customerUserId}/preferences/timeSeries",
                    request
                );
                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
                var customerUserTimeSeries = dbContext.CustomerUserTimeSeries.FirstOrDefault();
                customerUserTimeSeries.State.Should().Be("{}");
            }
        }
    }
}
