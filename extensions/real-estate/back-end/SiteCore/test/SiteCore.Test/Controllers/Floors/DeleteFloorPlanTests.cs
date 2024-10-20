using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq.Contrib.HttpClient;
using SiteCore.Dto;
using SiteCore.Entities;
using SiteCore.Services.ImageHub;
using SiteCore.Tests;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace SiteCore.Test.Controllers.Floors
{
    public class DeleteFloorPlanTests : BaseInMemoryTest
    {
        public DeleteFloorPlanTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task FloorExists_DeletePlanImage_ReturnsUpdatedFloor()
        {
            var siteId = Guid.NewGuid();
            var floorId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var visualId = Guid.NewGuid();
            var moduleId = Guid.NewGuid();
            var moduleTypeId = Guid.NewGuid();

            var site = Fixture.Build<SiteEntity>()
                .Without(x => x.Floors)
                .Without(x => x.PortfolioId)
                .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                .With(x => x.Postcode, "111250")
                .With(x => x.LogoId, (Guid?)null)
                .With(x => x.Id, siteId)
                .With(x => x.CustomerId, customerId)
                .Create();

            var floor = Fixture.Build<FloorEntity>()
                .With(x => x.Id, floorId)
                .Without(x => x.LayerGroups)
                .Without(x => x.Site)
                .Without(x => x.Modules)
                .With(x => x.Code, "Code1")
                .With(x => x.SiteId, siteId)
                .Create();

            var imageType = Fixture.Build<ModuleTypeEntity>()
                .With(x => x.Id, moduleTypeId)
                .With(x => x.CanBeDeleted, true)
                .Without(x => x.Modules)
                .With(x => x.Is3D, false)
                .Create();

            var module = Fixture.Build<ModuleEntity>()
                .With(x => x.Id, moduleId)
                .With(x => x.VisualId, visualId)
                .With(x => x.FloorId, floorId)
                .With(x => x.ModuleTypeId, moduleTypeId)
                .Without(x => x.Floor)
                .Without(x => x.ModuleType)
                .Create();
                

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var arrangement = server.Arrange();
                var db = arrangement.CreateDbContext<SiteDbContext>();
                db.Sites.Add(site);
                db.Floors.Add(floor);
                db.ModuleTypes.Add(imageType);
                db.Modules.Add(module);
                db.SaveChanges();

                var pathHelper = arrangement.MainServices.GetRequiredService<IImagePathHelper>();
                var imagePath = pathHelper.GetFloorModulePath(customerId, siteId, floorId, visualId);
                arrangement.GetImageHubApi()
                    .SetupRequest(HttpMethod.Delete, imagePath)
                    .ReturnsResponse(HttpStatusCode.OK);

                var response = await client.DeleteAsync($"sites/{siteId}/floors/{floorId}/module/{moduleId}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<FloorDetailDto>();

                var expectedDto = FloorDetailDto.MapFrom(FloorEntity.MapToDomainObject(floor));
                result.Should().BeEquivalentTo(expectedDto);

                var dbImage = await db.Modules.FirstOrDefaultAsync(di => di.Id == moduleId);
                dbImage.Should().BeNull();
            }
        }
    }
}
