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
using Newtonsoft.Json;
using Willow.Infrastructure;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace DirectoryCore.Test.Controllers.Sites
{
    public class CreateSiteTests : BaseInMemoryTest
    {
        public CreateSiteTests(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public async Task GivenValidInput_CreateSite_ReturnsTheNewSite()
        {
            var customer = Fixture.Build<CustomerEntity>().Create();
            var portfolio = Fixture
                .Build<PortfolioEntity>()
                .With(x => x.CustomerId, customer.Id)
                .Create();
            var request = Fixture
                .Build<CreateSiteRequest>()
                .With(s => s.Code, Guid.NewGuid().ToString().Substring(0, 20))
                .With(s => s.Name, Guid.NewGuid().ToString())
                .With(s => s.TimeZoneId, "AUS Eastern Standard Time")
                .With(s => s.Features, new Domain.SiteFeatures { IsHideOccurrencesEnabled = true })
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Customers.Add(customer);
                dbContext.Portfolios.Add(portfolio);
                await dbContext.SaveChangesAsync();

                var url = $"customers/{customer.Id}/portfolios/{portfolio.Id}/sites/extend";
                server
                    .Arrange()
                    .GetSiteCoreApi()
                    .SetupRequest(HttpMethod.Post, url)
                    .ReturnsJson(new Site() { Id = request.Id, Name = request.Name });

                var response = await client.PostAsJsonAsync(
                    $"customers/{customer.Id}/portfolios/{portfolio.Id}/sites",
                    request
                );

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<SiteDto>();
                result.Id.Should().Be(request.Id);
                result.Name.Should().Be(request.Name);
            }
        }

        [Fact]
        public async Task GivenInvalidInput_CreateSite_ReturnsBadRequest()
        {
            var request = new { Name = "abc" };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.PostAsJsonAsync(
                    $"customers/{Guid.NewGuid()}/portfolios/{Guid.NewGuid()}/sites",
                    request
                );

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var resultJson = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ErrorResponse>(resultJson);
                result.Message.Should().Contain("Site id");
            }
        }

        [Fact]
        public async Task TimeZoneIdIsWrong_CreateSite_ReturnsBadRequest()
        {
            var customerId = Guid.NewGuid();
            var portfolioId = Guid.NewGuid();

            var createSiteRequest = Fixture
                .Build<CreateSiteRequest>()
                .With(x => x.TimeZoneId, "wrong timezone id")
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.PostAsJsonAsync(
                    $"customers/{customerId}/portfolios/{portfolioId}/sites",
                    createSiteRequest
                );

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var resultJson = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ErrorResponse>(resultJson);
                result.Message.Should().Contain("timezone");
            }
        }
    }
}
