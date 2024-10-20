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
    public class CreateSiteTests : BaseInMemoryTest
    {
        public CreateSiteTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GivenValidInput_CreateSite_SiteAndFloorsAreCreated()
        {
            var customerId = Guid.NewGuid();
            var portfolioId = Guid.NewGuid();
            var createSiteRequest = Fixture.Build<CreateSiteRequest>()
                                           .With(x => x.FloorCodes, () => new List<string>{"Floor1", "Floor2", "Floor3"})
                                           .With(x => x.TimeZoneId, "AUS Eastern Standard Time")
                                           .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.PostAsJsonAsync($"customers/{customerId}/portfolios/{portfolioId}/sites", createSiteRequest);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<SiteDetailDto>();
                result.CustomerId.Should().Be(customerId);
                result.PortfolioId.Should().Be(portfolioId);
                result.Name.Should().Be(createSiteRequest.Name);
                result.Address.Should().Be(createSiteRequest.Address);
                result.TimeZoneId.Should().Be(createSiteRequest.TimeZoneId);
                result.Latitude.Should().Be(createSiteRequest.Latitude);
                result.Longitude.Should().Be(createSiteRequest.Longitude);
                var dbContext = server.Assert().GetDbContext<SiteDbContext>();
                dbContext.Sites.Should().HaveCount(1);
                var siteEntity = dbContext.Sites.First();
                siteEntity.CustomerId.Should().Be(customerId);
                siteEntity.PortfolioId.Should().Be(portfolioId);
                siteEntity.Name.Should().Be(createSiteRequest.Name);
                siteEntity.Address.Should().Be(createSiteRequest.Address);
                siteEntity.Suburb.Should().Be(createSiteRequest.Suburb);
                siteEntity.TimezoneId.Should().Be(createSiteRequest.TimeZoneId);
                siteEntity.Latitude.Should().Be(createSiteRequest.Latitude);
                siteEntity.Longitude.Should().Be(createSiteRequest.Longitude);
                siteEntity.Area.Should().Be(createSiteRequest.Area);
                siteEntity.Status.Should().Be(createSiteRequest.Status);
                siteEntity.Type.Should().Be(createSiteRequest.Type);
                siteEntity.State.Should().Be(createSiteRequest.State);
                siteEntity.CreatedDate.Should().NotBeNull();
                dbContext.Floors.Should().HaveCount(createSiteRequest.FloorCodes.Count);
            }
        }

        [Fact]
        public async Task FloorCodesAreNotGiven_CreateSite_ReturnsBadRequest()
        {
            var createSiteRequest = Fixture.Build<CreateSiteRequest>()
                                           .With(x => x.FloorCodes, (List<string>)null)
                                           .With(x => x.TimeZoneId, "AUS Eastern Standard Time")
                                           .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.PostAsJsonAsync(
                    $"customers/{Guid.NewGuid()}/portfolios/{Guid.NewGuid()}/sites",
                    createSiteRequest);

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var resultJson = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ErrorResponse>(resultJson);
                result.Message.Should().Contain("floor codes");
            }
        }

        [Fact]
        public async Task TimeZoneIdIsWrong_CreateSite_ReturnsBadRequest()
        {
            var createSiteRequest = Fixture.Build<CreateSiteRequest>()
                                           .With(x => x.FloorCodes, new List<string> { "abc", "xyz" })
                                           .With(x => x.TimeZoneId, "wrong timezone id")
                                           .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.PostAsJsonAsync(
                    $"customers/{Guid.NewGuid()}/portfolios/{Guid.NewGuid()}/sites",
                    createSiteRequest);

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var resultJson = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ErrorResponse>(resultJson);
                result.Message.Should().Contain("timezone");
            }
        }

        [Fact]
        public async Task DuplicateFloorCode_CreateSite_ReturnsBadRequest()
        {
            var customerId = Guid.NewGuid();
            var portfolioId = Guid.NewGuid();
            var createSiteRequest = Fixture.Build<CreateSiteRequest>()
                                           .With(x => x.FloorCodes, () => new List<string> { "Floor1", "Floor1", "Floor3" })
                                           .With(x => x.TimeZoneId, "AUS Eastern Standard Time")
                                           .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.PostAsJsonAsync($"customers/{customerId}/portfolios/{portfolioId}/sites", createSiteRequest);

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var result = await response.Content.ReadAsStringAsync();
                result.Should().Contain("Floor codes must be unique.");
            }
        }
    }
}