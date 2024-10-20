using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AutoFixture;
using DirectoryCore.Controllers.Requests;
using DirectoryCore.Domain;
using DirectoryCore.Dto;
using DirectoryCore.Entities;
using FluentAssertions;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace DirectoryCore.Test.Controllers.Portfolios
{
    public class CreateCustomerPortfolioTests : BaseInMemoryTest
    {
        public CreateCustomerPortfolioTests(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public async Task PortfoliosExist_GetCustomerPortfolios_ReturnThosePortfolios()
        {
            var customerId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var customer = Fixture.Build<CustomerEntity>().With(c => c.Id, customerId).Create();

                var serverDbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                serverDbContext.Customers.Add(customer);
                serverDbContext.SaveChanges();

                var request = new CreatePortfolioRequest()
                {
                    Name = Guid.NewGuid().ToString(),
                    Features = new PortfolioFeatures { IsKpiDashboardEnabled = true }
                };
                var response = await client.PostAsJsonAsync(
                    $"customers/{customerId}/portfolios",
                    request
                );

                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                var createdEntity = dbContext.Portfolios.FirstOrDefault();
                createdEntity.Should().NotBeNull();
                createdEntity.Name.Should().Be(request.Name);
                createdEntity.CustomerId.Should().Be(customerId);

                var result = await response.Content.ReadAsAsync<PortfolioDto>();
                result.Should().NotBeNull();
                result.Name.Should().Be(request.Name);
            }
        }
    }
}
