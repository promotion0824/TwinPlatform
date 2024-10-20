using AutoFixture;
using FluentAssertions;
using Newtonsoft.Json;
using SiteCore.Dto;
using SiteCore.Entities;
using SiteCore.Requests;
using SiteCore.Tests;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Willow.Infrastructure;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace SiteCore.Test.Controllers.Floors
{
    public class CreateFloorTests : BaseInMemoryTest
    {
        public CreateFloorTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task FloorCodesAreNotGiven_CreateFloor_ReturnsBadRequest()
        {
            var createFloorRequest = Fixture.Build<CreateFloorRequest>()
                                           .Without(x => x.Code)
                                           .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.PostAsJsonAsync(
                    $"sites/{Guid.NewGuid()}/floors",
                    createFloorRequest);

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var resultJson = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ErrorResponse>(resultJson);
                result.Message.Should().Contain("Missing floor code");
            }
        }

        [Fact]
        public async Task FloorNameAreNotGiven_CreateFloor_ReturnsBadRequest()
        {
            var createFloorRequest = Fixture.Build<CreateFloorRequest>()
                                           .Without(x => x.Name)
                                           .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.PostAsJsonAsync(
                    $"sites/{Guid.NewGuid()}/floors",
                    createFloorRequest);

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var resultJson = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ErrorResponse>(resultJson);
                result.Message.Should().Contain("Missing floor name");
            }
        }

        [Fact]
        public async Task ModelReferenceIsInvalid_CreateFloor_ReturnsBadRequest()
        {
	        var createFloorRequest = Fixture.Build<CreateFloorRequest>()
		        .With(x => x.ModelReference,"invalid")
		        .Create();

	        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
	        using (var client = server.CreateClient(null))
	        {
		        var response = await client.PostAsJsonAsync(
			        $"sites/{Guid.NewGuid()}/floors",
			        createFloorRequest);

		        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
		        var resultJson = await response.Content.ReadAsStringAsync();
		        var result = JsonConvert.DeserializeObject<ErrorResponse>(resultJson);
		        result.Message.Should().Contain("Model Reference is not valid");
	        }
        }

		[Fact]
        public async Task FloorCodeAlreadyExists_CreateFloor_ReturnsBadRequest()
        {
            var siteId = Guid.NewGuid();
            var createFloorRequest = Fixture.Build<CreateFloorRequest>().With(x => x.ModelReference, "").Create();

            var floor = Fixture.Build<FloorEntity>()
                .Without(x => x.Site)
                .Without(x => x.Modules)
                .Without(x => x.LayerGroups)
                .With(x => x.SiteId, siteId)
                .With(x => x.Code, createFloorRequest.Code)
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<SiteDbContext>();
                db.Floors.Add(floor);
                db.SaveChanges();

                var response = await client.PostAsJsonAsync(
                    $"sites/{siteId}/floors",
                    createFloorRequest);

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var resultJson = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ErrorResponse>(resultJson);
                result.Message.Should().Contain("Floor code already exists");
            }
        }

        [Theory]
        [InlineData(true, null)]
        [InlineData(true, "d7cb5e46-686a-4ff3-94ef-cd9cf1cf030b")]
		[InlineData(true,"")]
        [InlineData(false,null)]
        [InlineData(false, "d7cb5e46-686a-4ff3-94ef-cd9cf1cf030b")]
        [InlineData(false, "")]
		public async Task GivenValidInput_CreateFloor_FloorIsCreated(bool isSiteWide,string modelReference)
        {
            var siteId = Guid.NewGuid();
            var createFloorRequest = Fixture.Build<CreateFloorRequest>()
	            .With(x=>x.ModelReference,modelReference)
	            .With(x => x.IsSiteWide, isSiteWide)
                                            .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.PostAsJsonAsync($"sites/{siteId}/floors", createFloorRequest);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<FloorDetailDto>();
                result.SiteId.Should().Be(siteId);
                result.Name.Should().Be(createFloorRequest.Name);
                result.Code.Should().Be(createFloorRequest.Code);
                result.SortOrder.Should().Be(0);
                result.ModelReference.Should().Be(string.IsNullOrEmpty(modelReference) ? null : Guid.Parse(modelReference));
				result.Geometry.Should().BeEmpty();
                var dbContext = server.Assert().GetDbContext<SiteDbContext>();
                dbContext.Floors.Should().HaveCount(1);
                var floorEntity = dbContext.Floors.First();
                floorEntity.SiteId.Should().Be(siteId);
                floorEntity.Name.Should().Be(createFloorRequest.Name);
                floorEntity.IsSiteWide.Should().Be(isSiteWide);
                floorEntity.ModelReference.Should().Be(string.IsNullOrEmpty(modelReference)?null: Guid.Parse(modelReference));
				floorEntity.Code.Should().Be(createFloorRequest.Code);
                floorEntity.SortOrder.Should().Be(0);
                floorEntity.Geometry.Should().BeEmpty();
            }
        }

       
    }
}
