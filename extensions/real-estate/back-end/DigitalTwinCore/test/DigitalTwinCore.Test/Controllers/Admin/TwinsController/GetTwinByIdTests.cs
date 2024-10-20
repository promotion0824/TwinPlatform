using DigitalTwinCore.Dto;
using DigitalTwinCore.Entities;
using DigitalTwinCore.Services;
using DigitalTwinCore.Test.MockServices;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Workflow.Tests;
using Xunit;
using Xunit.Abstractions;

namespace DigitalTwinCore.Test.Controllers.Admin.TwinsController
{
    public class GetTwinByIdTests : BaseInMemoryTest
    {
        public GetTwinByIdTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task TwinExists_GetTwinById_ReturnsTwin()
        {
            var userId = Guid.NewGuid();
            var siteId = Guid.NewGuid();

            using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);

            var serverArrangement = server.Arrange();

            var context = serverArrangement.CreateDbContext<DigitalTwinDbContext>();

            context.SiteSettings.Add(new SiteSettingEntity { SiteId = siteId, InstanceUri = "https://localhost" });
            context.SaveChanges();

            var dtsp = serverArrangement.MainServices.GetRequiredService<IDigitalTwinServiceProvider>();
            var dts = await dtsp.GetForSiteAsync(siteId) as TestDigitalTwinService;

            var setup = new AdtSetupHelper(dts);
            setup.SetupModels();

            var expectedTwin = AdtSetupHelper.CreateTwin("AirHandlingUnit", "Test Asset", siteId);
            var twinId = setup.AddTwin(expectedTwin);
            dts.Reload();

            using var client = server.CreateClient(null, userId);

            var response = await client.GetAsync($"admin/sites/{siteId}/twins/{twinId}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<TwinDto>();
            result.Id.Should().Be(expectedTwin.Id);
            result.Metadata.ModelId.Should().Be(expectedTwin.Metadata.ModelId);
            foreach (var property in result.CustomProperties)
            {
                var expectedValue = expectedTwin.CustomProperties[property.Key].ToString();
                ((JsonElement)property.Value).GetString().Should().Be(expectedValue);
            }
        }

        [Fact]
        public async Task NoTwinExists_GetTwinById_ReturnsNotFound()
        {
            var userId = Guid.NewGuid();
            var siteId = Guid.NewGuid();

            using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);

            var serverArrangement = server.Arrange();

            var context = serverArrangement.CreateDbContext<DigitalTwinDbContext>();

            context.SiteSettings.Add(new SiteSettingEntity { SiteId = siteId, InstanceUri = "https://localhost" });
            context.SaveChanges();

            var dtsp = serverArrangement.MainServices.GetRequiredService<IDigitalTwinServiceProvider>();
            var dts = await dtsp.GetForSiteAsync(siteId) as TestDigitalTwinService;

            var setup = new AdtSetupHelper(dts);
            setup.SetupModels();

            var expectedTwin = AdtSetupHelper.CreateTwin("AirHandlingUnit", "Test Asset", siteId);
            var twinId = setup.AddTwin(expectedTwin);
            dts.Reload();

            using var client = server.CreateClient(null, userId);

            var response = await client.GetAsync($"admin/sites/{siteId}/twins/{Guid.NewGuid()}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

    }
}
