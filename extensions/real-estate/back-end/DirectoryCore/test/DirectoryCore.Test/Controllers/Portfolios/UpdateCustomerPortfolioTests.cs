using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using AutoFixture;
using DirectoryCore.Controllers.Requests;
using DirectoryCore.Dto;
using DirectoryCore.Entities;
using FluentAssertions;
using Willow.Infrastructure;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace DirectoryCore.Test.Controllers.Portfolios
{
    public class UpdateCustomerPortfolioTests : BaseInMemoryTest
    {
        public UpdateCustomerPortfolioTests(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public async Task PortfolioNotExist_UpdateCustomerPortfolio_ReturnsNotFound()
        {
            var customerId = Guid.NewGuid();
            var customer = Fixture.Build<CustomerEntity>().With(c => c.Id, customerId).Create();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var serverDbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                serverDbContext.Customers.Add(customer);
                serverDbContext.SaveChanges();

                var request = Fixture.Create<UpdatePortfolioRequest>();
                var response = await client.PutAsJsonAsync(
                    $"customers/{customerId}/portfolios/{Guid.NewGuid()}",
                    request
                );

                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
                var error = await response.Content.ReadAsErrorResponseAsync();
                error.Message.Should().Contain("portfolio");
            }
        }

        [Fact]
        public async Task PortfolioExists_UpdateCustomerPortfolio_ReturnUpdatedPortfolio()
        {
            var customerId = Guid.NewGuid();
            var customer = Fixture.Build<CustomerEntity>().With(c => c.Id, customerId).Create();
            var portfolio = Fixture
                .Build<PortfolioEntity>()
                .With(c => c.CustomerId, customerId)
                .With(c => c.FeaturesJson, "{\"isKpiDashboardEnabled\":false}")
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var serverDbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                serverDbContext.Customers.Add(customer);
                serverDbContext.Portfolios.Add(portfolio);
                serverDbContext.SaveChanges();

                var request = Fixture.Create<UpdatePortfolioRequest>();
                var response = await client.PutAsJsonAsync(
                    $"customers/{customerId}/portfolios/{portfolio.Id}",
                    request
                );

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                var updatedPortfolio = dbContext.Portfolios.FirstOrDefault();
                updatedPortfolio.Should().NotBeNull();
                updatedPortfolio.Name.Should().Be(request.Name);
                updatedPortfolio
                    .FeaturesJson.Should()
                    .Be(JsonSerializerExtensions.Serialize(request.Features));

                var result = await response.Content.ReadAsAsync<PortfolioDto>();
                result.Should().NotBeNull();
                result.Name.Should().Be(request.Name);
            }
        }
    }
}
