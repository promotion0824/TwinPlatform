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
    public class GetListTests : BaseInMemoryTest
    {
        public GetListTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact(Skip = "Fix later")]
        public async Task PointsExist_GetList_ReturnsPoints()
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

            var expectedPoint1 = AdtSetupHelper.CreateTwin("Setpoint", "Point2", siteId);
            expectedPoint1.CustomProperties[Properties.TrendInterval] = new decimal(900);
            twinId = setup.AddTwin(expectedPoint1);
            setup.AddRelationship(twinId, expectedAsset.Id, Relationships.IsCapabilityOf);

            var expectedPoint2 = AdtSetupHelper.CreateTwin("Setpoint", "Point3", siteId);
            expectedPoint2.CustomProperties[Properties.TrendInterval] = new decimal(600);
            twinId = setup.AddTwin(expectedPoint2);
            setup.AddRelationship(twinId, expectedAsset.Id, Relationships.IsCapabilityOf);

            using var client = server.CreateClient(null, userId);

            var response = await client.GetAsync($"sites/{siteId}/points");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<List<PointDto>>();

            result.Count.Should().Be(3);
            result = result.OrderBy(d => d.Name).ToList();

            result[1].Id.Should().Be(expectedPoint1.UniqueId);
            result[2].Id.Should().Be(expectedPoint2.UniqueId);
            result[1].TrendInterval.Should().Be("900");
            result[2].TrendInterval.Should().Be("600");
            result.All(r => r.Assets == null).Should().BeTrue();
        }

        [Fact(Skip = "Fix later")]
        public async Task PointsExist_GetList_WithIncludeAssets_ReturnsPointsWithAssets()
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

            var expectedPoint1 = AdtSetupHelper.CreateTwin("Setpoint", "Point2", siteId);
            twinId = setup.AddTwin(expectedPoint1);
            setup.AddRelationship(twinId, expectedAsset.Id, Relationships.IsCapabilityOf);

            var expectedPoint2 = AdtSetupHelper.CreateTwin("Setpoint", "Point3", siteId);
            twinId = setup.AddTwin(expectedPoint2);
            setup.AddRelationship(twinId, expectedAsset.Id, Relationships.IsCapabilityOf);

            using var client = server.CreateClient(null, userId);

            var response = await client.GetAsync($"sites/{siteId}/points?includeAssets=true");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<List<PointDto>>();

            result.Count.Should().Be(3);
            result = result.OrderBy(d => d.Name).ToList();

            result[1].Id.Should().Be(expectedPoint1.UniqueId);
            result[2].Id.Should().Be(expectedPoint2.UniqueId);
            result[1].Assets.Single().Id.Should().Be(expectedAsset.UniqueId);
            result[2].Assets.Single().Id.Should().Be(expectedAsset.UniqueId);
        }

#if false // Building/site-specific not supported
        [Fact]
        public async Task BuildingSpecificPointsExist_GetList_ReturnsPoints()
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

            var expectedAsset = AdtSetupHelper.CreateTwin("AirHandlingUnit", "Test Asset", siteId);
            var twinId = setup.AddTwin(expectedAsset);
            setup.AddRelationship(twinId, AdtSetupHelper.MakeId(siteId, "Level1"), Relationships.LocatedIn);

            var expectedPoint1 = AdtSetupHelper.CreateTwinWithSiteCode("BuildingSpecific", "Setpoint", "Point2", siteId);
            twinId = setup.AddTwin(expectedPoint1);
            setup.AddRelationship(twinId, expectedAsset.Id, Relationships.IsCapabilityOf);

            var expectedPoint2 = AdtSetupHelper.CreateTwinWithSiteCode("BuildingSpecific", "Setpoint", "Point3", siteId);
            twinId = setup.AddTwin(expectedPoint2);
            setup.AddRelationship(twinId, expectedAsset.Id, Relationships.IsCapabilityOf);

            using var client = server.CreateClient(null, userId);

            var response = await client.GetAsync($"sites/{siteId}/points");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<List<PointDto>>();

            result.Count.Should().Be(3);

            result[1].ModelId.Should().Be("dtmi:com:willowinc:BuildingSpecific:Setpoint;1");
            result[1].Id.Should().Be(expectedPoint1.UniqueId);
            result[2].ModelId.Should().Be("dtmi:com:willowinc:BuildingSpecific:Setpoint;1");
            result[2].Id.Should().Be(expectedPoint2.UniqueId);
        }
#endif

        [Fact(Skip = "Fix later")]
        public async Task InvalidSiteId_GetList_ReturnsNotFound()
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

            var response = await client.GetAsync($"sites/{Guid.NewGuid()}/points");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact(Skip = "Fix later")]
        public async Task PointsExistForDifferentSite_GetList_ReturnsOnlyPointsForSite()
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

            var pointOfDifferentSite1 = AdtSetupHelper.CreateTwin("Setpoint", "PointWithTag1", otherSiteId);
            twinId = setup.AddTwin(pointOfDifferentSite1);
            setup.AddRelationship(twinId, assetOfDifferentSite.Id, Relationships.IsCapabilityOf);

            var pointOfDifferentSite2 = AdtSetupHelper.CreateTwin("Setpoint", "PointWithTag2", otherSiteId);
            twinId = setup.AddTwin(pointOfDifferentSite2);
            setup.AddRelationship(twinId, assetOfDifferentSite.Id, Relationships.IsCapabilityOf);

            dts.Reload();

            using var client = server.CreateClient(null, userId);

            var response = await client.GetAsync($"sites/{siteId}/points");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<List<PointDto>>();
            result.Count().Should().Be(1);
        }
    }
}
