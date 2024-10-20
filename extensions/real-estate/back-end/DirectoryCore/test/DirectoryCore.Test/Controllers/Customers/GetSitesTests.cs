using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using DirectoryCore.Domain;
using DirectoryCore.Dto;
using DirectoryCore.Entities;
using FluentAssertions;
using Willow.Calendar;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace DirectoryCore.Test.Controllers.Customers
{
    public class GetSitesTests : BaseInMemoryTest
    {
        public GetSitesTests(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public async Task CustomerSitesExist_GetSites_ReturnCustomerSites()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var customer = Fixture.Build<CustomerEntity>().Create();

                var customerSiteCount = 5;
                var sites = new List<Site>();
                for (int i = 0; i < customerSiteCount; i++)
                {
                    var site = Fixture
                        .Build<Site>()
                        .With(s => s.CustomerId, customer.Id)
                        .With(s => s.TimezoneId, "AUS Eastern Standard Time")
                        .Create();
                    sites.Add(site);
                }

                var anotherCustomer = Fixture.Build<CustomerEntity>().Create();
                var notCustomerSite = Fixture
                    .Build<Site>()
                    .With(s => s.CustomerId, anotherCustomer.Id)
                    .Create();

                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Customers.Add(customer);
                dbContext.Customers.Add(anotherCustomer);
                dbContext.SaveChanges();

                var url = $"sites/customer/{customer.Id}/extend";
                server
                    .Arrange()
                    .GetSiteCoreApi()
                    .SetupRequest(HttpMethod.Get, url)
                    .ReturnsJson(sites);

                var response = await client.GetAsync($"customers/{customer.Id}/sites");
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<List<SiteDto>>();
                result.Should().HaveCount(customerSiteCount);
                result.Should().BeEquivalentTo(SiteDto.MapFrom(sites));
            }
        }

        [Fact]
        public async Task QueryProvided_GetSites_ReturnMatchedCustomerSites()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var customer = Fixture.Build<CustomerEntity>().Create();

                var customerSiteNameKeyword = "sitename";
                var customerSiteCount = 5;
                var sites = new List<Site>();
                for (int i = 0; i < customerSiteCount; i++)
                {
                    var site = Fixture
                        .Build<Site>()
                        .With(s => s.Name, customerSiteNameKeyword + i)
                        .With(s => s.CustomerId, customer.Id)
                        .With(s => s.TimezoneId, "AUS Eastern Standard Time")
                        .Create();
                    sites.Add(site);
                }

                var nameKeywordNotMatchedCustomerSite = Fixture
                    .Build<Site>()
                    .With(s => s.CustomerId, customer.Id)
                    .Create();

                var anotherCustomer = Fixture.Build<CustomerEntity>().Create();

                var nonCustomerSite = Fixture
                    .Build<Site>()
                    .With(s => s.CustomerId, anotherCustomer.Id)
                    .Create();

                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Customers.Add(customer);
                dbContext.Customers.Add(anotherCustomer);
                dbContext.SaveChanges();

                var url = $"sites/customer/{customer.Id}/extend";
                server
                    .Arrange()
                    .GetSiteCoreApi()
                    .SetupRequest(HttpMethod.Get, url)
                    .ReturnsJson(sites);

                var response = await client.GetAsync(
                    $"customers/{customer.Id}/sites?query={customerSiteNameKeyword}"
                );
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<List<SiteDto>>();
                result.Should().HaveCount(customerSiteCount);
                result.Should().BeEquivalentTo(SiteDto.MapFrom(sites));
            }
        }
    }
}
