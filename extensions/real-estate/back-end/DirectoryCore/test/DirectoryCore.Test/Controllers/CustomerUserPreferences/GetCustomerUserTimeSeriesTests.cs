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

namespace DirectoryCore.Test.Controllers.CustomerUserPreferences
{
    public class GetCustomerUserTimeSeriesTests : BaseInMemoryTest
    {
        public GetCustomerUserTimeSeriesTests(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public async Task NoCustomerUserTimeSeries_GetCustomerUserTimeSeries_ReturnNotFound()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var customerUserId = Guid.NewGuid();
                var customerId = Guid.NewGuid();

                var response = await client.GetAsync(
                    $"customers/{customerId}/users/{customerUserId}/preferences/timeSeries"
                );
                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task CustomerUserTimeSeriesExist_GetCustomerUserTimeSeries_ReturnCustomerUserTimeSeries()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var customerId = Guid.NewGuid();
                var customerUserId = Guid.NewGuid();
                var customerUserTimeSeries = Fixture
                    .Build<CustomerUserTimeSeriesEntity>()
                    .Without(x => x.User)
                    .With(x => x.CustomerUserId, customerUserId)
                    .With(x => x.State, "{}")
                    .With(x => x.RecentAssets, "{}")
                    .With(x => x.Favorites, "[]")
                    .With(x => x.ExportedCsvs, "[]")
                    .Create();
                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.CustomerUserTimeSeries.Add(customerUserTimeSeries);
                await dbContext.SaveChangesAsync();

                var response = await client.GetAsync(
                    $"customers/{customerId}/users/{customerUserId}/preferences/timeSeries"
                );
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<CustomerUserTimeSeriesDto>();
                var userTimeSeries = CustomerUserTimeSeriesEntity.MapTo(customerUserTimeSeries);
                result.State.ValueKind.Should().Be(userTimeSeries.State.ValueKind);
            }
        }
    }
}
