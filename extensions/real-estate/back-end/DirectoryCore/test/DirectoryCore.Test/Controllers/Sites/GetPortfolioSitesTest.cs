using System;
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
using Willow.Calendar;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace DirectoryCore.Test.Controllers.Sites
{
    public class GetPortfolioSitesTest : BaseInMemoryTest
    {
        public GetPortfolioSitesTest(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public async Task PortfolioHasSites_GetPortfolioSitesTest_ReturnsThoseSites()
        {
            var customerId = Guid.NewGuid();
            var portfolioId = Guid.NewGuid();
            var expectedSites = Fixture
                .Build<Site>()
                .With(x => x.CustomerId, customerId)
                .With(x => x.PortfolioId, portfolioId)
                .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                .CreateMany()
                .ToList();
            var otherSites = Fixture
                .Build<Site>()
                .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                .CreateMany();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                await dbContext.SaveChangesAsync();

                var url = $"customers/{customerId}/portfolios/{portfolioId}/sites/extend";
                server
                    .Arrange()
                    .GetSiteCoreApi()
                    .SetupRequest(HttpMethod.Get, url)
                    .ReturnsJson(expectedSites);

                var response = await client.GetAsync(
                    $"customers/{customerId}/portfolios/{portfolioId}/sites"
                );
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<SiteDto>>();
                result.Should().BeEquivalentTo(SiteDto.MapFrom(expectedSites));
            }
        }
    }
}
