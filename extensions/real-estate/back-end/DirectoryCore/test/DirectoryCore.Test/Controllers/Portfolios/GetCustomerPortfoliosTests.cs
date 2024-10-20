using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using DirectoryCore.Domain;
using DirectoryCore.Dto;
using DirectoryCore.Entities;
using FluentAssertions;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace DirectoryCore.Test.Controllers.Portfolios
{
    public class GetCustomerPortfoliosTests : BaseInMemoryTest
    {
        public GetCustomerPortfoliosTests(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public async Task PortfoliosExist_GetCustomerPortfolios_ReturnThosePortfolios()
        {
            var customer = Fixture.Build<CustomerEntity>().Create();
            var portfolios = Fixture
                .Build<PortfolioEntity>()
                .With(x => x.CustomerId, customer.Id)
                .CreateMany(10)
                .ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Customers.Add(customer);
                dbContext.Portfolios.AddRange(portfolios);
                dbContext.SaveChanges();

                var response = await client.GetAsync($"customers/{customer.Id}/portfolios");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<PortfolioDto>>();
                result.Where(x => x.Sites != null).Should().BeEmpty();
                result
                    .Should()
                    .BeEquivalentTo(PortfolioDto.MapFrom(PortfolioEntity.MapTo(portfolios)));
            }
        }

        [Fact]
        public async Task PortfoliosExist_GetCustomerPortfoliosIncludeSites_ReturnThosePortfoliosWithSites()
        {
            var customer = Fixture.Build<CustomerEntity>().Create();
            var portfolio = Fixture
                .Build<PortfolioEntity>()
                .With(x => x.CustomerId, customer.Id)
                .Create();

            var site = Fixture
                .Build<Site>()
                .With(x => x.CustomerId, customer.Id)
                .With(x => x.PortfolioId, () => portfolio.Id)
                .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                await dbContext.Customers.AddAsync(customer);
                await dbContext.Portfolios.AddAsync(portfolio);
                await dbContext.SaveChangesAsync();

                var url = $"sites/customer/{customer.Id}/extend";
                server
                    .Arrange()
                    .GetSiteCoreApi()
                    .SetupRequest(HttpMethod.Get, url)
                    .ReturnsJson(new List<Site>() { site });

                var response = await client.GetAsync(
                    $"customers/{customer.Id}/portfolios?includeSites={true}"
                );

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<PortfolioDto>>();
                var returnedPortfolio = result.Single();
                returnedPortfolio.Id.Should().Be(portfolio.Id);
                returnedPortfolio.Name.Should().Be(portfolio.Name);
                var returnedSite = returnedPortfolio.Sites.Single();
                returnedSite.Id.Should().Be(site.Id);
                returnedSite.Name.Should().Be(site.Name);
            }
        }
    }
}
