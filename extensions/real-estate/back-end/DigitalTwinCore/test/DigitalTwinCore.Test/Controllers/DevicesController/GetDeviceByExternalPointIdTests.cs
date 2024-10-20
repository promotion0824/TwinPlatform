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
using System.Text.Json;
using System.Threading.Tasks;
using DigitalTwinCore.Constants;
using Willow.Infrastructure;
using Willow.Tests.Infrastructure;
using Workflow.Tests;
using Xunit;
using Xunit.Abstractions;

namespace DigitalTwinCore.Test.Controllers.DevicesController
{
    public class GetDeviceByExternalPointIdTests : BaseInMemoryTest
    {
        public GetDeviceByExternalPointIdTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact(Skip = "Fix later")]
        public async Task PointWithExternalPointIdExists_GetDeviceByExternalPointId_ReturnsThatDevice()
        {
            var userId = Guid.NewGuid();
            var siteId = Guid.NewGuid();
            var floor1Id = Guid.NewGuid();
            var floor2Id = Guid.NewGuid();
            var externalPointId = Guid.NewGuid().ToString();

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
            var expectedDeviceId = setup.AddTwin(expectedDevice);
            setup.AddRelationship(expectedDeviceId, AdtSetupHelper.MakeId(siteId, "Level1"), Relationships.LocatedIn);

            var expectedPoint1 = AdtSetupHelper.CreateTwin("Setpoint", "Point2", siteId);
            expectedPoint1.CustomProperties.Add("externalID", externalPointId);
            twinId = setup.AddTwin(expectedPoint1);
            setup.AddRelationship(twinId, expectedAsset.Id, Relationships.IsCapabilityOf);
            setup.AddRelationship(twinId, expectedDeviceId, Relationships.HostedBy);

            var expectedPoint2 = AdtSetupHelper.CreateTwin("Setpoint", "Point3", siteId);
            twinId = setup.AddTwin(expectedPoint2);
            setup.AddRelationship(twinId, expectedAsset.Id, Relationships.IsCapabilityOf);
            setup.AddRelationship(twinId, expectedDeviceId, Relationships.HostedBy);

            using var client = server.CreateClient(null, userId);

            var response = await client.GetAsync($"sites/{siteId}/devices/externalPointId/{externalPointId}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<DeviceDto>();

            result.Id.Should().Be(expectedDevice.UniqueId);
            result.Points.Count().Should().Be(2);

            result.Points = result.Points.OrderBy(p => p.Name).ToList();

            result.Points.First().Id.Should().Be(expectedPoint1.UniqueId);
            result.Points.First().Assets.Single().Id.Should().Be(expectedAsset.UniqueId);
            ((JsonElement)result.Points.First().Properties["externalID"].Value).GetString().Should().Be(externalPointId);
            result.Points.Last().Id.Should().Be(expectedPoint2.UniqueId);
            result.Points.Last().Assets.Single().Id.Should().Be(expectedAsset.UniqueId);
        }

        [Fact(Skip = "Fix later")]
        public async Task NoPointExists_GetDeviceByExternalPointId_ReturnsNotFound()
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

            using var client = server.CreateClient(null, userId);
            var response = await client.GetAsync($"sites/{siteId}/devices/externalPointId/{Guid.NewGuid()}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            var error = await response.Content.ReadAsErrorResponseAsync();
            error.Message.Should().Contain("Device");
        }

        [Fact(Skip = "Fix later")]
        public async Task PointWithExternalPointExistsForDifferentSite_GetDeviceByExternalPointId_ReturnsNotFound()
        {
            var userId = Guid.NewGuid();
            var siteId = Guid.NewGuid();
            var otherSiteId = Guid.NewGuid();
            var floor1Id = Guid.NewGuid();
            var floor2Id = Guid.NewGuid();
            var externalPointId = Guid.NewGuid().ToString();

            using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);
            var serverArrangement = server.Arrange();
            var context = serverArrangement.CreateDbContext<DigitalTwinDbContext>();
            context.SiteSettings.Add(new SiteSettingEntity { SiteId = siteId, InstanceUri = "https://localhost", SiteCodeForModelId = null });
            context.SaveChanges();

            var dtsp = serverArrangement.MainServices.GetRequiredService<IDigitalTwinServiceProvider>();
            var dts = await dtsp.GetForSiteAsync(siteId) as TestDigitalTwinService;

            var setup = new AdtSetupHelper(dts);
            setup.SetupModels();
            setup.SetupTwins(null, otherSiteId, floor1Id, floor2Id);

            var expectedAsset = AdtSetupHelper.CreateTwin("AirHandlingUnit", "Test Asset", otherSiteId);
            var twinId = setup.AddTwin(expectedAsset);
            setup.AddRelationship(twinId, AdtSetupHelper.MakeId(otherSiteId, "Level1"), Relationships.LocatedIn);

            var expectedDevice = AdtSetupHelper.CreateTwin("Controller", "Test Device", otherSiteId);
            var expectedDeviceId = setup.AddTwin(expectedDevice);
            setup.AddRelationship(expectedDeviceId, AdtSetupHelper.MakeId(otherSiteId, "Level1"), Relationships.LocatedIn);

            var expectedPoint1 = AdtSetupHelper.CreateTwin("Setpoint", "Point2", otherSiteId);
            expectedPoint1.CustomProperties.Add("externalID", externalPointId);
            twinId = setup.AddTwin(expectedPoint1);
            setup.AddRelationship(twinId, expectedAsset.Id, Relationships.IsCapabilityOf);
            setup.AddRelationship(twinId, expectedDeviceId, Relationships.HostedBy);

            using var client = server.CreateClient(null, userId);

            var response = await client.GetAsync($"sites/{siteId}/devices/externalPointId/{externalPointId}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            var error = await response.Content.ReadAsErrorResponseAsync();
            error.Message.Should().Contain("Device");
        }
    }
}
