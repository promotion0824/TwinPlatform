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
    public class GetPortfoliosTests : BaseInMemoryTest
    {
        public GetPortfoliosTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task CustomerHasPortfolios_GetPortfolios_ReturnsPortfoliosUserCanAccess()
        {
            var customerId = Guid.NewGuid();
            var user = Fixture.Build<User>().With(x => x.CustomerId, customerId).Create();
            var allowedPortfolios = Fixture.CreateMany<Portfolio>(10);
            var deniedPortfolios = Fixture.CreateMany<Portfolio>(10);
            var allPortfolios = allowedPortfolios.Union(deniedPortfolios);

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, user.Id))
            {
                var handler = server.Arrange().GetDirectoryApi();
                handler
                    .SetupRequest(HttpMethod.Get, $"users/{user.Id}")
                    .ReturnsJson(user);
                handler
                    .SetupRequest(HttpMethod.Get, $"customers/{customerId}/portfolios?includeSites={true}")
                    .ReturnsJson(allPortfolios);
                foreach (var portfolio in allPortfolios)
                {
                    var isAllowed = allowedPortfolios.Any(x => x.Id == portfolio.Id);
                    handler
                        .SetupRequest(HttpMethod.Get, $"users/{user.Id}/permissions/{Permissions.ViewPortfolios}/eligibility?portfolioId={portfolio.Id}")
                        .ReturnsJson(new { IsAuthorized = isAllowed });
                }

                var response = await client.GetAsync($"me/portfolios");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<PortfolioDto>>();
                result.Should().BeEquivalentTo(PortfolioDto.MapFrom(allowedPortfolios));
            }
        }

    }
}