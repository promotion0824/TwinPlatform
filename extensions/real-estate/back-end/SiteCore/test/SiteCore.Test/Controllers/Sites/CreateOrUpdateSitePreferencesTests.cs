using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Willow.Tests.Infrastructure;
using SiteCore.Tests;
using Xunit;
using Xunit.Abstractions;
using System;
using SiteCore.Entities;
using System.Net.Http.Json;

namespace SiteCore.Test.Controllers.Sites
{
    public class CreateOrUpdateSitePreferencesTests : BaseInMemoryTest
    {
        public CreateOrUpdateSitePreferencesTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GivenEmptyInput_CreateOrUpdateSitePreferences_ReturnsNoContent()
        {
            var siteId = Guid.NewGuid();
            var site = Fixture.Build<SiteEntity>()
                .Without(x => x.Floors)
                .Without(x => x.PortfolioId)
                .With(x => x.Postcode, "111250")
                .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                .With(x => x.Id, siteId)
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<SiteDbContext>();
                db.Sites.Add(site);
                db.SaveChanges();

                var response = await client.PutAsJsonAsync($"sites/{siteId}/preferences", new { });
                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }

        [Fact]
        public async Task GivenProfle_CreateOrUpdateSitePreferences_ReturnsNoContent()
        {
            var siteId = Guid.NewGuid();
            var site = Fixture.Build<SiteEntity>()
                .Without(x => x.Floors)
                .Without(x => x.PortfolioId)
                .With(x => x.Postcode, "111250")
                .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                .With(x => x.Id, siteId)
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<SiteDbContext>();
                db.Sites.Add(site);
                db.SaveChanges();
                var request = new { TimeMachine = new {} };

                var response = await client.PutAsJsonAsync($"sites/{siteId}/preferences", request);
                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }
    }
}