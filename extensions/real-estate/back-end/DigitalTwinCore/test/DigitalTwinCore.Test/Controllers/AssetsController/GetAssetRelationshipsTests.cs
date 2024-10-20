using DigitalTwinCore.Dto;
using DigitalTwinCore.Entities;
using DigitalTwinCore.Services;
using DigitalTwinCore.Test.MockServices;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
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
    public class GetAssetRelationshipsTests : BaseInMemoryTest
    {
        public GetAssetRelationshipsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact(Skip = "Fix later")]
        public async Task NotUsingBuildingSpecificModels_GetAssetRelationships_ReturnsRelationships()
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

            var response = await client.GetAsync($"sites/{siteId}/assets/{expectedAsset.UniqueId}/relationships");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<AssetRelationshipsDto>();

            result.Relationships.Count.Should().Be(1);
            var relationship = result.Relationships.Single();
            relationship.Relationship.Name.Should().Be(Relationships.LocatedIn);
            relationship.Target.Metadata.ModelId.Should().Be("dtmi:com:willowinc:Level;1");
            relationship.Target.CustomProperties[Properties.UniqueId].ToString().Should().Be(floor1Id.ToString());
            result.IncomingRelationships.Should().BeEmpty();
        }

        [Fact(Skip = "Fix later")]
        public async Task InvalidAssetId_GetAssetRelationships_ReturnsNotFound()
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

            var response = await client.GetAsync($"sites/{siteId}/assets/{Guid.NewGuid()}/relationships");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact(Skip = "Fix later")]
        public async Task AssetOfDifferentSite_GetAssetRelationships_ReturnsNotFound()
{
            var userId = Guid.NewGuid();
            var siteId = Guid.NewGuid();
            var otherSiteId = Guid.NewGuid();

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
            setup.SetupTwins(null, otherSiteId);

            var assetOfDifferentSite = AdtSetupHelper.CreateTwin("AirHandlingUnit", "Test Asset", otherSiteId);
            var twinId = setup.AddTwin(assetOfDifferentSite);
            setup.AddRelationship(twinId, AdtSetupHelper.MakeId(otherSiteId, "Level1"), Relationships.LocatedIn);
            dts.Reload();

            using var client = server.CreateClient(null, userId);

            var response = await client.GetAsync($"sites/{siteId}/assets/{assetOfDifferentSite.UniqueId}/relationships");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }


        [Fact(Skip = "Fix later")]
        public async Task UsingBuildingSpecificModels_GetAssetRelationships_ReturnsRelationships()
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

            var response = await client.GetAsync($"sites/{siteId}/assets/{expectedAsset.UniqueId}/relationships");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<AssetRelationshipsDto>();

            result.Relationships.Count.Should().Be(1);
            var relationship = result.Relationships.Single();
            relationship.Relationship.Name.Should().Be(Relationships.LocatedIn);
            relationship.Target.Metadata.ModelId.Should().Be("dtmi:com:willowinc:Level;1");
            result.IncomingRelationships.Should().BeEmpty();
        }
    }
}
