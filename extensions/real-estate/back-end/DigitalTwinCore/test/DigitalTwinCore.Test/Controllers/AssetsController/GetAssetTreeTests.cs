using DigitalTwinCore.Dto;
using DigitalTwinCore.Entities;
using DigitalTwinCore.Services;
using DigitalTwinCore.Test.MockServices;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Workflow.Tests;
using Xunit;
using Xunit.Abstractions;

namespace DigitalTwinCore.Test.Controllers.AssetsController
{
    public class GetAssetTreeTests : BaseInMemoryTest
    {
        public GetAssetTreeTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact(Skip = "Fix later")]
        public async Task NotUsingBuildingSpecificModels_GetAssetTreeByDefault_ReturnsAssetOnlyAssetTree()
        {
            var userId = Guid.NewGuid();
            var siteId = Guid.NewGuid();
            var floor1Id = Guid.NewGuid();
            var floor2Id = Guid.NewGuid();

            using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);

            var serverArrangement = server.Arrange();

            var context = serverArrangement.CreateDbContext<DigitalTwinDbContext>();

            context.SiteSettings.Add(new SiteSettingEntity { SiteId = siteId, InstanceUri = "https://localhost", SiteCodeForModelId = null });
            context.SaveChanges();


            var dtsp = serverArrangement.MainServices.GetRequiredService<IDigitalTwinServiceProvider>();
            var dts = await dtsp.GetForSiteAsync(siteId) as TestDigitalTwinService;

            var setup = new AdtSetupHelper(dts);
            setup.SetupModels();
            setup.SetupTwins(null, siteId, floor1Id, floor2Id);
            setup.SetupTwins("BuildingSpecific");
            setup.SetupTwins("OtherBuildingSpecific");


            dts.Reload();

            using var client = server.CreateClient(null, userId);

            var response = await client.GetAsync($"sites/{siteId}/assets/AssetTree");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<List<AssetTreeCategoryDto>>();

            // TODO: May be easier to test results using JsonPath style queries
            result.Count.Should().Be(1);
            var category = result.Single();
            category.Name.Should().Be("Equipment");

            category.Categories.Count.Should().Be(1);
            category = category.Categories.Single();
            category.Name.Should().Be("HVAC Equipment");

            category.Categories.Count.Should().Be(1);
            category = category.Categories.Single();
            category.Name.Should().Be("Air Handling Unit");
            
            category.Assets.Count().Should().Be(2);

            category.Assets = category.Assets.OrderBy(a => a.Name).ToList();

            category.Assets[0].Name.Should().Be("AHU1");
            category.Assets[0].FloorId.Should().Be(floor1Id);
            category.Assets[0].HasLiveData.Should().BeTrue();

            category.Assets[1].Name.Should().Be("AHU2");
            category.Assets[1].FloorId.Should().Be(floor2Id);
            category.Assets[1].HasLiveData.Should().BeFalse();
        }

        [Fact(Skip = "Fix later")]
        public async Task NotUsingBuildingSpecificModels_GetAssetTreeBySpecifiedModels_ReturnsAssetTree()
        {
            var userId = Guid.NewGuid();
            var siteId = Guid.NewGuid();
            var floor1Id = Guid.NewGuid();
            var floor2Id = Guid.NewGuid();

            using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);

            var serverArrangement = server.Arrange();

            var context = serverArrangement.CreateDbContext<DigitalTwinDbContext>();

            context.SiteSettings.Add(new SiteSettingEntity { SiteId = siteId, InstanceUri = "https://localhost", SiteCodeForModelId = null });
            context.SaveChanges();

            var dtsp = serverArrangement.MainServices.GetRequiredService<IDigitalTwinServiceProvider>();
            var dts = await dtsp.GetForSiteAsync(siteId) as TestDigitalTwinService;

            var setup = new AdtSetupHelper(dts);
            setup.SetupModels();
            setup.SetupTwins(null, siteId, floor1Id, floor2Id);
            setup.SetupTwins("BuildingSpecific");
            setup.SetupTwins("OtherBuildingSpecific");

            dts.Reload();

            using var client = server.CreateClient(null, userId);

            var response = await client.GetAsync($"sites/{siteId}/assets/AssetTree?modelNames=Asset&modelNames=Space&modelNames=BuildingComponent");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<List<AssetTreeCategoryDto>>();

            // TODO: May be easier to test results using JsonPath style queries
            result.Count.Should().Be(4);
            var equipmentCategory = result.First(c => c.Name == "Equipment");

            equipmentCategory.Categories.Count.Should().Be(1);
            equipmentCategory = equipmentCategory.Categories.Single();
            equipmentCategory.Name.Should().Be("HVAC Equipment");

            equipmentCategory.Categories.Should().HaveCount(1);
            equipmentCategory = equipmentCategory.Categories.Single();
            equipmentCategory.Name.Should().Be("Air Handling Unit");

            equipmentCategory.Assets.Should().HaveCount(2);

            equipmentCategory.Assets = equipmentCategory.Assets.OrderBy(a => a.Name).ToList();

            equipmentCategory.Assets[0].Name.Should().Be("AHU1");
            equipmentCategory.Assets[0].FloorId.Should().Be(floor1Id);
            equipmentCategory.Assets[0].HasLiveData.Should().BeTrue();

            equipmentCategory.Assets[1].Name.Should().Be("AHU2");
            equipmentCategory.Assets[1].FloorId.Should().Be(floor2Id);
            equipmentCategory.Assets[1].HasLiveData.Should().BeFalse();

            var buildingCategory = result.First(c => c.Name == "Building");
            buildingCategory.Assets.Should().HaveCount(1);
            buildingCategory.Assets[0].Name.Should().Be("Building1");

            var LandCategory = result.First(c => c.Name == "Land");
            LandCategory.Assets.Should().HaveCount(1);
            LandCategory.Assets[0].Name.Should().Be("Site1");

            var LevelCategory = result.First(c => c.Name == "Level");
            LevelCategory.Assets = LevelCategory.Assets.OrderBy(a => a.Name).ToList();
            LevelCategory.Assets.Should().HaveCount(2);
            LevelCategory.Assets[0].Name.Should().Be("Level1");
            equipmentCategory.Assets[0].FloorId.Should().Be(floor1Id);
            LevelCategory.Assets[1].Name.Should().Be("Level2");
            equipmentCategory.Assets[1].FloorId.Should().Be(floor2Id);
        }

#if false // Note supporting site/building-specific models
        [Fact]
        public async Task UsingBuildingSpecificModels_GetAssetTree_ReturnsAssetTree()
        {
            var userId = Guid.NewGuid();
            var siteId = Guid.NewGuid();
            var floor1Id = Guid.NewGuid();
            var floor2Id = Guid.NewGuid();

            using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);

            var serverArrangement = server.Arrange();

            var context = serverArrangement.CreateDbContext<DigitalTwinDbContext>();

            context.SiteSettings.Add(new SiteSettingEntity { SiteId = siteId, InstanceUri = "https://localhost", SiteCodeForModelId = "BuildingSpecific" });
            context.SaveChanges();

            var dtsp = serverArrangement.MainServices.GetRequiredService<IDigitalTwinServiceProvider>();
            var dts = await dtsp.GetForSiteAsync(siteId) as TestDigitalTwinService;

            var setup = new AdtSetupHelper(dts);
            setup.SetupModels();
            setup.SetupTwins("BuildingSpecific", siteId, floor1Id, floor2Id);
            setup.SetupTwins(null);
            setup.SetupTwins("OtherBuildingSpecific");
            dts.Reload();

            using var client = server.CreateClient(null, userId);

            var response = await client.GetAsync($"sites/{siteId}/assets/AssetTree");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<List<AssetTreeCategoryDto>>();

            result.Count.Should().Be(1);
            var category = result.First();
            category.Assets.Should().BeEmpty();
            category.Name.Should().Be("Equipment");
            category.Categories.Count.Should().Be(1);
            category = category.Categories.Single();

            category.Assets.Should().BeEmpty();
            category.Name.Should().Be("HVAC Equipment");
            category.Categories.Count.Should().Be(1);
            category = category.Categories.Single();

            category.Assets.Should().BeEmpty();
            category.Name.Should().Be("Air Handling Unit");
            category.Categories.Count.Should().Be(1);
            category = category.Categories.Single();

            category.Assets.Count().Should().Be(2);
            category.Name.Should().Be("Air Handling Unit (BuildingSpecific)");
            category.Categories.Count.Should().Be(0);


            category.Assets[0].Name.Should().Be("AHU1-BuildingSpecific");
            category.Assets[0].FloorId.Should().Be(floor1Id);
            category.Assets[0].HasLiveData.Should().BeTrue();

            category.Assets[1].Name.Should().Be("AHU2-BuildingSpecific");
            category.Assets[1].FloorId.Should().Be(floor2Id);
            category.Assets[1].HasLiveData.Should().BeFalse();
        }
#endif
    }
}
