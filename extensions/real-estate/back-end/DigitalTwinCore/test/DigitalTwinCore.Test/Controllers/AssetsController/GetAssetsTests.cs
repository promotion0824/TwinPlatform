using DigitalTwinCore.Dto;
using DigitalTwinCore.Entities;
using DigitalTwinCore.Services;
using DigitalTwinCore.Test.MockServices;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DigitalTwinCore.Constants;
using Willow.Tests.Infrastructure;
using Workflow.Tests;
using Xunit;
using Xunit.Abstractions;

namespace DigitalTwinCore.Test.Controllers.AssetsController
{
    public class GetAssetsTests : BaseInMemoryTest
    {
        // This is the internal category Id for a RooftopUnit, currently generated
        //   at run-time from a hash of the full model Id.
        // This categoryId is used "above" this layer in the platform to refer to the model.
        // TODO: Investigate removing unique Ids in lieu of just using qualified model names throughout the system
        private static readonly Guid  RtuCategoryId = new Guid("8ab9f9e5-154d-7f7e-d735-d370ae746f73");

        public GetAssetsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact(Skip = "Fix later")]
        public async Task NotUsingBuildingSpecificModels_GetAssets_ReturnsAssets()
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

            var expectedAsset = AdtSetupHelper.CreateTwin("AirHandlingUnit", "Test Asset", siteId);
            var twinId = setup.AddTwin(expectedAsset);
            setup.AddRelationship(twinId, AdtSetupHelper.MakeId(siteId, "Level1"), Relationships.LocatedIn);
            dts.Reload();

            using var client = server.CreateClient(null, userId);

            var response = await client.GetAsync($"sites/{siteId}/assets");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<List<AssetDto>>();


            result.Count.Should().Be(3);
            result.SingleOrDefault(i => i.Id == expectedAsset.UniqueId).Should().NotBeNull();
            var ahu = result.Single(i => i.Name == "AHU1");
            ahu.Points.Count().Should().Be(1);
            ahu.HasLiveData.Should().BeTrue();
        }

        [Fact(Skip = "Fix later")]
        public async Task WithLiveDataOnly_GetAssets_ReturnsLiveDataAssetsOnly()
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

            var expectedAsset = AdtSetupHelper.CreateTwin("AirHandlingUnit", "Test Asset", siteId);
            var twinId = setup.AddTwin(expectedAsset);
            setup.AddRelationship(twinId, AdtSetupHelper.MakeId(siteId, "Level1"), Relationships.LocatedIn);
            dts.Reload();

            using var client = server.CreateClient(null, userId);

            var response = await client.GetAsync($"sites/{siteId}/assets?liveDataOnly=true");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<List<AssetDto>>();

            result.Count.Should().Be(1);
            result.Single().Points.Count().Should().Be(1);
            result.Single().HasLiveData.Should().BeTrue();
        }

        [Fact(Skip = "Fix later")]
        public async Task WithFloorId_GetAssets_ReturnsAssetsOfThatFloor()
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

            var expectedAsset = AdtSetupHelper.CreateTwin("AirHandlingUnit", "Test Asset", siteId);
            var twinId = setup.AddTwin(expectedAsset);
            setup.AddRelationship(twinId, AdtSetupHelper.MakeId(siteId, "Level1"), Relationships.LocatedIn);
            dts.Reload();

            using var client = server.CreateClient(null, userId);

            var response = await client.GetAsync($"sites/{siteId}/assets?floorId={floor1Id}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<List<AssetDto>>();
            result = result.OrderBy(a => a.Name).ToList();

            result.Count.Should().Be(2);
            result.Where(i => i.FloorId == floor1Id).Count().Should().Be(result.Count);
            result.Single(i => i.Id == expectedAsset.UniqueId).Should().NotBeNull();
            result.First().Points.Count().Should().Be(1);
            result.First().HasLiveData.Should().BeTrue();
        }

        [Fact(Skip = "Fix later")]
        public async Task WithCategoryId_GetAssets_ReturnsAssetsOfThatCategory()
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

            var expectedAsset = AdtSetupHelper.CreateTwin("RooftopUnit", "Test Asset", siteId);
            var twinId = setup.AddTwin(expectedAsset);
            setup.AddRelationship(twinId, AdtSetupHelper.MakeId(siteId, "Level1"), Relationships.LocatedIn);

            var expectedAsset2 = AdtSetupHelper.CreateTwin("RooftopUnit", "Test Asset 2", siteId);
            twinId = setup.AddTwin(expectedAsset2);
            setup.AddRelationship(twinId, AdtSetupHelper.MakeId(siteId, "Level2"), Relationships.LocatedIn);
            dts.Reload();

            using var client = server.CreateClient(null, userId);

            var response = await client.GetAsync($"sites/{siteId}/assets?categoryId={RtuCategoryId}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<List<AssetDto>>();
            result = result.OrderBy(a => a.Name).ToList();


            result.Count.Should().Be(2);
            result.Should().Contain(a => a.Id == expectedAsset.UniqueId );
            result.Should().Contain(a => a.Id == expectedAsset2.UniqueId );

            result.First().Id.Should().Be(expectedAsset.UniqueId);
            result.First().FloorId.Should().Be(floor1Id);
            result.Last().Id.Should().Be(expectedAsset2.UniqueId);
            result.Last().FloorId.Should().Be(floor2Id);
        }

        [Fact(Skip = "Fix later")]
        public async Task WithCategoryIdAndFloorId_GetAssets_ReturnsAssetsOfThatCategoryAndFloor()
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

            var expectedAsset = AdtSetupHelper.CreateTwin("RooftopUnit", "Test Asset", siteId);
            var twinId = setup.AddTwin(expectedAsset);
            setup.AddRelationship(twinId, AdtSetupHelper.MakeId(siteId, "Level1"), Relationships.LocatedIn);

            var otherAsset = AdtSetupHelper.CreateTwin("RooftopUnit", "Test Asset 2", siteId);
            twinId = setup.AddTwin(otherAsset);
            setup.AddRelationship(twinId, AdtSetupHelper.MakeId(siteId, "Level2"), Relationships.LocatedIn);
            dts.Reload();

            using var client = server.CreateClient(null, userId);

            var response = await client.GetAsync($"sites/{siteId}/assets?floorId={floor1Id}&categoryId={RtuCategoryId}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<List<AssetDto>>();

            result.Count.Should().Be(1);
            result.Single().Id.Should().Be(expectedAsset.UniqueId);
        }


        [Fact(Skip = "Fix later")]
        public async Task WithSearchKeyword_GetAssets_ReturnsAssetsMatchingKeywoed()
        {
            var userId = Guid.NewGuid();
            var siteId = Guid.NewGuid();
            var floor1Id = Guid.NewGuid();
            var floor2Id = Guid.NewGuid();
            var categoryId = new Guid("6473ef7a-4d06-b813-3a51-0ce772edd774");

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

            var expectedIds = new List<Guid>();

            var expectedAsset = AdtSetupHelper.CreateTwin("RooftopUnit", "SearchTerm", siteId);
            var twinId = setup.AddTwin(expectedAsset);
            expectedIds.Add(expectedAsset.UniqueId);
            setup.AddRelationship(twinId, AdtSetupHelper.MakeId(siteId, "Level1"), Relationships.LocatedIn);

            var otherAsset = AdtSetupHelper.CreateTwin("RooftopUnit", "SearchTerm2", siteId);
            twinId = setup.AddTwin(otherAsset);
            expectedIds.Add(otherAsset.UniqueId);
            setup.AddRelationship(twinId, AdtSetupHelper.MakeId(siteId, "Level2"), Relationships.LocatedIn);

            otherAsset = AdtSetupHelper.CreateTwin("RooftopUnit", "NotMatching", siteId);
            twinId = setup.AddTwin(otherAsset);
            setup.AddRelationship(twinId, AdtSetupHelper.MakeId(siteId, "Level1"), Relationships.LocatedIn);

            otherAsset = AdtSetupHelper.CreateTwin("RooftopUnit", "inSearchTerm", siteId);
            twinId = setup.AddTwin(otherAsset);
            setup.AddRelationship(twinId, AdtSetupHelper.MakeId(siteId, "Level1"), Relationships.LocatedIn);
            expectedIds.Add(otherAsset.UniqueId);
            dts.Reload();

            using var client = server.CreateClient(null, userId);

            var response = await client.GetAsync($"sites/{siteId}/assets?searchKeyword=SearchTerm");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<List<AssetDto>>();

            result.Count.Should().Be(3);
            result.Select(r => r.Id).Should().BeEquivalentTo(expectedIds);
        }



        [Fact(Skip = "Fix later")]
        public async Task InvalidSiteId_GetAssets_ReturnsNotFound()
        {
            var userId = Guid.NewGuid();
            var siteId = Guid.NewGuid();

            using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);

            var serverArrangement = server.Arrange();

            var context = serverArrangement.CreateDbContext<DigitalTwinDbContext>();

            context.SiteSettings.Add(new SiteSettingEntity { SiteId = siteId, InstanceUri = "https://localhost", SiteCodeForModelId = null });
            context.SaveChanges();

            var dtsp = serverArrangement.MainServices.GetRequiredService<IDigitalTwinServiceProvider>();
            var dts = await dtsp.GetForSiteAsync(siteId) as TestDigitalTwinService;

            var setup = new AdtSetupHelper(dts);
            setup.SetupModels();
            setup.SetupTwins(null, siteId, Guid.NewGuid(), Guid.NewGuid());
            dts.Reload();

            using var client = server.CreateClient(null, userId);

            var response = await client.GetAsync($"sites/{Guid.NewGuid()}/assets");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact(Skip = "Fix later")]
        public async Task UsingBuildingSpecificModels_GetAssets_ReturnsAssets()
        {
            var userId = Guid.NewGuid();
            var siteId = Guid.NewGuid();

            using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);

            var serverArrangement = server.Arrange();

            var context = serverArrangement.CreateDbContext<DigitalTwinDbContext>();

            context.SiteSettings.Add(new SiteSettingEntity { SiteId = siteId, InstanceUri = "https://localhost", SiteCodeForModelId = "BuildingSpecific" });
            context.SaveChanges();

            var dtsp = serverArrangement.MainServices.GetRequiredService<IDigitalTwinServiceProvider>();
            var dts = await dtsp.GetForSiteAsync(siteId) as TestDigitalTwinService;

            var setup = new AdtSetupHelper(dts);
            setup.SetupModels();
            setup.SetupTwins(null);
            setup.SetupTwins("BuildingSpecific", siteId);

            var expectedAsset = AdtSetupHelper.CreateTwinWithSiteCode("BuildingSpecific", "AirHandlingUnit", "Test Asset", siteId);
            var twinId = setup.AddTwin(expectedAsset);
            setup.AddRelationship(twinId, AdtSetupHelper.MakeId(siteId, "Level1"), Relationships.LocatedIn);
            dts.Reload();

            using var client = server.CreateClient(null, userId);

            var response = await client.GetAsync($"sites/{siteId}/assets");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<List<AssetDto>>();

            result.Count.Should().Be(3);
            result.Single(i => i.Id == expectedAsset.UniqueId).Should().NotBeNull();
        }
    }
}
