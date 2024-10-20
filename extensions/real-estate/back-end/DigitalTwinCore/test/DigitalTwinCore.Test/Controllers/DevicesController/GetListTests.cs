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

namespace DigitalTwinCore.Test.Controllers.DevicesController
{
    public class GetListTests : BaseInMemoryTest
    {
        public GetListTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact(Skip = "Fix later")]
        public async Task DeviceExists_GetList_ReturnsDevice()
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

            var expectedDevice = AdtSetupHelper.CreateTwin("Controller", "Test Device", siteId);
            var expectedDeviceId = setup.AddTwin(expectedDevice);
            setup.AddRelationship(expectedDeviceId, AdtSetupHelper.MakeId(siteId, "Level1"), Relationships.LocatedIn);

            var expectedPoint1 = AdtSetupHelper.CreateTwin("Setpoint", "Point2", siteId);
            twinId = setup.AddTwin(expectedPoint1);
            setup.AddRelationship(twinId, expectedAsset.Id, Relationships.IsCapabilityOf);
            setup.AddRelationship(twinId, expectedDeviceId, Relationships.HostedBy);

            var expectedPoint2 = AdtSetupHelper.CreateTwin("Setpoint", "Point3", siteId);
            twinId = setup.AddTwin(expectedPoint2);
            setup.AddRelationship(twinId, expectedAsset.Id, Relationships.IsCapabilityOf);
            setup.AddRelationship(twinId, expectedDeviceId, Relationships.HostedBy);

            using var client = server.CreateClient(null, userId);

            var response = await client.GetAsync($"sites/{siteId}/devices");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<List<DeviceDto>>();

            result.Count.Should().Be(1);

            result.Single().Id.Should().Be(expectedDevice.UniqueId);
            result.Single().Points.Should().BeEmpty();
        }

        [Fact(Skip = "Fix later")]
        public async Task DeviceExists_GetList_WithIncludePoints_ReturnsDeviceWithPoints()
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

            var expectedDevice = AdtSetupHelper.CreateTwin("Controller", "Test Device", siteId);
            var expectedDeviceId = setup.AddTwin(expectedDevice);
            setup.AddRelationship(expectedDeviceId, AdtSetupHelper.MakeId(siteId, "Level1"), Relationships.LocatedIn);

            var expectedPoint1 = AdtSetupHelper.CreateTwin("Setpoint", "Point2", siteId);
            twinId = setup.AddTwin(expectedPoint1);
            setup.AddRelationship(twinId, expectedAsset.Id, Relationships.IsCapabilityOf);
            setup.AddRelationship(twinId, expectedDeviceId, Relationships.HostedBy);

            var expectedPoint2 = AdtSetupHelper.CreateTwin("Setpoint", "Point3", siteId);
            twinId = setup.AddTwin(expectedPoint2);
            setup.AddRelationship(twinId, expectedAsset.Id, Relationships.IsCapabilityOf);
            setup.AddRelationship(twinId, expectedDeviceId, Relationships.HostedBy);

            using var client = server.CreateClient(null, userId);

            var response = await client.GetAsync($"sites/{siteId}/devices?includePoints=true");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<List<DeviceDto>>();

            result.Count.Should().Be(1);

            result.Single().Id.Should().Be(expectedDevice.UniqueId);
            result.Single().Points.Count().Should().Be(2);
            result.Single().Points.First().Assets.Single().Id.Should().Be(expectedAsset.UniqueId);
            result.Single().Points.Last().Assets.Single().Id.Should().Be(expectedAsset.UniqueId);
        }

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

            var response = await client.GetAsync($"sites/{Guid.NewGuid()}/devices");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact(Skip = "Fix later")]
        public async Task DeviceExistsForDifferentSite_GetList_ReturnsEmptyList()
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

            var expectedAsset = AdtSetupHelper.CreateTwin("AirHandlingUnit", "Test Asset", otherSiteId);
            var twinId = setup.AddTwin(expectedAsset);
            setup.AddRelationship(twinId, AdtSetupHelper.MakeId(otherSiteId, "Level1"), Relationships.LocatedIn);

            var expectedDevice = AdtSetupHelper.CreateTwin("Controller", "Test Device", otherSiteId);
            var expectedDeviceId = setup.AddTwin(expectedDevice);
            setup.AddRelationship(expectedDeviceId, AdtSetupHelper.MakeId(otherSiteId, "Level1"), Relationships.LocatedIn);

            var expectedPoint1 = AdtSetupHelper.CreateTwin("Setpoint", "Point2", otherSiteId);
            twinId = setup.AddTwin(expectedPoint1);
            setup.AddRelationship(twinId, expectedAsset.Id, Relationships.IsCapabilityOf);
            setup.AddRelationship(twinId, expectedDeviceId, Relationships.HostedBy);

            var expectedPoint2 = AdtSetupHelper.CreateTwin("Setpoint", "Point3", otherSiteId);
            twinId = setup.AddTwin(expectedPoint2);
            setup.AddRelationship(twinId, expectedAsset.Id, Relationships.IsCapabilityOf);
            setup.AddRelationship(twinId, expectedDeviceId, Relationships.HostedBy);

            dts.Reload();

            using var client = server.CreateClient(null, userId);

            var response = await client.GetAsync($"sites/{siteId}/devices");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<List<DeviceDto>>();
            result.Should().BeEmpty();
        }
    }
}
