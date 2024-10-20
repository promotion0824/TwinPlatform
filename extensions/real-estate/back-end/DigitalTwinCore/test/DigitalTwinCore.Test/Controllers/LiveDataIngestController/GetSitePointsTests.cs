using DigitalTwinCore.Dto;
using DigitalTwinCore.Entities;
using DigitalTwinCore.Models;
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
using System.Text.Json;

namespace DigitalTwinCore.Test.Controllers.LiveDataIngestController
{
    public class GetSitePointsTests : BaseInMemoryTest
    {
        public GetSitePointsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact(Skip = "Fix later")]
        public async Task PointsExist_GetSitePoints_ReturnsPoints()
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

            var expectedPoint1 = AdtSetupHelper.CreateTwin("Setpoint", "TestPoint1", siteId);
            expectedPoint1.CustomProperties["externalID"] = "externalId1";
            twinId = setup.AddTwin(expectedPoint1);
            setup.AddRelationship(twinId, expectedAsset.Id, Relationships.IsCapabilityOf);

            var expectedPoint2 = AdtSetupHelper.CreateTwin("Setpoint", "TestPoint2", siteId);
            expectedPoint2.CustomProperties["externalID"] = "externalId2";
            twinId = setup.AddTwin(expectedPoint2);
            setup.AddRelationship(twinId, expectedAsset.Id, Relationships.IsCapabilityOf);

            dts.Reload();

            using var client = server.CreateClient(null, userId);

            var response = await client.GetAsync($"LiveDataIngest/sites/{siteId}/points?ids={expectedPoint1.UniqueId}&ids={expectedPoint2.UniqueId}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<List<LiveDataIngestPointDto>>();

            result.Count.Should().Be(2);

            result = result.OrderBy(p => p.ExternalId).ToList();

            result.First().AssetId.Should().Be(expectedAsset.UniqueId);
            result.First().UniqueId.Should().Be(expectedPoint1.UniqueId);
            result.First().ExternalId.Should().Be(expectedPoint1.GetStringProperty("externalID"));

            result.Last().AssetId.Should().Be(expectedAsset.UniqueId);
            result.Last().UniqueId.Should().Be(expectedPoint2.UniqueId);
            result.Last().ExternalId.Should().Be(expectedPoint2.GetStringProperty("externalID"));
        }

        [Fact(Skip = "Fix later")]
        public async Task PointWithExternalIdExists_GetSitePoints_ReturnsPoint()
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

            var expectedPoint1 = AdtSetupHelper.CreateTwin("Setpoint", "TestPoint1", siteId);
            expectedPoint1.CustomProperties["externalID"] = "externalId1";
            twinId = setup.AddTwin(expectedPoint1);
            setup.AddRelationship(twinId, expectedAsset.Id, Relationships.IsCapabilityOf);

            var expectedPoint2 = AdtSetupHelper.CreateTwin("Setpoint", "TestPoint2", siteId);
            expectedPoint2.CustomProperties["externalID"] = "externalId2";
            twinId = setup.AddTwin(expectedPoint2);
            setup.AddRelationship(twinId, expectedAsset.Id, Relationships.IsCapabilityOf);

            dts.Reload();

            using var client = server.CreateClient(null, userId);

            var response = await client.GetAsync($"LiveDataIngest/sites/{siteId}/points?externalIds={expectedPoint1.GetStringProperty("externalID")}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<List<LiveDataIngestPointDto>>();

            result.Count.Should().Be(1);

            result.Single().AssetId.Should().Be(expectedAsset.UniqueId);
            result.Single().UniqueId.Should().Be(expectedPoint1.UniqueId);
            result.Single().ExternalId.Should().Be(expectedPoint1.GetStringProperty("externalID"));
        }



        [Fact(Skip = "Fix later")]
        public async Task GetSitePoints_MixedScenarioReturns207()
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

            var expectedPoint1 = AdtSetupHelper.CreateTwin("Setpoint", "TestPoint1", siteId);
            expectedPoint1.CustomProperties["externalID"] = "externalId1";
            twinId = setup.AddTwin(expectedPoint1);
            setup.AddRelationship(twinId, expectedAsset.Id, Relationships.IsCapabilityOf);

            var expectedPoint2 = AdtSetupHelper.CreateTwin("Setpoint", "TestPoint2", siteId);
            expectedPoint2.CustomProperties["externalID"] = "externalId2";
            twinId = setup.AddTwin(expectedPoint2);
            setup.AddRelationship(twinId, expectedAsset.Id, Relationships.IsCapabilityOf);

            var expectedPoint3 = AdtSetupHelper.CreateTwin("Setpoint", "TestPoint3", siteId);
            expectedPoint3.CustomProperties["trendID"] = Guid.NewGuid().ToString();
            twinId = setup.AddTwin(expectedPoint3);
            setup.AddRelationship(twinId, expectedAsset.Id, Relationships.IsCapabilityOf);

            var expectedPoint4 = AdtSetupHelper.CreateTwin("Setpoint", "TestPoint4", siteId);
            twinId = setup.AddTwin(expectedPoint4);
            setup.AddRelationship(twinId, expectedAsset.Id, Relationships.IsCapabilityOf);

            var expectedPoint5 = AdtSetupHelper.CreateTwin("Setpoint", "TestPoint5", siteId);
            twinId = setup.AddTwin(expectedPoint5);
            // Note: not setting associated relationship for this Point

            dts.Reload();

            using var client = server.CreateClient(null, userId);

            var uri = $"LiveDataIngest/sites/{siteId}/points?";
            uri += $"externalIds={expectedPoint1.GetStringProperty("externalID")}";
            uri += $"&ids={expectedPoint2.UniqueId}";
            uri += $"&trendIds={expectedPoint3.GetStringProperty("trendID")}";
            uri += $"&ids={expectedPoint4.UniqueId}";

            uri += $"&ids={expectedPoint5.UniqueId}"; // should not be returned - no relationship to asset
            uri += $"&ids={new Guid()}"; // should not be returned - not found

            var response = await client.GetAsync(uri);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.MultiStatus); // 207 - partial success
            var result = await response.Content.ReadAsAsync<List<LiveDataIngestPointDto>>();

            result.Count.Should().Be(4);

            result.Single(r => r.UniqueId == expectedPoint1.UniqueId);
            result.Single(r => r.UniqueId == expectedPoint2.UniqueId);
            result.Single(r => r.UniqueId == expectedPoint3.UniqueId);
            result.Single(r => r.UniqueId == expectedPoint4.UniqueId);
        }

        [Fact(Skip = "Fix later")]
        public async Task PostSitePoints_MixedScenarioReturns207()
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

            var expectedPoint1 = AdtSetupHelper.CreateTwin("Setpoint", "TestPoint1", siteId);
            expectedPoint1.CustomProperties["externalID"] = "externalId1";
            twinId = setup.AddTwin(expectedPoint1);
            setup.AddRelationship(twinId, expectedAsset.Id, Relationships.IsCapabilityOf);

            var expectedPoint2 = AdtSetupHelper.CreateTwin("Setpoint", "TestPoint2", siteId);
            expectedPoint2.CustomProperties["externalID"] = "externalId2";
            twinId = setup.AddTwin(expectedPoint2);
            setup.AddRelationship(twinId, expectedAsset.Id, Relationships.IsCapabilityOf);

            var expectedPoint3 = AdtSetupHelper.CreateTwin("Setpoint", "TestPoint3", siteId);
            expectedPoint3.CustomProperties["trendID"] = Guid.NewGuid().ToString();
            twinId = setup.AddTwin(expectedPoint3);
            setup.AddRelationship(twinId, expectedAsset.Id, Relationships.IsCapabilityOf);

            var expectedPoint4 = AdtSetupHelper.CreateTwin("Setpoint", "TestPoint4", siteId);
            twinId = setup.AddTwin(expectedPoint4);
            setup.AddRelationship(twinId, expectedAsset.Id, Relationships.IsCapabilityOf);

            var expectedPoint5 = AdtSetupHelper.CreateTwin("Setpoint", "TestPoint5", siteId);
            twinId = setup.AddTwin(expectedPoint5);
            // Note: not setting associated relationship for this Point

            dts.Reload();

            var req = new LiveDataIngestPointsRequest
            {
                IncludePointsWithNoAssets = true,
                Ids = new List<Guid> { 
                    expectedPoint2.UniqueId, expectedPoint4.UniqueId, 
                    expectedPoint5.UniqueId, new Guid()
                },
                ExternalIds = new List<string> { 
                    expectedPoint1.GetStringProperty("externalID") 
                },
                TrendIds = new List<Guid> {
                    new Guid(expectedPoint3.GetStringProperty("trendID"))
                }
            };

            using var client = server.CreateClient(null, userId);

            var uri = $"LiveDataIngest/sites/{siteId}/points?";
            var response = await client.PostAsync(uri, new StringContent(
                JsonSerializer.Serialize(req), System.Text.Encoding.UTF8, "application/json"));

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.MultiStatus); // 207 - partial success
            var result = await response.Content.ReadAsAsync<List<LiveDataIngestPointDto>>();

            result.Count.Should().Be(5);

            result.Single(r => r.UniqueId == expectedPoint1.UniqueId);
            result.Single(r => r.UniqueId == expectedPoint2.UniqueId);
            result.Single(r => r.UniqueId == expectedPoint3.UniqueId);
            result.Single(r => r.UniqueId == expectedPoint4.UniqueId);
            result.Single(r => r.UniqueId == expectedPoint5.UniqueId);
        }

#if false // Building/Site-specific Twins not supported

        [Fact]
        public async Task BuildingSpecificPointsExist_GetSitePoints_ReturnsPoints()
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

            var expectedAsset = AdtSetupHelper.CreateTwinWithSiteCode("BuildingSpecific", "AirHandlingUnit", "Test Asset", siteId);
            var twinId = setup.AddTwin(expectedAsset);
            setup.AddRelationship(twinId, AdtSetupHelper.MakeId(siteId, "Level1"), Relationships.LocatedIn);

            var expectedPoint1 = AdtSetupHelper.CreateTwinWithSiteCode("BuildingSpecific", "Setpoint", "TestPoint1", siteId);
            expectedPoint1.CustomProperties["externalID"] = "externalId1";
            twinId = setup.AddTwin(expectedPoint1);
            setup.AddRelationship(twinId, expectedAsset.Id, Relationships.IsCapabilityOf);

            var expectedPoint2 = AdtSetupHelper.CreateTwinWithSiteCode("BuildingSpecific", "Setpoint", "TestPoint2", siteId);
            expectedPoint2.CustomProperties["externalID"] = "externalId2";
            twinId = setup.AddTwin(expectedPoint2);
            setup.AddRelationship(twinId, expectedAsset.Id, Relationships.IsCapabilityOf);

            dts.Reload();

            using var client = server.CreateClient(null, userId);

            var response = await client.GetAsync($"LiveDataIngest/sites/{siteId}/points?ids={expectedPoint1.UniqueId}&ids={expectedPoint2.UniqueId}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<List<LiveDataIngestPointDto>>();

            result.Count.Should().Be(2);

            result = result.OrderBy(p => p.AssetId).ToList();

            result.First().AssetId.Should().Be(expectedAsset.UniqueId);
            result.First().Id.Should().Be(expectedPoint1.UniqueId);
            result.First().ExternalId.Should().Be(expectedPoint1.GetStringProperty("externalID"));

            result.Last().AssetId.Should().Be(expectedAsset.UniqueId);
            result.Last().Id.Should().Be(expectedPoint2.UniqueId);
            result.Last().ExternalId.Should().Be(expectedPoint2.GetStringProperty("externalID"));
        }
#endif


        [Fact(Skip = "Fix later")]
        public async Task InvalidSiteId_GetSitePoints_ReturnsNotFound()
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

            var response = await client.GetAsync($"LiveDataIngest/sites/{Guid.NewGuid()}/points");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

#if false // Site-specific not supported
        [Fact]
        public async Task PointsExistForDifferentSite_GetSitePoints_ReturnsNoPoints()
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
            setup.SetupTwins(null, siteId);
            setup.SetupTwins(null, otherSiteId);

            var otherAsset = AdtSetupHelper.CreateTwin("AirHandlingUnit", "Test Asset", otherSiteId);
            var twinId = setup.AddTwin(otherAsset);
            setup.AddRelationship(twinId, AdtSetupHelper.MakeId(siteId, "Level1"), Relationships.LocatedIn);

            var otherPoint1 = AdtSetupHelper.CreateTwin("Setpoint", "TestPoint1", otherSiteId);
            otherPoint1.CustomProperties["externalID"] = "externalId1";
            twinId = setup.AddTwin(otherPoint1);
            setup.AddRelationship(twinId, otherAsset.Id, Relationships.IsCapabilityOf);

            var otherPoint2 = AdtSetupHelper.CreateTwin("Setpoint", "TestPoint2", otherSiteId);
            otherPoint1.CustomProperties["externalID"] = "externalId2";
            twinId = setup.AddTwin(otherPoint2);
            setup.AddRelationship(twinId, otherAsset.Id, Relationships.IsCapabilityOf);

            dts.Reload();

            using var client = server.CreateClient(null, userId);

            var response = await client.GetAsync($"LiveDataIngest/sites/{siteId}/points?ids={otherPoint1.UniqueId}&ids={otherPoint2.UniqueId}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<List<LiveDataIngestPointDto>>();
            result.Should().BeEmpty();
        }
#endif
    }
}
