using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SiteCore.Dto;
using SiteCore.Entities;
using SiteCore.Enums;
using SiteCore.Services.ImageHub;
using SiteCore.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace SiteCore.Test.Controllers.Sites
{
    public class GetSitesTests : BaseInMemoryTest
    {
        public GetSitesTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task CustomerHasSites_GetCustomerSites_ReturnsThoseSites()
        {
            var customerId = Guid.NewGuid();
            var sites = Fixture.Build<SiteEntity>()
                               .Without(x => x.Floors)
                               .Without(x => x.PortfolioId)
                               .With(x => x.CustomerId, customerId)
                               .With(x => x.Postcode, "111250")
                               .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                               .With(x => x.Status, SiteStatus.Operations)
                               .CreateMany(5)
                               .ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<SiteDbContext>();
                db.Sites.AddRange(sites);
                db.SaveChanges();

                var response = await client.GetAsync($"customers/{customerId}/sites");
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<List<SiteSimpleDto>>();
                result.Should().BeEquivalentTo(SiteSimpleDto.MapFrom(SiteEntity.MapToDomainObjects(sites), new ImagePathHelper()));
            }
        }

		[Fact]
		public async Task CustomerHasSitesWithDeletedStatus_GetCustomerSites_ReturnUndeltedSites()
		{
			var customerId = Guid.NewGuid();
			var expbectedSites = Fixture.Build<SiteEntity>()
										.Without(x => x.Floors)
										.Without(x => x.PortfolioId)
										.With(x => x.CustomerId, customerId)
										.With(x => x.Postcode, "111250")
										.With(x => x.TimezoneId, "AUS Eastern Standard Time")
										.CreateMany(5)
										.Where(x => x.Status != SiteStatus.Deleted)
										.ToList();

			var otherSites = Fixture.Build<SiteEntity>()
										.Without(x => x.Floors)
										.Without(x => x.PortfolioId)
										.With(x => x.CustomerId, customerId)
										.With(x => x.Postcode, "111250")
										.With(x => x.TimezoneId, "AUS Eastern Standard Time")
										.With(x => x.Status, SiteStatus.Deleted)
										.CreateMany(2)
										.ToList();

			using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
			using (var client = server.CreateClient(null))
			{
				var db = server.Arrange().CreateDbContext<SiteDbContext>();
				db.Sites.AddRange(expbectedSites);
				db.Sites.AddRange(otherSites);
				db.SaveChanges();

				var response = await client.GetAsync($"customers/{customerId}/sites");
				response.StatusCode.Should().Be(HttpStatusCode.OK);

				var result = await response.Content.ReadAsAsync<List<SiteSimpleDto>>();
				result.Should().BeEquivalentTo(SiteSimpleDto.MapFrom(SiteEntity.MapToDomainObjects(expbectedSites), new ImagePathHelper()));
			}
		}

		// TODO : if there is a use case for getting a single site, then add a test here. Else delete the web method GET /sites/{siteId}
	}
}