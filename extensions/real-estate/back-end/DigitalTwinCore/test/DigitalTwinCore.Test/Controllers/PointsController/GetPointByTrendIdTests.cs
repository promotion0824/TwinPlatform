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
using Willow.Infrastructure;
using Willow.Tests.Infrastructure;
using Workflow.Tests;
using Xunit;
using Xunit.Abstractions;

namespace DigitalTwinCore.Test.Controllers.PointsController
{
    public class GetPointByTrendIdTests : BaseInMemoryTest
    {
        public GetPointByTrendIdTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact(Skip = "Fix later")]
        public async Task PointExist_GetPointByTrendId_ReturnsThatPoint()
        {
            var userId = Guid.NewGuid();
            var siteId = Guid.NewGuid();
            var assetId = Guid.NewGuid();
            var trendId = Guid.NewGuid();

            using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);
            var serverArrangement = server.Arrange();
            var context = serverArrangement.CreateDbContext<DigitalTwinDbContext>();
            context.SiteSettings.Add(new SiteSettingEntity { SiteId = siteId, InstanceUri = "https://localhost", SiteCodeForModelId = null });
            context.SaveChanges();

            var dtsp = serverArrangement.MainServices.GetRequiredService<IDigitalTwinServiceProvider>();
            var dts = await dtsp.GetForSiteAsync(siteId) as TestDigitalTwinService;
            var setup = new AdtSetupHelper(dts);
            setup.SetupModels();

            var expectedAsset = AdtSetupHelper.CreateTwin("Asset", "Asset1", siteId, assetId);
            var assetTwinId = setup.AddTwin(expectedAsset);
            var expectedPoint = AdtSetupHelper.CreateTwin("Setpoint", "Point1", siteId);
            expectedPoint.CustomProperties.Add(Properties.TrendID, trendId.ToString());
            var pointTwinId = setup.AddTwin(expectedPoint);
            setup.AddRelationship(pointTwinId, assetTwinId, Relationships.IsCapabilityOf);

            using var client = server.CreateClient(null, userId);
            var response = await client.GetAsync($"sites/{siteId}/points/trendId/{trendId}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<PointDto>();
            result.Id.Should().Be(expectedPoint.UniqueId);
            result.TrendId.Should().Be(trendId);
            result.Assets.Single().Id.Should().Be(assetId);
        }

        [Fact(Skip = "Fix later")]
        public async Task PointDoesNotExist_GetPointByTrendId_ReturnsNotFound()
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
            var response = await client.GetAsync($"sites/{siteId}/points/trendId/{Guid.NewGuid()}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            var error = await response.Content.ReadAsErrorResponseAsync();
            error.Message.Should().Contain("point");
        }
    }
}
