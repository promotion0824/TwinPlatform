using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using SiteCore.Dto;
using SiteCore.Entities;
using SiteCore.Requests;
using Willow.Tests.Infrastructure;
using SiteCore.Tests;
using Xunit;
using Xunit.Abstractions;
using System.Net.Http.Json;
using System.Linq;
using Newtonsoft.Json;

namespace SiteCore.Test.Controllers.Floors
{
    public class UpdateFloorTests : BaseInMemoryTest
    {
        public UpdateFloorTests(ITestOutputHelper output) : base(output)
        {
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(false, false)]
        [InlineData(true, null)]
        [InlineData(false, null)]
        public async Task SitesHasFloor_UpdateFloors_ReturnsUpdatedFloors(bool isSiteWide, bool? newSiteWide)
        {
            var siteId = Guid.NewGuid();
            var floorId = Guid.NewGuid();

            var site = Fixture.Build<SiteEntity>()
                .Without(x => x.Floors)
                .Without(x => x.PortfolioId)
                .With(x => x.Postcode, "111250")
                .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                .With(x => x.Id, siteId)
                .Create();

            var floor = Fixture.Build<FloorEntity>()
                .Without(x => x.Site)
                .Without(x => x.Modules)
                .Without(x => x.LayerGroups)
                .With(x => x.Code, "Code1")
                .With(x => x.SiteId, siteId)
                .With(x => x.Id, floorId)
                .With(x => x.IsDecomissioned, false)
                .With(x => x.IsSiteWide, isSiteWide)
                .Create();

            var updateRequest = new UpdateFloorRequest { Name = "NewName" + Guid.NewGuid(), ModelReference = "d7cb5e46-686a-4ff3-94ef-cd9cf1cf030b", IsSiteWide = newSiteWide };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<SiteDbContext>();
                db.Sites.Add(site);
                db.Floors.Add(floor);
                db.SaveChanges();

                var response = await client.PutAsJsonAsync($"sites/{siteId}/floors/{floorId}", updateRequest);
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<FloorDetailDto>();
                result.Name.Should().Be(updateRequest.Name);
                result.ModelReference.Should().Be(updateRequest.ModelReference);
                result.IsSiteWide.Should().Be(newSiteWide.HasValue ? newSiteWide.Value : isSiteWide);
            }
        }

        [Fact]
        public async Task SitesHasFloor_UpdateFloorToRemoveModelReference_ReturnsUpdatedFloors()
        {
            var siteId = Guid.NewGuid();
            var floorId = Guid.NewGuid();

            var site = Fixture.Build<SiteEntity>()
                .Without(x => x.Floors)
                .Without(x => x.PortfolioId)
                .With(x => x.Postcode, "111250")
                .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                .With(x => x.Id, siteId)
                .Create();

            var modelReference = Guid.NewGuid();
            var floor = Fixture.Build<FloorEntity>()
                .Without(x => x.Site)
                .Without(x => x.Modules)
                .Without(x => x.LayerGroups)
                .With(x => x.Code, "Code1")
                .With(x => x.SiteId, siteId)
                .With(x => x.Id, floorId)
                .With(x => x.IsDecomissioned, false)
                .With(x => x.ModelReference, modelReference)
                .Create();

            var updateRequest = new UpdateFloorRequest { Name = "NewName" + Guid.NewGuid(), ModelReference ="" };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<SiteDbContext>();
                db.Sites.Add(site);
                db.Floors.Add(floor);
                db.SaveChanges();

                var response = await client.PutAsJsonAsync($"sites/{siteId}/floors/{floorId}", updateRequest);
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<FloorDetailDto>();
                result.Name.Should().Be(updateRequest.Name);
                result.ModelReference.Should().Be(null);
            }
        }
        [Fact]
        public async Task SitesHasFloor_UpdateFloor_ModelReferenceShouldNotChange_ReturnsUpdatedFloors()
        {
	        var siteId = Guid.NewGuid();
	        var floorId = Guid.NewGuid();

	        var site = Fixture.Build<SiteEntity>()
		        .Without(x => x.Floors)
		        .Without(x => x.PortfolioId)
		        .With(x => x.Postcode, "111250")
		        .With(x => x.TimezoneId, "AUS Eastern Standard Time")
		        .With(x => x.Id, siteId)
		        .Create();

	        var modelReference = Guid.NewGuid();
	        var floor = Fixture.Build<FloorEntity>()
		        .Without(x => x.Site)
		        .Without(x => x.Modules)
		        .Without(x => x.LayerGroups)
		        .With(x => x.Code, "Code1")
		        .With(x => x.SiteId, siteId)
		        .With(x => x.Id, floorId)
		        .With(x => x.IsDecomissioned, false)
		        .With(x => x.ModelReference, modelReference)
		        .Create();

	        var updateRequest = new UpdateFloorRequest { Name = "NewName" + Guid.NewGuid(), ModelReference =null };

	        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
	        using (var client = server.CreateClient(null))
	        {
		        var db = server.Arrange().CreateDbContext<SiteDbContext>();
		        db.Sites.Add(site);
		        db.Floors.Add(floor);
		        db.SaveChanges();

		        var response = await client.PutAsJsonAsync($"sites/{siteId}/floors/{floorId}", updateRequest);
		        response.StatusCode.Should().Be(HttpStatusCode.OK);

		        var result = await response.Content.ReadAsAsync<FloorDetailDto>();
		        result.Name.Should().Be(updateRequest.Name);
		        result.ModelReference.Should().Be(modelReference);
	        }
        }
        [Fact]
        public async Task SitesHasFloor_UpdateFloorToUpdateModelReference_ReturnsUpdatedFloors()
        {
	        var siteId = Guid.NewGuid();
	        var floorId = Guid.NewGuid();

	        var site = Fixture.Build<SiteEntity>()
		        .Without(x => x.Floors)
		        .Without(x => x.PortfolioId)
		        .With(x => x.Postcode, "111250")
		        .With(x => x.TimezoneId, "AUS Eastern Standard Time")
		        .With(x => x.Id, siteId)
		        .Create();

	        var modelReference = Guid.NewGuid();
	        var floor = Fixture.Build<FloorEntity>()
		        .Without(x => x.Site)
		        .Without(x => x.Modules)
		        .Without(x => x.LayerGroups)
		        .With(x => x.Code, "Code1")
		        .With(x => x.SiteId, siteId)
		        .With(x => x.Id, floorId)
		        .With(x => x.IsDecomissioned, false)
		        .With(x => x.ModelReference, modelReference)
		        .Create();

	        var updateRequest = new UpdateFloorRequest { Name = "NewName" + Guid.NewGuid(), ModelReference = "d7cb5e46-686a-4ff3-94ef-cd9cf1cf030b" };

	        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
	        using (var client = server.CreateClient(null))
	        {
		        var db = server.Arrange().CreateDbContext<SiteDbContext>();
		        db.Sites.Add(site);
		        db.Floors.Add(floor);
		        db.SaveChanges();

		        var response = await client.PutAsJsonAsync($"sites/{siteId}/floors/{floorId}", updateRequest);
		        response.StatusCode.Should().Be(HttpStatusCode.OK);

		        var result = await response.Content.ReadAsAsync<FloorDetailDto>();
		        result.Name.Should().Be(updateRequest.Name);
		        result.ModelReference.Should().Be(Guid.Parse("d7cb5e46-686a-4ff3-94ef-cd9cf1cf030b"));
	        }
        }

        [Fact]
        public async Task FloorCodeExists_UpdateFloors_ReturnsBadRequest()
        {
            var siteId = Guid.NewGuid();
            var site = Fixture.Build<SiteEntity>()
                .Without(x => x.Floors)
                .Without(x => x.PortfolioId)
                .With(x => x.Postcode, "111250")
                .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                .With(x => x.Id, siteId)
                .Create();

            var siteFloors = Fixture.Build<FloorEntity>()
                .With(x => x.SiteId, siteId)
                .Without(x => x.Site)
                .Without(x => x.Modules)
                .Without(x => x.LayerGroups)
                .CreateMany(3);

            var floorId = siteFloors.First().Id;
            var floorCode = siteFloors.Last().Code;
            var updateRequest = new UpdateFloorRequest { Name = "NewName" + Guid.NewGuid(), Code = floorCode };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<SiteDbContext>();
                db.Sites.Add(site);
                db.Floors.AddRange(siteFloors);
                db.SaveChanges();

                var response = await client.PutAsJsonAsync($"sites/{siteId}/floors/{floorId}", updateRequest);
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var result = await response.Content.ReadAsStringAsync();
                result.Should().Contain("Floor code exists!");
            }
        }

        [Fact]
        public async Task SameFloorCode_UpdateFloors_ReturnsUpdatedFloor()
        {
            var siteId = Guid.NewGuid();
            var floorId = Guid.NewGuid();
            var floorCode = "Code1";
            var site = Fixture.Build<SiteEntity>()
                .Without(x => x.Floors)
                .Without(x => x.PortfolioId)
                .With(x => x.Postcode, "111250")
                .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                .With(x => x.Id, siteId)
                .Create();

            var floor = Fixture.Build<FloorEntity>()
                .Without(x => x.Site)
                .Without(x => x.Modules)
                .Without(x => x.LayerGroups)
                .With(x => x.Code, floorCode)
                .With(x => x.SiteId, siteId)
                .With(x => x.Id, floorId)
                .Create();

            var updateRequest = new UpdateFloorRequest
            {
                Name = "NewName" + Guid.NewGuid(),
                Code = floorCode
            };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<SiteDbContext>();
                db.Sites.Add(site);
                db.Floors.Add(floor);
                db.SaveChanges();

                var response = await client.PutAsJsonAsync($"sites/{siteId}/floors/{floorId}", updateRequest);
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<FloorDetailDto>();
                result.Name.Should().Be(updateRequest.Name);
            }
        }

        [Fact]
        public async Task GivenFloorIdsList_UpdateSortOrder_ReturnsNoContent()
        {
            var siteId = Guid.NewGuid();
            var site = Fixture.Build<SiteEntity>()
                .Without(x => x.Floors)
                .Without(x => x.PortfolioId)
                .With(x => x.Postcode, "111250")
                .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                .With(x => x.Id, siteId)
                .Create();
            var siteFloors = Fixture.Build<FloorEntity>()
                .With(f => f.SiteId, siteId)
                .Without(x => x.Site)
                .Without(x => x.Modules)
                .Without(x => x.LayerGroups)
                .With(x => x.IsDecomissioned, false)
                .CreateMany(3)
                .ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<SiteDbContext>();
                db.Sites.Add(site);
                db.Floors.AddRange(siteFloors);
                db.SaveChanges();

                var request = new Guid[] { siteFloors[2].Id, siteFloors[1].Id, siteFloors[0].Id };

                var response = await client.PutAsJsonAsync($"sites/{siteId}/floors/sortorder", request);
                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }

        [Fact]
        public async Task GivenEmptyFloorIdsList_UpdateSortOrder_ReturnsBadRequest()
        {
            var siteId = Guid.NewGuid();
            var site = Fixture.Build<SiteEntity>()
                .Without(x => x.Floors)
                .Without(x => x.PortfolioId)
                .With(x => x.Postcode, "111250")
                .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                .With(x => x.Id, siteId)
                .Create();
            var siteFloors = Fixture.Build<FloorEntity>()
                .With(f => f.SiteId, siteId)
                .Without(x => x.Site)
                .Without(x => x.Modules)
                .Without(x => x.LayerGroups)
                .CreateMany(3)
                .ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<SiteDbContext>();
                db.Sites.Add(site);
                db.Floors.AddRange(siteFloors);
                db.SaveChanges();

                var response = await client.PutAsJsonAsync($"sites/{siteId}/floors/sortorder", new Guid[] { });
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var result = await response.Content.ReadAsStringAsync();
                result.Should().Contain("floorIds is empty.");
            }
        }

        [Fact]
        public async Task GivenInvalidFloorIds_UpdateSortOrder_ReturnsBadRequest()
        {
            var siteId = Guid.NewGuid();
            var site = Fixture.Build<SiteEntity>()
                .Without(x => x.Floors)
                .Without(x => x.PortfolioId)
                .With(x => x.Postcode, "111250")
                .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                .With(x => x.Id, siteId)
                .Create();
            var siteFloors = Fixture.Build<FloorEntity>()
                .With(f => f.SiteId, siteId)
                .Without(x => x.Site)
                .Without(x => x.Modules)
                .Without(x => x.LayerGroups)
                .With(x => x.IsDecomissioned, false)
                .CreateMany(3)
                .ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<SiteDbContext>();
                db.Sites.Add(site);
                db.Floors.AddRange(siteFloors);
                db.SaveChanges();

                var response = await client.PutAsJsonAsync($"sites/{siteId}/floors/sortorder", new Guid[] { siteFloors[0].Id });
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var result = await response.Content.ReadAsStringAsync();
                result.Should().Contain("floorIds doesn't match floors count.");
            }
        }

        [Fact]
        public async Task ModelReferenceIsInvalid_UpdateFloor_ReturnsBadRequest()
        {
	        var updateFloorRequest = Fixture.Build<UpdateFloorRequest>()
		        .With(x => x.ModelReference, "invalid")
		        .Create();
	        var siteId = Guid.NewGuid();
	        var floorId = Guid.NewGuid();
			using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
			using (var client = server.CreateClient(null))
			{

				var response = await client.PutAsJsonAsync($"sites/{siteId}/floors/{floorId}",updateFloorRequest);

				response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
				response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
				var result = await response.Content.ReadAsStringAsync();
				result.Should().Contain("Model Reference is not valid");
	        }
        }
}
}