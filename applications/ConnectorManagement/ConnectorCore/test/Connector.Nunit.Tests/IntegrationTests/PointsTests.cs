namespace Connector.Nunit.Tests.IntegrationTests;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Connector.Nunit.Tests.Infrastructure.Extensions;
using Connector.Nunit.Tests.TestData;
using ConnectorCore.Common.Models;
using ConnectorCore.Entities;
using ConnectorCore.Models;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NUnit.Framework;

public class PointsTests
{
    [Test]
    public async Task GetPoints_ReturnsPoints()
    {
        using var client = IntegrationFixture.Server.CreateClientRandomUser();
        var options = (IOptions<AppSettings>)IntegrationFixture.Server.Services.GetService(typeof(IOptions<AppSettings>))!;
        options.Value.PointsPageSize = 1000;
        var pageSize = options.Value.PointsPageSize;

        var points = await client.GetJsonAsync<GetPointsResult>("sites/6a8cb6ef-f23b-4608-a08b-0b779fd616cb/points?includeEquipment=true");
        points.Data.Should().HaveCount(cnt => cnt <= pageSize);

        var point1 = points.Data.First(q => q.EntityId == Constants.PointId1);
        point1.Tags.Should().NotBeNullOrEmpty();
        point1.Tags.Should().Contain("Tag4");

        var equipment = points.Data.SelectMany(p => p.Equipment).ToList();
        equipment.Should().Contain(e => e.Id == Constants.EquipmentId1);
    }

    [Test]
    public async Task GetPoints_WithEquipmentId_Returns_EquipmentPoints()
    {
        using var client = IntegrationFixture.Server.CreateClientRandomUser();

        var equipmentId = Constants.EquipmentId1;

        var points = await client.GetJsonAsync<GetPointsResult>($"sites/6a8cb6ef-f23b-4608-a08b-0b779fd616cb/points?equipmentId={equipmentId}&includeEquipment=true");
        points.Data.Should().HaveCount(2);
        points.Data.Select(q => q.EntityId).Should().BeEquivalentTo(new List<Guid> { Constants.PointId1, Constants.PointId2 });

        var point1 = points.Data.First(q => q.EntityId == Constants.PointId1);
        point1.Tags.Should().NotBeNullOrEmpty();
        point1.Tags.Should().Contain("Tag4");

        var equipment = points.Data.SelectMany(p => p.Equipment).ToList();
        equipment.Should().Contain(e => e.Id == Constants.EquipmentId1);
    }

    [Test]
    public async Task GetPoints_WithWrongEquipmentId_Returns_NoItems()
    {
        using var client = IntegrationFixture.Server.CreateClientRandomUser();

        var equipmentId = Guid.NewGuid();

        var points = await client.GetJsonAsync<GetPointsResult>($"sites/6a8cb6ef-f23b-4608-a08b-0b779fd616cb/points?equipmentId={equipmentId}");
        points.Data.Should().HaveCount(0);
    }

    [Test]
    public async Task GetPoints_WithBadContToken_Returns_BadRequest()
    {
        using var client = IntegrationFixture.Server.CreateClientRandomUser();
        const string ctoken = "1312231";
        var error = null as string;

        try
        {
            await client.GetJsonAsync<GetPointsResult>($"sites/6a8cb6ef-f23b-4608-a08b-0b779fd616cb/points?continuationToken={ctoken}");
        }
        catch (HttpRequestException ex)
        {
            error = ex.ToString();
        }

        error.Should().NotBeNull();
        error.Should().Contain("400 (Bad Request)");
    }

    [Test]
    public async Task GetPoints_WithWrongContToken_Returns_BadRequest()
    {
        using var client = IntegrationFixture.Server.CreateClientRandomUser();
        var ctoken = Guid.NewGuid();
        var error = null as string;

        try
        {
            await client.GetJsonAsync<GetPointsResult>($"sites/6a8cb6ef-f23b-4608-a08b-0b779fd616cb/points?continuationToken={ctoken}");
        }
        catch (HttpRequestException ex)
        {
            error = ex.ToString();
        }

        error.Should().NotBeNull();
        error.Should().Contain("400 (Bad Request)");
    }

    [Test]
    public async Task GetPoints_WithContToken_Returns_NextPage()
    {
        using var client = IntegrationFixture.Server.CreateClientRandomUser();
        var options = (IOptions<AppSettings>)IntegrationFixture.Server.Services.GetService(typeof(IOptions<AppSettings>))!;
        options.Value.PointsPageSize = 1000;

        var pointsAll = await client.GetJsonAsync<GetPointsResult>($"sites/6a8cb6ef-f23b-4608-a08b-0b779fd616cb/points");
        pointsAll.Data.Should().NotBeEmpty();

        var pageSize = options.Value.PointsPageSize = 2;

        var pointsPage1 = await client.GetJsonAsync<GetPointsResult>($"sites/6a8cb6ef-f23b-4608-a08b-0b779fd616cb/points");
        pointsPage1.Data.Should().NotBeEmpty();
        pointsPage1.Data.Should().BeEquivalentTo(pointsAll.Data.Take(pageSize), q => q.WithStrictOrdering());
        pointsPage1.ContinuationToken.Should().NotBeEmpty();

        var pointsPage2 = await client.GetJsonAsync<GetPointsResult>($"sites/6a8cb6ef-f23b-4608-a08b-0b779fd616cb/points?continuationToken={pointsPage1.ContinuationToken}");
        pointsPage2.Data.Should().NotBeEmpty();
        pointsPage2.Data.Should().BeEquivalentTo(pointsAll.Data.Skip(pageSize).Take(pageSize), q => q.WithStrictOrdering());
    }

    [Test]
    public async Task GetPoints_WithContToken_Returns_LastPageWithoutToken()
    {
        using var client = IntegrationFixture.Server.CreateClientRandomUser();
        var options = (IOptions<AppSettings>)IntegrationFixture.Server.Services.GetService(typeof(IOptions<AppSettings>))!;
        options.Value.PointsPageSize = 1000;

        var pointsAll = await client.GetJsonAsync<GetPointsResult>($"sites/6a8cb6ef-f23b-4608-a08b-0b779fd616cb/points");
        pointsAll.Data.Should().NotBeEmpty();

        var pageSize = options.Value.PointsPageSize = (pointsAll.Data.Count / 2) + 2;

        var pointsPage1 = await client.GetJsonAsync<GetPointsResult>($"sites/6a8cb6ef-f23b-4608-a08b-0b779fd616cb/points");
        pointsPage1.Data.Should().NotBeEmpty();
        pointsPage1.Data.Should().BeEquivalentTo(pointsAll.Data.Take(pageSize), q => q.WithStrictOrdering());
        pointsPage1.ContinuationToken.Should().NotBeEmpty();

        var pointsPage2 = await client.GetJsonAsync<GetPointsResult>($"sites/6a8cb6ef-f23b-4608-a08b-0b779fd616cb/points?continuationToken={pointsPage1.ContinuationToken}");
        pointsPage2.Data.Should().NotBeEmpty();
        pointsPage2.Data.Should().BeEquivalentTo(pointsAll.Data.Skip(pageSize).Take(pageSize), q => q.WithStrictOrdering());
        pointsPage2.ContinuationToken.Should().BeNull();
    }

    [Test]
    public async Task GetPoints_WithEquipmentId_WithBadContToken_Returns_BadRequest()
    {
        using var client = IntegrationFixture.Server.CreateClientRandomUser();
        const string ctoken = "1312231";
        var error = null as string;

        try
        {
            await client.GetJsonAsync<GetPointsResult>($"sites/6a8cb6ef-f23b-4608-a08b-0b779fd616cb/points?equipmentId={Constants.EquipmentId1}&continuationToken={ctoken}");
        }
        catch (HttpRequestException ex)
        {
            error = ex.ToString();
        }

        error.Should().NotBeNull();
        error.Should().Contain("400 (Bad Request)");
    }

    [Test]
    public async Task GetPoints_WithEquipmentId_WithWrongContToken_Returns_BadRequest()
    {
        using var client = IntegrationFixture.Server.CreateClientRandomUser();
        var ctoken = Guid.NewGuid();
        var error = null as string;

        try
        {
            await client.GetJsonAsync<GetPointsResult>($"sites/6a8cb6ef-f23b-4608-a08b-0b779fd616cb/points?equipmentId={Constants.EquipmentId1}&continuationToken={ctoken}");
        }
        catch (HttpRequestException ex)
        {
            error = ex.ToString();
        }

        error.Should().NotBeNull();
        error.Should().Contain("400 (Bad Request)");
    }

    [Test]
    public async Task GetPoint_ReturnsPoint()
    {
        using var client = IntegrationFixture.Server.CreateClientRandomUser();
        var point = await client.GetJsonAsync<PointEntity>("points/1c58f9d2-6d32-4b2f-9b6a-dd5255714bf1");
        point.EntityId.Should().Be(Guid.Parse("1c58f9d2-6d32-4b2f-9b6a-dd5255714bf1"));
        point.Tags.Should().NotBeNull();
        point.Tags.Should().NotBeEmpty();
        point.Tags.Should().Contain(t => t.Id == Constants.TagId4);
        point.IsEnabled.Should().BeTrue();
    }

    [Test]
    public async Task GetPoint_WrongFormatGuid_Returns400()
    {
        using var client = IntegrationFixture.Server.CreateClientRandomUser();
        var response = await client.GetAsync(new Uri(client.BaseAddress + "points/ca32df47-d84c-4aef-b18d-1b2fa5cd686"));
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task GetPoint_WrongGuid_Returns404()
    {
        using var client = IntegrationFixture.Server.CreateClientRandomUser();
        var response = await client.GetAsync(new Uri(client.BaseAddress + "sites/6a8cb6ef-f23b-4608-a08b-0b779fd616cb/devices/ca32df47-d84c-4aef-b18d-1b2fa5cd686a"));
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task GetPointWithDeviceEquipment_ReturnsPointWithDeviceEquipment()
    {
        using var client = IntegrationFixture.Server.CreateClientRandomUser();
        var point = await client.GetJsonAsync<PointEntity>("points/77e1f782-6588-4276-a5c2-fba9d99396b8?includeDevice=true&includeEquipment=true");
        point.EntityId.Should().Be(Guid.Parse("77e1f782-6588-4276-a5c2-fba9d99396b8"));
        point.Device.Should().NotBeNull();
        point.Device.Id.Should().Be(Guid.Parse("ca32df47-d84c-4aef-b18d-1b2fa5cd6868"));
        point.Equipment.Should().NotBeNull();
        point.Equipment.Should().HaveCountGreaterThan(0);
        point.Equipment.Should().HaveCount(1);
        point.Equipment.Select(q => q.Id).Should().Contain(Constants.EquipmentId1);
    }

    [Test]
    public async Task GetPointsByDevice_ReturnsPoints()
    {
        using var client = IntegrationFixture.Server.CreateClientRandomUser();
        var points = await client.GetJsonAsync<List<PointEntity>>($"sites/6a8cb6ef-f23b-4608-a08b-0b779fd616cb/devices/{Constants.DeviceId1}/points");
        points.Should().Contain(p => p.EntityId == Constants.PointId1);
        points.Should().Contain(p => p.EntityId == Constants.PointId2);
    }

    [Test]
    public async Task GetPointsByConnector_ReturnsPoints()
    {
        using var client = IntegrationFixture.Server.CreateClientRandomUser();
        var points = await client.GetJsonAsync<List<PointEntity>>($"connectors/{Constants.ConnectorId1}/points");
        points.Should().Contain(p => p.EntityId == Constants.PointId1);
        points.Should().Contain(p => p.EntityId == Constants.PointId2);
    }

    [Test]
    public async Task GetPointsByTagName_ReturnsPoints()
    {
        const string tagName = "Tag4";
        using var client = IntegrationFixture.Server.CreateClientRandomUser();
        var points = await client.GetJsonAsync<List<PointEntity>>($"sites/{Constants.SiteIdDefault}/points/bytag/{tagName}");
        points.Should().Contain(p => p.EntityId == Constants.PointId1);
    }

    [Test]
    public async Task GetPointsByTagName_WithEquipments_ReturnsPointsWithEquipments()
    {
        const string tagName = "Tag4";
        using var client = IntegrationFixture.Server.CreateClientRandomUser();
        var points = await client.GetJsonAsync<List<PointEntity>>($"sites/{Constants.SiteIdDefault}/points/bytag/{tagName}?includeEquipment=true");
        points.Should().Contain(p => p.EntityId == Constants.PointId1);
        var point = points.First(p => p.EntityId == Constants.PointId1);

        point.Equipment.Should().HaveCount(1);
        point.Equipment.Should().Contain(e => e.Id == Constants.EquipmentId1);
    }
}
