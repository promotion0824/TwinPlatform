using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using DirectoryCore.Domain;
using DirectoryCore.Entities;
using FluentAssertions;
using Willow.Infrastructure;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace DirectoryCore.Test.Controllers.Portfolios
{
    public class DeleteCustomerPortfolioTests : BaseInMemoryTest
    {
        public DeleteCustomerPortfolioTests(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public async Task PortfolioExists_DeleteCustomerPortfolio_DeletionSucceeds()
        {
            var portfolio = Fixture.Create<PortfolioEntity>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<DirectoryDbContext>();
                db.Portfolios.Add(portfolio);
                db.SaveChanges();

                var url =
                    $"customers/{portfolio.CustomerId}/portfolios/{portfolio.Id}/sites/extend";
                server
                    .Arrange()
                    .GetSiteCoreApi()
                    .SetupRequest(HttpMethod.Get, url)
                    .ReturnsJson(new List<Site>());

                var response = await client.DeleteAsync(
                    $"customers/{portfolio.CustomerId}/portfolios/{portfolio.Id}"
                );

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
                db = server.Assert().GetDirectoryDbContext();
                db.Portfolios.Should().BeEmpty();
            }
        }

        [Fact]
        public async Task PortfolioDoesExist_DeleteCustomerPortfolio_ReturnsNotFound()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var portfolioId = new Guid();
                var customerId = new Guid();

                var url = $"customers/{customerId}/portfolios/{portfolioId}/sites/extend";
                server
                    .Arrange()
                    .GetSiteCoreApi()
                    .SetupRequest(HttpMethod.Get, url)
                    .ReturnsJson(new List<Site>());

                var response = await client.DeleteAsync(
                    $"customers/{customerId}/portfolios/{portfolioId}"
                );

                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
                var error = await response.Content.ReadAsErrorResponseAsync();
                error.Message.Should().Contain("portfolio");
            }
        }

        [Fact]
        public async Task PortfolioHasAtLeastOneSite_DeleteCustomerPortfolio_ReturnsBadRequest()
        {
            var portfolio = Fixture.Create<PortfolioEntity>();
            var site = Fixture
                .Build<Site>()
                .With(x => x.CustomerId, portfolio.CustomerId)
                .With(x => x.PortfolioId, portfolio.Id)
                .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<DirectoryDbContext>();
                db.Portfolios.Add(portfolio);
                db.SaveChanges();

                var url =
                    $"customers/{portfolio.CustomerId}/portfolios/{portfolio.Id}/sites/extend";
                server
                    .Arrange()
                    .GetSiteCoreApi()
                    .SetupRequest(HttpMethod.Get, url)
                    .ReturnsJson(new List<Site>() { site });

                var response = await client.DeleteAsync(
                    $"customers/{portfolio.CustomerId}/portfolios/{portfolio.Id}"
                );

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var error = await response.Content.ReadAsErrorResponseAsync();
                error.Message.Should().Contain("has sites");
            }
        }
    }
}
