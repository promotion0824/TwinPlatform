using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Willow.Tests.Infrastructure;
using SiteCore.Tests;
using Xunit;
using Xunit.Abstractions;
using System;
using SiteCore.Dto;
using SiteCore.Entities;
using SiteCore.Requests;
using System.Collections.Generic;
using Willow.Infrastructure;
using Newtonsoft.Json;
using System.Net.Http.Json;

namespace SiteCore.Test.Controllers.Sites
{
    public class UpdateSiteTests : BaseInMemoryTest
    {
        public UpdateSiteTests (ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GivenValidInput_UpdateSite_SiteAndFloorsAreCreated()
        {
            var siteId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var portfolioId = Guid.NewGuid();
            var site = Fixture.Build<SiteEntity>()
                              .With(x => x.Id, siteId)
                              .With(x => x.CustomerId, customerId)
                              .With(x => x.PortfolioId, portfolioId)
                              .Without(x => x.Floors)
                              .Create();
            var updateSiteRequest = Fixture.Build<UpdateSiteRequest>()
                                           .With(x => x.TimeZoneId, "AUS Eastern Standard Time")
                                           .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var dbContext = server.Assert().GetDbContext<SiteDbContext>();
                dbContext.Sites.Add(site);
                await dbContext.SaveChangesAsync();
                var response = await client.PutAsJsonAsync($"customers/{customerId}/portfolios/{portfolioId}/sites/{siteId}", updateSiteRequest);
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<SiteDetailDto>();
                result.CustomerId.Should().Be(customerId);
                result.PortfolioId.Should().Be(portfolioId);
                result.Name.Should().Be(updateSiteRequest.Name);
                result.Address.Should().Be(updateSiteRequest.Address);
                result.TimeZoneId.Should().Be(updateSiteRequest.TimeZoneId);
                result.Latitude.Should().Be(updateSiteRequest.Latitude);
                result.Longitude.Should().Be(updateSiteRequest.Longitude);
                result.Status.Should().Be(updateSiteRequest.Status);
                result.Type.Should().Be(updateSiteRequest.Type);
                result.Area.Should().Be(updateSiteRequest.Area);
                result.State.Should().Be(updateSiteRequest.State);
                dbContext.Sites.Should().HaveCount(1);
            }
        }

        [Fact]
        public async Task TimeZoneIdIsWrong_UpdateSite_ReturnsBadRequest()
        {
            var updateSiteRequest = Fixture.Build<UpdateSiteRequest>()
                                           .With(x => x.TimeZoneId, "wrong timezone id")
                                           .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.PutAsJsonAsync($"customers/{Guid.NewGuid()}/portfolios/{Guid.NewGuid()}/sites/{Guid.NewGuid()}", updateSiteRequest);

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var resultJson = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ErrorResponse>(resultJson);
                result.Message.Should().Contain("timezone");
            }
        }
    }
}