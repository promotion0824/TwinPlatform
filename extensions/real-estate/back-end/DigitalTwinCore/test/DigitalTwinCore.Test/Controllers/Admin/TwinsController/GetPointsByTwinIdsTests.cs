using CachelessMigrationTests;
using DigitalTwinCore.Entities;
using DigitalTwinCore.Models;
using DigitalTwinCore.Services;
using DigitalTwinCore.Test.MockServices;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Willow.Tests.Infrastructure.MockServices;
using Workflow.Tests;
using Xunit;
using Xunit.Abstractions;

namespace DigitalTwinCore.Test.Controllers.Admin.TwinsController;

public class GetPointsByTwinIdsTests : BaseInMemoryTest
{
    public GetPointsByTwinIdsTests(ITestOutputHelper output) : base(output)
    {
    }


    [Fact]
    public async Task PointTwinExists_GetPointsByTwinIds_ReturnsPointTwins()
    {
        var userId = Guid.NewGuid();
        var siteId = Guid.NewGuid();
        var trendId1 = Guid.NewGuid();
        var trendId2 = Guid.NewGuid();

        using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);

        var serverArrangement = server.Arrange();
        var assetService = serverArrangement.MainServices.GetRequiredService<IAssetService>() as MockCachelessAssetService;

        var context = serverArrangement.CreateDbContext<DigitalTwinDbContext>();

        context.SiteSettings.Add(new SiteSettingEntity { SiteId = siteId, InstanceUri = "https://localhost" });
        context.SaveChanges();

        var dtsp = serverArrangement.MainServices.GetRequiredService<IDigitalTwinServiceProvider>();
        var dts = await dtsp.GetForSiteAsync(siteId) as TestDigitalTwinService;

        var setup = new AdtSetupHelper(dts);
        setup.SetupModels();

        var poinTwin1 = AdtSetupHelper.CreatePointTwin("AirHandlingUnit", "Test Asset", siteId, trendId1);
        var poinTwin2 = AdtSetupHelper.CreatePointTwin("AirHandlingUnit2", "Test Asset2", siteId, trendId2);
        var twinId1 = setup.AddTwin(poinTwin1);
        var twinId2 = setup.AddTwin(poinTwin2);
        dts.Reload();
        using var client = server.CreateClient(null, userId);
        var expectedResults = new List<PointTwinDto> { new PointTwinDto {PointTwinId = twinId1, TrendId = trendId1 }, new PointTwinDto { PointTwinId = twinId2, TrendId = trendId2 } };
        assetService.PointTwinDtos = expectedResults;
        var url = $"admin/sites/{siteId}/twins/twinIds/points";
        var response = await client.PostAsJsonAsync(url,new List<string> { twinId1, twinId2 } );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadAsAsync<List<PointTwinDto>>();
        result.Should().NotBeNull();
        result.Count.Should().Be(2);
    }

    [Fact]
    public async Task NoTokedProvided_GetPointsByTwinIds_ReturnsUnAuthorized()
    {
        using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);
        using var client = server.CreateClient();

        var response = await client.PostAsJsonAsync($"admin/sites/{Guid.NewGuid()}/twins/twinIds/points", new List<string>());
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

}

