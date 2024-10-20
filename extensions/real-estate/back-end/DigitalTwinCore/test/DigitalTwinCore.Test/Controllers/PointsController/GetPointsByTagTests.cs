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
using DigitalTwinCore.Constants;
using Willow.Tests.Infrastructure;
using Workflow.Tests;
using Xunit;
using Xunit.Abstractions;

namespace DigitalTwinCore.Test.Controllers.PointsController
{
    public class GetPointsByTagTests : BaseInMemoryTest
    {
        public GetPointsByTagTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact(Skip = "Fix later")]
        public async Task PointsExist_GetPointsByTag_ReturnsPoints()
        {
            var userId = Guid.NewGuid();
            var siteId = Guid.NewGuid();
            var floor1Id = Guid.NewGuid();
            var floor2Id = Guid.NewGuid();
            var tagName = Properties.TagName;

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

            var expectedTags = new Dictionary<string, object>
            {
                [tagName] = true
            };

            var expectedPoint1 = AdtSetupHelper.CreateTwin("Setpoint", "PointWithTag1", siteId);
            expectedPoint1.CustomProperties.Add(Properties.Tags, expectedTags);
            twinId = setup.AddTwin(expectedPoint1);
            setup.AddRelationship(twinId, expectedAsset.Id, Relationships.IsCapabilityOf);

            var expectedPoint2 = AdtSetupHelper.CreateTwin("Setpoint", "PointWithTag2", siteId);
            expectedPoint2.CustomProperties.Add(Properties.Tags, expectedTags);
            twinId = setup.AddTwin(expectedPoint2);
            setup.AddRelationship(twinId, expectedAsset.Id, Relationships.IsCapabilityOf);

            var otherPoint = AdtSetupHelper.CreateTwin("Setpoint", "PointWithoutTag", siteId);
            twinId = twinId = setup.AddTwin(otherPoint);
            setup.AddRelationship(twinId, expectedAsset.Id, Relationships.IsCapabilityOf);
            dts.Reload();

            using var client = server.CreateClient(null, userId);

            var response = await client.GetAsync($"sites/{siteId}/points/ByTag/{tagName.ToLowerInvariant()}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<List<PointDto>>();
            result = result.OrderBy(p => p.Name).ToList();

            result.Count.Should().Be(2);

            result.First().Id.Should().Be(expectedPoint1.UniqueId);
            result.Last().Id.Should().Be(expectedPoint2.UniqueId);
            result.All(r => r.Tags.Select(t => t.Name).Contains(tagName)).Should().BeTrue();
        }

#if false // Building-specific not supported

        [Fact]
        public async Task BuildingSpecificPointsExist_GetPointsByTag_ReturnsPoints()
        {
            var userId = Guid.NewGuid();
            var siteId = Guid.NewGuid();
            var floor1Id = Guid.NewGuid();
            var floor2Id = Guid.NewGuid();
            var tagName = Properties.TagName;

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

            var expectedAsset = AdtSetupHelper.CreateTwin("AirHandlingUnit", "Test Asset", siteId);
            var twinId = setup.AddTwin(expectedAsset);
            setup.AddRelationship(twinId, AdtSetupHelper.MakeId(siteId, "Level1"), Relationships.LocatedIn);

            var expectedTags = new Dictionary<string, bool>
            {
                [tagName] = true
            };

            var expectedPoint1 = AdtSetupHelper.CreateTwinWithSiteCode("BuildingSpecific", "Setpoint", "PointWithTag1", siteId);
            expectedPoint1.CustomProperties.Add(Properties.Tags, expectedTags);
            twinId = setup.AddTwin(expectedPoint1);
            setup.AddRelationship(twinId, expectedAsset.Id, Relationships.IsCapabilityOf);

            var expectedPoint2 = AdtSetupHelper.CreateTwinWithSiteCode("BuildingSpecific", "Setpoint", "PointWithTag2", siteId);
            expectedPoint2.CustomProperties.Add(Properties.Tags, expectedTags);
            twinId = setup.AddTwin(expectedPoint2);
            setup.AddRelationship(twinId, expectedAsset.Id, Relationships.IsCapabilityOf);

            var otherPoint = AdtSetupHelper.CreateTwinWithSiteCode("BuildingSpecific", "Setpoint", "PointWithoutTag", siteId);
            twinId = twinId = setup.AddTwin(otherPoint);
            setup.AddRelationship(twinId, expectedAsset.Id, Relationships.IsCapabilityOf);
            dts.Reload();

            using var client = server.CreateClient(null, userId);

            var response = await client.GetAsync($"sites/{siteId}/points/ByTag/{tagName.ToLowerInvariant()}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<List<PointDto>>();

            result.Count.Should().Be(2);

            result.First().ModelId.Should().Be("dtmi:com:willowinc:BuildingSpecific:Setpoint;1");
            result.First().Id.Should().Be(expectedPoint1.UniqueId);
            result.Last().Id.Should().Be(expectedPoint2.UniqueId);
            result.All(r => r.Tags.Select(t => t.Name).Contains(tagName)).Should().BeTrue();
        }
#endif

        [Fact(Skip = "Fix later")]
        public async Task InvalidSiteId_GetPointsByTag_ReturnsNotFound()
        {
            var userId = Guid.NewGuid();
            var siteId = Guid.NewGuid();
            var tagName = Properties.TagName;

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

            var response = await client.GetAsync($"sites/{Guid.NewGuid()}/points/ByTag/{tagName.ToLowerInvariant()}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact(Skip = "Fix later")]
        public async Task PointsExistForDifferentSite_GetPointsByTag_ReturnsNoPoints()
{
            var userId = Guid.NewGuid();
            var siteId = Guid.NewGuid();
            var otherSiteId = Guid.NewGuid();
            var tagName = Properties.TagName;

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

            var expectedTags = new Dictionary<string, object>
            {
                [tagName] = true
            };

            var pointOfDifferentSite1 = AdtSetupHelper.CreateTwin("Setpoint", "PointWithTag1", otherSiteId);
            pointOfDifferentSite1.CustomProperties.Add(Properties.Tags, expectedTags);
            twinId = setup.AddTwin(pointOfDifferentSite1);
            setup.AddRelationship(twinId, assetOfDifferentSite.Id, Relationships.IsCapabilityOf);

            var pointOfDifferentSite2 = AdtSetupHelper.CreateTwin("Setpoint", "PointWithTag2", otherSiteId);
            pointOfDifferentSite2.CustomProperties.Add(Properties.Tags, expectedTags);
            twinId = setup.AddTwin(pointOfDifferentSite2);
            setup.AddRelationship(twinId, assetOfDifferentSite.Id, Relationships.IsCapabilityOf);

            var otherPointOfDifferentSite = AdtSetupHelper.CreateTwin("Setpoint", "PointWithoutTag", otherSiteId);
            twinId = setup.AddTwin(otherPointOfDifferentSite);
            setup.AddRelationship(twinId, assetOfDifferentSite.Id, Relationships.IsCapabilityOf);

            dts.Reload();

            using var client = server.CreateClient(null, userId);

            var response = await client.GetAsync($"sites/{siteId}/points/ByTag/{tagName.ToLowerInvariant()}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<List<PointDto>>();
            result.Should().BeEmpty();
        }
    }
}
