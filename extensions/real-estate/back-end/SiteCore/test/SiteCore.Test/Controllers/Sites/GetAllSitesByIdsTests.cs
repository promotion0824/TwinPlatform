using System;
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
using System.Net.Http.Json;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace SiteCore.Test.Controllers.Sites;

public class GetAllSitesByIdsTests : BaseInMemoryTest
{
    public GetAllSitesByIdsTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task SitesExist_GetAllSitesByIds_ReturnsAllSites()
    {
        var siteIds =new[]{Guid.NewGuid(),Guid.NewGuid()};
        var expectedSite = siteIds.Select(c => Fixture.Build<SiteEntity>()
            .With(x => x.Id, c)
            .Without(x => x.Floors)
            .With(x => x.Postcode, "111250")
            .With(x => x.TimezoneId, "AUS Eastern Standard Time")
            .With(x => x.Status, SiteStatus.Operations)
            .With(x => x.Latitude, 21.23)
            .With(x => x.Longitude, 121.39).Create()).ToList();
        var sites = Fixture.Build<SiteEntity>()
                           .Without(x => x.Floors)
                           .With(x => x.Postcode, "111250")
                           .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                           .With(x => x.Status, SiteStatus.Operations)
                           .With(x => x.Latitude, 21.23)
                           .With(x => x.Longitude, 121.39)
                           .CreateMany(10)
                           .ToList();

        sites.AddRange(expectedSite);
        using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);
        using var client = server.CreateClient(null);

        var db = server.Arrange().CreateDbContext<SiteDbContext>();
        db.Sites.AddRange(sites);
        db.SaveChanges();

        var response = await client.PostAsJsonAsync("sites", siteIds);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadAsAsync<List<SiteSimpleDto>>();
        result.Should().BeEquivalentTo(SiteSimpleDto.MapFrom(SiteEntity.MapToDomainObjects(expectedSite), new ImagePathHelper()));
    }

    [Fact]
    public async Task SitesExist_GetAllSitesByIds_ReturnsAllSitesWithoutLocation()
    {
        var siteIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var expectedSite =new []{  Fixture.Build<SiteEntity>()
            .With(x => x.Id, siteIds[0])
            .Without(x => x.Floors)
            .With(x => x.Postcode, "111250")
            .With(x => x.TimezoneId, "AUS Eastern Standard Time")
            .With(x => x.Status, SiteStatus.Operations)
            .With(x => x.Latitude, 21.23)
            .With(x => x.Longitude, 121.39).Create()};

        var sites = Fixture.Build<SiteEntity>()
            .Without(x => x.Floors)
            .With(x => x.Postcode, "111250")
            .With(x => x.TimezoneId, "AUS Eastern Standard Time")
            .With(x => x.Status, SiteStatus.Operations)
            .With(x => x.Latitude, 21.23)
            .With(x => x.Longitude, 121.39)
            .CreateMany(10)
            .ToList();
        sites.Add(Fixture.Build<SiteEntity>()
            .With(x => x.Id, siteIds[1])
            .Without(x => x.Floors)
            .With(x => x.Postcode, "111250")
            .With(x => x.TimezoneId, "AUS Eastern Standard Time")
            .With(x => x.Status, SiteStatus.Deleted)
            .With(x => x.Latitude, 21.23)
            .With(x => x.Longitude, 121.39).Create());
        sites.AddRange(expectedSite);
        using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);
        using var client = server.CreateClient(null);

        var db = server.Arrange().CreateDbContext<SiteDbContext>();
        db.Sites.AddRange(sites);
        db.SaveChanges();

        var response = await client.PostAsJsonAsync("sites", siteIds);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadAsAsync<List<SiteSimpleDto>>();
        result.Should().BeEquivalentTo(SiteSimpleDto.MapFrom(SiteEntity.MapToDomainObjects(expectedSite), new ImagePathHelper()));
    }

	[Fact]
	public async Task HasSitesWithDeletedStatus_GetAllSitesByIds_ReturnUndeletedSites()
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

    [Fact]
    public async Task SitesNotExist_GetAllSitesWithSiteIds_ReturnsEmpty()
    {
        using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);
        using var client = server.CreateClient(null);

        var response = await client.PostAsJsonAsync("sites", Array.Empty<Guid>());
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadAsAsync<List<SiteSimpleDto>>();
        result.Should().BeEmpty();
    }
}
