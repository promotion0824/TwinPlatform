using DigitalTwinCore.Dto;
using DigitalTwinCore.Entities;
using DigitalTwinCore.Services;
using DigitalTwinCore.Test.MockServices;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Workflow.Tests;
using Xunit;
using Xunit.Abstractions;

namespace DigitalTwinCore.Test.Controllers.Admin.TwinsController
{
    public class PostTwinTests : BaseInMemoryTest
    {
        public PostTwinTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact(Skip = "Twin Platform migration failure")]
        public async Task CreateNewTwin_PostTwin_ReturnsCreatedTwin()
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
            dts.Reload();

            using var client = server.CreateClient(null, userId);

            var expectedTwin = AdtSetupHelper.CreateTwin("AirHandlingUnit", "Test Asset", siteId);

            var response = await client.PostAsJsonAsync($"admin/sites/{siteId}/twins", TwinDto.MapFrom(expectedTwin));

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location.Should().Be($"/admin/sites/{siteId}/twins/{expectedTwin.Id}");

            var result = await response.Content.ReadAsAsync<TwinDto>();
            result.Id.Should().Be(expectedTwin.Id);
            result.Metadata.ModelId.Should().Be(expectedTwin.Metadata.ModelId);
            foreach (var property in result.CustomProperties)
            {
                var expectedValue = expectedTwin.CustomProperties[property.Key].ToString();
                ((JsonElement)property.Value).GetString().Should().Be(expectedValue);
            }
        }

        [Fact(Skip = "Twin Platform migration failure")]
        public async Task UpdateExistingTwin_PostTwin_ReturnsUpdatedTwin()
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

            var originalTwin = AdtSetupHelper.CreateTwin("AirHandlingUnit", "Test Asset", siteId);
            var twinId = setup.AddTwin(originalTwin);

            dts.Reload();

            using var client = server.CreateClient(null, userId);

            var expectedTwin = AdtSetupHelper.CreateTwin("AirHandlingUnit", "Test Asset", siteId, originalTwin.UniqueId);

            var response = await client.PostAsJsonAsync($"admin/sites/{siteId}/twins", TwinDto.MapFrom(expectedTwin));

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location.Should().Be($"/admin/sites/{siteId}/twins/{expectedTwin.Id}");

            var result = await response.Content.ReadAsAsync<TwinDto>();
            result.Id.Should().Be(expectedTwin.Id);
            result.Metadata.ModelId.Should().Be(expectedTwin.Metadata.ModelId);
            foreach (var property in result.CustomProperties)
            {
                var expectedValue = expectedTwin.CustomProperties[property.Key].ToString();
                ((JsonElement)property.Value).GetString().Should().Be(expectedValue);
            }
        }

        [Fact(Skip = "Twin Platform migration failure")]
        public async Task UpdateExistingTwinDifferentUniqueId_PostTwin_Fails()
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

            var originalTwin = AdtSetupHelper.CreateTwin("AirHandlingUnit", "Test Asset", siteId);
            var twinId = setup.AddTwin(originalTwin);

            dts.Reload();

            using var client = server.CreateClient(null, userId);

            var expectedTwin = AdtSetupHelper.CreateTwin("AirHandlingUnit", "Test Asset", siteId);

            var response = await client.PostAsJsonAsync($"admin/sites/{siteId}/twins", TwinDto.MapFrom(expectedTwin));

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact(Skip = "Twin Platform migration failure")]
        public async Task CreateNewTwinWithExistingUniqueId_PostTwin_Fails()
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

            var originalTwin = AdtSetupHelper.CreateTwin("AirHandlingUnit", "Test Asset 1", siteId);
            var twinId = setup.AddTwin(originalTwin);

            dts.Reload();

            using var client = server.CreateClient(null, userId);

            var expectedTwin = AdtSetupHelper.CreateTwin("AirHandlingUnit", "Test Asset 2", siteId, originalTwin.UniqueId);

            var response = await client.PostAsJsonAsync($"admin/sites/{siteId}/twins", TwinDto.MapFrom(expectedTwin));

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
