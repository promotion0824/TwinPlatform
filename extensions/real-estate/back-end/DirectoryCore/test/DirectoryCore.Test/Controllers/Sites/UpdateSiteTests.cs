using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using AutoFixture;
using Castle.Core.Resource;
using DirectoryCore.Controllers.Requests;
using DirectoryCore.Domain;
using DirectoryCore.Entities;
using FluentAssertions;
using Willow.Calendar;
using Willow.Infrastructure;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace DirectoryCore.Test.Controllers.Sites
{
    public class UpdateSiteTests : BaseInMemoryTest
    {
        public UpdateSiteTests(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public async Task SiteDoesNotExist_UpdateSite_ReturnNotFound()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var siteId = Guid.NewGuid();
                var customerId = Guid.NewGuid();
                var portfolioId = Guid.NewGuid();

                Site site = null;
                var url = $"sites/{siteId}/extend";
                server
                    .Arrange()
                    .GetSiteCoreApi()
                    .SetupRequest(HttpMethod.Get, url)
                    .ReturnsJson(site);

                var response = await client.PutAsJsonAsync(
                    $"customers/{customerId}/portfolios/{portfolioId}/sites/{siteId}",
                    new UpdateSiteRequest()
                );
                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
                var error = await response.Content.ReadAsErrorResponseAsync();
                error.Message.Should().Contain($"Resource(site: {siteId}) cannot be found. ");
            }
        }

        [Fact]
        public async Task TimeZoneIdIsWrong_UpdateSite_ReturnsBadRequest()
        {
            var siteId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var portfolioId = Guid.NewGuid();
            var site = Fixture
                .Build<Site>()
                .With(x => x.Id, siteId)
                .With(x => x.CustomerId, customerId)
                .With(x => x.PortfolioId, portfolioId)
                .With(x => x.Features, new SiteFeatures())
                .With(s => s.TimezoneId, "AUS Eastern Standard Time")
                .Create();

            var updateSiteRequest = Fixture
                .Build<UpdateSiteRequest>()
                .With(x => x.TimeZoneId, "wrong timezone id")
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                await dbContext.SaveChangesAsync();

                var url = $"sites/{siteId}/extend";
                server
                    .Arrange()
                    .GetSiteCoreApi()
                    .SetupRequest(HttpMethod.Get, url)
                    .ReturnsJson(site);

                var response = await client.PutAsJsonAsync(
                    $"customers/{customerId}/portfolios/{portfolioId}/sites/{siteId}",
                    updateSiteRequest
                );

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var resultJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializerExtensions.Deserialize<ErrorResponse>(resultJson);
                result.Message.Should().Contain("timezone");
            }
        }

        [Fact]
        public async Task SiteExist_UpdateSite_ReturnsNoContent()
        {
            var siteId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var portfolioId = Guid.NewGuid();
            var features = Fixture
                .Build<SiteFeatures>()
                .With(x => x.IsOccupancyEnabled, false)
                .Create();
            var site = Fixture
                .Build<Site>()
                .With(x => x.Id, siteId)
                .With(x => x.CustomerId, customerId)
                .With(x => x.PortfolioId, portfolioId)
                .With(x => x.Features, new SiteFeatures())
                .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                .Create();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                await dbContext.SaveChangesAsync();

                var request = new UpdateSiteRequest
                {
                    Name = "new name",
                    Features = features,
                    TimeZoneId = "Pacific Standard Time",
                    Status = Enums.SiteStatus.Operations
                };

                var url = $"sites/{siteId}/extend";
                server
                    .Arrange()
                    .GetSiteCoreApi()
                    .SetupRequest(HttpMethod.Get, url)
                    .ReturnsJson(site);

                url = $"customers/{customerId}/portfolios/{portfolioId}/sites/{siteId}/extend";
                server
                    .Arrange()
                    .GetSiteCoreApi()
                    .SetupRequest(HttpMethod.Put, url)
                    .ReturnsJson(HttpStatusCode.NoContent);

                var response = await client.PutAsJsonAsync(
                    $"customers/{customerId}/portfolios/{portfolioId}/sites/{siteId}",
                    request
                );
                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }
    }
}
