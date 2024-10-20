using DigitalTwinCore.Dto;
using DigitalTwinCore.Entities;
using DigitalTwinCore.Services;
using DigitalTwinCore.Test.MockServices;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
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
    public class GetCountByConnectorTests : BaseInMemoryTest
    {
        public GetCountByConnectorTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact(Skip = "Fix later")]
        public async Task PointsExist_ReturnsCount()
        {
            var userId = Guid.NewGuid();
            var siteId = Guid.NewGuid();
            var connectorId = Guid.NewGuid();
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

            var expectedDevice = AdtSetupHelper.CreateTwin("Controller", "Test Device", siteId);
            expectedDevice.CustomProperties.Add(Properties.ConnectorID, connectorId.ToString());
            twinId = setup.AddTwin(expectedDevice);

            var expectedPoint1 = AdtSetupHelper.CreateTwin("Setpoint", "Point2", siteId);
            expectedPoint1.CustomProperties[Properties.Enabled] = true;
            twinId = setup.AddTwin(expectedPoint1);
            setup.AddRelationship(twinId, expectedAsset.Id, Relationships.IsCapabilityOf);
            setup.AddRelationship(twinId, expectedDevice.Id, Relationships.HostedBy);

            var expectedPoint2 = AdtSetupHelper.CreateTwin("Setpoint", "Point3", siteId);
            expectedPoint2.CustomProperties[Properties.Enabled] = true;
            twinId = setup.AddTwin(expectedPoint2);
            setup.AddRelationship(twinId, expectedAsset.Id, Relationships.IsCapabilityOf);
            setup.AddRelationship(twinId, expectedDevice.Id, Relationships.HostedBy);

            using var client = server.CreateClient(null, userId);

            var response = await client.GetAsync($"sites/{siteId}/connectors/{connectorId}/points/count");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<CountResponse>();

            result.Count.Should().Be(2);
        }

        [Fact(Skip = "Fix later")]
        public async Task BuildingSpecificPointsExist_ReturnsCount()
        {
            var userId = Guid.NewGuid();
            var siteId = Guid.NewGuid();
            var connectorId = Guid.NewGuid();
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

            var expectedDevice = AdtSetupHelper.CreateTwinWithSiteCode("BuildingSpecific", "Controller", "Test Device", siteId);
            expectedDevice.CustomProperties.Add(Properties.ConnectorID, connectorId.ToString());
            twinId = setup.AddTwin(expectedDevice);

            var expectedPoint1 = AdtSetupHelper.CreateTwinWithSiteCode("BuildingSpecific", "Setpoint", "Point2", siteId);
            expectedPoint1.CustomProperties[Properties.Enabled] = true;
            twinId = setup.AddTwin(expectedPoint1);
            setup.AddRelationship(twinId, expectedAsset.Id, Relationships.IsCapabilityOf);
            setup.AddRelationship(twinId, expectedDevice.Id, Relationships.HostedBy);

            var expectedPoint2 = AdtSetupHelper.CreateTwinWithSiteCode("BuildingSpecific", "Setpoint", "Point3", siteId);
            expectedPoint2.CustomProperties[Properties.Enabled] = true;
            twinId = setup.AddTwin(expectedPoint2);
            setup.AddRelationship(twinId, expectedAsset.Id, Relationships.IsCapabilityOf);
            setup.AddRelationship(twinId, expectedDevice.Id, Relationships.HostedBy);

            using var client = server.CreateClient(null, userId);

            var response = await client.GetAsync($"sites/{siteId}/connectors/{connectorId}/points/count");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<CountResponse>();

            result.Count.Should().Be(2);
        }

        [Fact(Skip = "Fix later")]
        public async Task InvalidSiteId_ReturnsNotFound()
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

            var response = await client.GetAsync($"sites/{Guid.NewGuid()}/connectors/{Guid.NewGuid()}/points/count");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact(Skip = "Fix later")]
        public async Task PointsExistForDifferentConnectorId_ReturnsZero()
{
            var userId = Guid.NewGuid();
            var siteId = Guid.NewGuid();
            var connectorId = Guid.NewGuid();
            var otherConnectorId = Guid.NewGuid();
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

            var expectedDevice = AdtSetupHelper.CreateTwin("Controller", "Test Device", siteId);
            expectedDevice.CustomProperties.Add(Properties.ConnectorID, otherConnectorId.ToString());
            twinId = setup.AddTwin(expectedDevice);

            var expectedPoint1 = AdtSetupHelper.CreateTwin("Setpoint", "Point2", siteId);
            expectedPoint1.CustomProperties[Properties.Enabled] = true;
            twinId = setup.AddTwin(expectedPoint1);
            setup.AddRelationship(twinId, expectedAsset.Id, Relationships.IsCapabilityOf);
            setup.AddRelationship(twinId, expectedDevice.Id, Relationships.HostedBy);

            var expectedPoint2 = AdtSetupHelper.CreateTwin("Setpoint", "Point3", siteId);
            expectedPoint2.CustomProperties[Properties.Enabled] = true;
            twinId = setup.AddTwin(expectedPoint2);
            setup.AddRelationship(twinId, expectedAsset.Id, Relationships.IsCapabilityOf);
            setup.AddRelationship(twinId, expectedDevice.Id, Relationships.HostedBy);

            dts.Reload();

            using var client = server.CreateClient(null, userId);

            var response = await client.GetAsync($"sites/{siteId}/connectors/{connectorId}/points/count");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<CountResponse>();
            result.Count.Should().Be(0);
        }
    }
}
