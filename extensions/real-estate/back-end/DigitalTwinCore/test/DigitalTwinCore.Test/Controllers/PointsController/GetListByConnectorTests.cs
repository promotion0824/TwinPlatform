using DigitalTwinCore.Dto;
using DigitalTwinCore.Entities;
using DigitalTwinCore.Models.Connectors;
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
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Willow.Tests.Infrastructure;
using Workflow.Tests;
using Xunit;
using Xunit.Abstractions;

namespace DigitalTwinCore.Test.Controllers.PointsController
{
    public class GetListByConnectorTests : BaseInMemoryTest
    {
        public GetListByConnectorTests(ITestOutputHelper output) : base(output)
        {
        }

        [Theory(Skip = "Fix later")]
        [InlineData("AI", BacNetPointProperties.BacNetPointObjectType.AnalogInput, 0, 0, "EndianBig", ModbusPointProperties.ModbusEndianType.EndianBig, 0, 0)]
        [InlineData("AO", BacNetPointProperties.BacNetPointObjectType.AnalogOutput, 1, 1, "EndianLittle", ModbusPointProperties.ModbusEndianType.EndianLittle, 1, 1)]
        [InlineData("AV", BacNetPointProperties.BacNetPointObjectType.AnalogValue, 2, 2, "EndianBig", ModbusPointProperties.ModbusEndianType.EndianBig, 2, 2)]
        [InlineData("BI", BacNetPointProperties.BacNetPointObjectType.BinaryInput, 3, 3, "EndianLittle", ModbusPointProperties.ModbusEndianType.EndianLittle, 3, 3)]
        [InlineData("BO", BacNetPointProperties.BacNetPointObjectType.BinaryOutput, 4, 4, "EndianBig", ModbusPointProperties.ModbusEndianType.EndianBig, 0, 0)]
        [InlineData("BV", BacNetPointProperties.BacNetPointObjectType.BinaryValue, 5, 5, "EndianLittle", ModbusPointProperties.ModbusEndianType.EndianLittle, 1, 1)]
        [InlineData("DEV", BacNetPointProperties.BacNetPointObjectType.Device, 6, 6, "EndianBig", ModbusPointProperties.ModbusEndianType.EndianBig, 2, 2)]
        [InlineData("MSI", BacNetPointProperties.BacNetPointObjectType.MultistateInput, 7, 7, "EndianLittle", ModbusPointProperties.ModbusEndianType.EndianLittle, 3, 3)]
        [InlineData("MSO", BacNetPointProperties.BacNetPointObjectType.MultistateOutput, 8, 8, "EndianBig", ModbusPointProperties.ModbusEndianType.EndianBig, 0, 0)]
        [InlineData("MSV", BacNetPointProperties.BacNetPointObjectType.MultistateValue, 9, 9, "EndianLittle", ModbusPointProperties.ModbusEndianType.EndianLittle, 1, 1)]
        [InlineData("MSV", BacNetPointProperties.BacNetPointObjectType.MultistateValue, 10, 10, "EndianBig", ModbusPointProperties.ModbusEndianType.EndianBig, 2, 2)]
        [InlineData("unknown", BacNetPointProperties.BacNetPointObjectType.Unknown, -1, -1, "unknown", ModbusPointProperties.ModbusEndianType.Unknown, -1, -1)]
        public async Task PointsExist_GetList_ReturnsPoints(
            string bacnetObjectTypeProperty, 
            BacNetPointProperties.BacNetPointObjectType bacnetObjectTypeResult,
            int modbusDataTypeProperty,
            int modbusDataTypeResult,
            string modbusEndianTypeProperty,
            ModbusPointProperties.ModbusEndianType modbusEndianTypeResult,
            int modbusRegisterTypeProperty,
            int modbusRegisterTypeResult)
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
            var pointComms = new Dictionary<string, object>()
            {
                ["protocol"] = "Modbus",
                ["Modbus"] = new Dictionary<string, object>()
                {
                    ["dataType"] = modbusDataTypeProperty,
                    ["registerAddress"] = 1000,
                    ["swap"] = true,
                    [Properties.SlaveId] = 50,
                    ["endian"] = modbusEndianTypeProperty,
                    ["registerType"] = modbusRegisterTypeProperty,
                    ["scale"] = 100f
                }
            };
            expectedPoint1.CustomProperties["communication"] = pointComms;
            twinId = setup.AddTwin(expectedPoint1);
            setup.AddRelationship(twinId, expectedAsset.Id, Relationships.IsCapabilityOf);
            setup.AddRelationship(twinId, expectedDevice.Id, Relationships.HostedBy);

            var expectedPoint2 = AdtSetupHelper.CreateTwin("Setpoint", "Point3", siteId);

            pointComms = new Dictionary<string, object>()
            {
                ["protocol"] = "BACnet",
                ["BACnet"] = new Dictionary<string, object>()
                {
                    [Constants.Properties.DeviceId] = 1,
                    [Constants.Properties.ObjectType] = bacnetObjectTypeProperty,
                    [Constants.Properties.ObjectId] = 2
                }
            };
            expectedPoint2.CustomProperties["communication"] = pointComms;

            twinId = setup.AddTwin(expectedPoint2);
            setup.AddRelationship(twinId, expectedAsset.Id, Relationships.IsCapabilityOf);
            setup.AddRelationship(twinId, expectedDevice.Id, Relationships.HostedBy);

            using var client = server.CreateClient(null, userId);

            var response = await client.GetAsync($"sites/{siteId}/connectors/{connectorId}/points");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<List<PointDto>>();

            result.Count.Should().Be(2);
            result = result.OrderBy(d => d.Name).ToList();

            result[0].Id.Should().Be(expectedPoint1.UniqueId);
            result[1].Id.Should().Be(expectedPoint2.UniqueId);
            result[0].Metadata.Communication.Should().BeEquivalentTo(new PointCommunicationDto 
            { 
                Protocol = PointCommunicationProtocol.Modbus,
                Modbus = new ModbusPointProperties
                {
                    DataType = modbusDataTypeResult,
                    Endian = modbusEndianTypeResult,
                    RegisterAddress = 1000,
                    Swap = true,
                    SlaveId = 50,
                    RegisterType = modbusRegisterTypeResult,
                    Scale = 100f
                }
            });
            result[1].Metadata.Communication.Should().BeEquivalentTo(new PointCommunicationDto
            {
                Protocol = PointCommunicationProtocol.BACnet,
                BacNet = new BacNetPointProperties
                {
                    DeviceId = 1,
                    ObjectType = bacnetObjectTypeResult,
                    ObjectId = 2
                }
            });
            result.All(r => r.DeviceId == expectedDevice.UniqueId).Should().BeTrue();
            result.All(r => r.Assets == null).Should().BeTrue();
        }

        [Fact(Skip = "Fix later")]
        public async Task PointsExist_GetList_WithIncludeAssets_ReturnsPointsWithAssets()
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
            twinId = setup.AddTwin(expectedPoint1);
            setup.AddRelationship(twinId, expectedAsset.Id, Relationships.IsCapabilityOf);
            setup.AddRelationship(twinId, expectedDevice.Id, Relationships.HostedBy);

            var expectedPoint2 = AdtSetupHelper.CreateTwin("Setpoint", "Point3", siteId);
            twinId = setup.AddTwin(expectedPoint2);
            setup.AddRelationship(twinId, expectedAsset.Id, Relationships.IsCapabilityOf);
            setup.AddRelationship(twinId, expectedDevice.Id, Relationships.HostedBy);

            using var client = server.CreateClient(null, userId);

            var response = await client.GetAsync($"sites/{siteId}/connectors/{connectorId}/points?includeAssets=true");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<List<PointDto>>();

            result.Count.Should().Be(2);
            result = result.OrderBy(d => d.Name).ToList();

            result[0].Id.Should().Be(expectedPoint1.UniqueId);
            result[1].Id.Should().Be(expectedPoint2.UniqueId);
            result.All(r => r.DeviceId == expectedDevice.UniqueId).Should().BeTrue();
            result.All(r => r.Assets.Single().Id == expectedAsset.UniqueId).Should().BeTrue();
        }

#if false // Building/site-specific not supported
        [Fact]
        public async Task BuildingSpecificPointsExist_GetList_ReturnsPoints()
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
            expectedDevice.CustomProperties.Add(Properties.ConnectorID, connectorId);
            twinId = setup.AddTwin(expectedDevice);

            var expectedPoint1 = AdtSetupHelper.CreateTwinWithSiteCode("BuildingSpecific", "Setpoint", "Point2", siteId);
            twinId = setup.AddTwin(expectedPoint1);
            setup.AddRelationship(twinId, expectedAsset.Id, Relationships.IsCapabilityOf);
            setup.AddRelationship(twinId, expectedDevice.Id, Relationships.IsPartOf);

            var expectedPoint2 = AdtSetupHelper.CreateTwinWithSiteCode("BuildingSpecific", "Setpoint", "Point3", siteId);
            twinId = setup.AddTwin(expectedPoint2);
            setup.AddRelationship(twinId, expectedAsset.Id, Relationships.IsCapabilityOf);
            setup.AddRelationship(twinId, expectedDevice.Id, Relationships.IsPartOf);

            using var client = server.CreateClient(null, userId);

            var response = await client.GetAsync($"sites/{siteId}/connectors/{connectorId}/points");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<List<PointDto>>();

            result.Count.Should().Be(2);
            result = result.OrderBy(t => t.Id).ToList();

            result[0].ModelId.Should().Be("dtmi:com:willowinc:BuildingSpecific:Setpoint;1");
            result[0].Id.Should().Be(expectedPoint1.UniqueId);
            result[1].ModelId.Should().Be("dtmi:com:willowinc:BuildingSpecific:Setpoint;1");
            result[1].Id.Should().Be(expectedPoint2.UniqueId);
            result.All(r => r.DeviceId == expectedDevice.UniqueId).Should().BeTrue();
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

            var response = await client.GetAsync($"sites/{Guid.NewGuid()}/connectors/{Guid.NewGuid()}/points");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact(Skip = "Fix later")]
        public async Task PointsExistForDifferentConnectorId_GetList_ReturnsEmptyList()
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
            twinId = setup.AddTwin(expectedPoint1);
            setup.AddRelationship(twinId, expectedAsset.Id, Relationships.IsCapabilityOf);
            setup.AddRelationship(twinId, expectedDevice.Id, Relationships.HostedBy);

            var expectedPoint2 = AdtSetupHelper.CreateTwin("Setpoint", "Point3", siteId);
            twinId = setup.AddTwin(expectedPoint2);
            setup.AddRelationship(twinId, expectedAsset.Id, Relationships.IsCapabilityOf);
            setup.AddRelationship(twinId, expectedDevice.Id, Relationships.HostedBy);

            dts.Reload();

            using var client = server.CreateClient(null, userId);

            var response = await client.GetAsync($"sites/{siteId}/connectors/{connectorId}/points");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<List<PointDto>>();
            result.Should().BeEmpty();
        }
    }
}
