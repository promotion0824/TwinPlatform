using AutoFixture;
using FluentAssertions;
using SiteCore.Dto;
using SiteCore.Entities;
using SiteCore.Enums;
using SiteCore.Services.ImageHub;
using SiteCore.Tests;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace SiteCore.Test.Controllers.Sites;

public class GetAllSitesTests : BaseInMemoryTest
{
    public GetAllSitesTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task SitesExist_GetAllSitesTests_ReturnsAllSites()
    {
        var sites = Fixture.Build<SiteEntity>()
                           .Without(x => x.Floors)
                           .With(x => x.Postcode, "111250")
                           .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                           .With(x => x.Status, SiteStatus.Operations)
                           .With(x => x.Latitude, 21.23)
                           .With(x => x.Longitude, 121.39)
                           .CreateMany(10)
                           .ToList();

        using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);
        using var client = server.CreateClient(null);

        var db = server.Arrange().CreateDbContext<SiteDbContext>();
        db.Sites.AddRange(sites);
        db.SaveChanges();

        var response = await client.GetAsync($"sites");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadAsAsync<List<SiteSimpleDto>>();
        result.Should().BeEquivalentTo(SiteSimpleDto.MapFrom(SiteEntity.MapToDomainObjects(sites), new ImagePathHelper()));
    }


    [Fact]
    public async Task SitesExist_GetAllSitesTests_ReturnsAllSitesWithoutLocation()
    {
        var sites = Fixture.Build<SiteEntity>()
                           .Without(x => x.Floors)
                           .Without(x => x.Latitude)
                           .Without(x => x.Longitude)
                           .With(x => x.Postcode, "111250")
                           .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                           .With(x => x.Status, SiteStatus.Operations)
                           .CreateMany(10)
                           .ToList();

        using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);
        using var client = server.CreateClient(null);

        var db = server.Arrange().CreateDbContext<SiteDbContext>();
        db.Sites.AddRange(sites);
        db.SaveChanges();

        var response = await client.GetAsync($"sites");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadAsAsync<List<SiteSimpleDto>>();
        result.Should().BeEquivalentTo(SiteSimpleDto.MapFrom(SiteEntity.MapToDomainObjects(sites), new ImagePathHelper()));
    }

	[Fact]
	public async Task HasSitesWithDeletedStatus_GetAllSites_ReturnUndeletedSites()
	{
		var expectedSites = Fixture.Build<SiteEntity>()
									.Without(x => x.Floors)
									.With(x => x.Postcode, "111250")
									.With(x => x.TimezoneId, "AUS Eastern Standard Time")
									.With(x => x.Latitude, 21.23)
									.With(x => x.Longitude, 121.39)
									.CreateMany(10)
									.Where(x => x.Status != SiteStatus.Deleted)
									.ToList();
								 

		var otherSites = Fixture.Build<SiteEntity>()
								 .Without(x => x.Floors)
								 .With(x => x.Postcode, "111250")
								 .With(x => x.TimezoneId, "AUS Eastern Standard Time")
								 .With(x => x.Status, SiteStatus.Deleted)
								 .With(x => x.Latitude, 21.23)
								 .With(x => x.Longitude, 121.39)
						         .CreateMany(2)
								 .ToList();

		using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
		using (var client = server.CreateClient(null))
		{
			var dbContext = server.Arrange().CreateDbContext<SiteDbContext>();
			dbContext.Sites.AddRange(expectedSites);
			dbContext.Sites.AddRange(otherSites);
			await dbContext.SaveChangesAsync();

			var response = await client.GetAsync("sites");
			response.StatusCode.Should().Be(HttpStatusCode.OK);
			var result = await response.Content.ReadAsAsync<List<SiteSimpleDto>>();
			result.Should().BeEquivalentTo(SiteSimpleDto.MapFrom(SiteEntity.MapToDomainObjects(expectedSites), new ImagePathHelper()));
		}



	}
}
