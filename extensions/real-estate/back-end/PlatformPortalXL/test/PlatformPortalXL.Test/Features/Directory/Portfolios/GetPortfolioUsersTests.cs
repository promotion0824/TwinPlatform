using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using Willow.Platform.Models;
using Willow.Platform.Users;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Directory.Portfolios
{
    public class GetPortfolioUsersTests : BaseInMemoryTest
    {
        public GetPortfolioUsersTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task PortfolioHasUsers_GetPortfolioUsers_ReturnUsersList()
        {
            var customerId = Guid.NewGuid();
            var user = Fixture.Build<User>().With(x => x.CustomerId, customerId).Create();
            var allowedPortfolios = Fixture.CreateMany<Portfolio>(10);
            var deniedPortfolios = Fixture.CreateMany<Portfolio>(10);
            var allPortfolios = allowedPortfolios.Union(deniedPortfolios);
            var portfolioId = allowedPortfolios.First().Id;
            var portfolioUsers = Fixture.Build<User>().CreateMany(3);

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, user.Id))
            {
                var handler = server.Arrange().GetDirectoryApi();
                handler.SetupRequest(HttpMethod.Get, $"users/{user.Id}")
                       .ReturnsJson(user);
                handler.SetupRequest(HttpMethod.Get, $"customers/{customerId}/portfolios")
                       .ReturnsJson(allPortfolios);
                handler.SetupRequest(HttpMethod.Get, $"portfolios/{portfolioId}/users")
                       .ReturnsJson(portfolioUsers);
                foreach (var portfolio in allPortfolios)
                {
                    var isAllowed = allowedPortfolios.Any(x => x.Id == portfolio.Id);
                    handler
                        .SetupRequest(HttpMethod.Get, $"users/{user.Id}/permissions/{Permissions.ViewPortfolios}/eligibility?portfolioId={portfolio.Id}")
                        .ReturnsJson(new { IsAuthorized = isAllowed });
                }

                var response = await client.GetAsync($"customers/{customerId}/portfolios/{portfolioId}/users");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<User>>();
                result.Should().BeEquivalentTo(UserSimpleDto.Map(portfolioUsers));
            }
        }

        [Fact]
        public async Task PortfolioDoesNotExist_GetPortfolioUsers_ReturnNotFound()
        {
            var customerId = Guid.NewGuid();
            var user = Fixture.Build<User>().With(x => x.CustomerId, customerId).Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, user.Id))
            {
                var handler = server.Arrange().GetDirectoryApi();
                handler.SetupRequest(HttpMethod.Get, $"users/{user.Id}")
                       .ReturnsJson(user);
                handler.SetupRequest(HttpMethod.Get, $"customers/{customerId}/portfolios")
                       .ReturnsJson(new Portfolio[0]);

                var response = await client.GetAsync($"customers/{customerId}/portfolios/{Guid.NewGuid()}/users");
                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task UserDoesNotHavePermissionOnPortfolio_GetPortfolioUsers_ReturnsForbidden()
        {
            var customerId = Guid.NewGuid();
            var user = Fixture.Build<User>().With(x => x.CustomerId, customerId).Create();
            var allowedPortfolios = Fixture.CreateMany<Portfolio>(10);
            var deniedPortfolios = Fixture.CreateMany<Portfolio>(10);
            var allPortfolios = allowedPortfolios.Union(deniedPortfolios);
            var portfolioId = allowedPortfolios.First().Id;

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, user.Id))
            {
                var handler = server.Arrange().GetDirectoryApi();
                handler.SetupRequest(HttpMethod.Get, $"users/{user.Id}")
                       .ReturnsJson(user);
                handler.SetupRequest(HttpMethod.Get, $"customers/{customerId}/portfolios")
                       .ReturnsJson(allPortfolios);
                foreach (var portfolio in allPortfolios)
                {
                    var isAllowed = allowedPortfolios.Any(x => x.Id == portfolio.Id);
                    handler
                        .SetupRequest(HttpMethod.Get, $"users/{user.Id}/permissions/{Permissions.ViewPortfolios}/eligibility?portfolioId={portfolio.Id}")
                        .ReturnsJson(new { IsAuthorized = !isAllowed });
                }

                var response = await client.GetAsync($"customers/{customerId}/portfolios/{portfolioId}/users");
                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

    }
}