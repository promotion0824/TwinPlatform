using DigitalTwinCore.Dto;
using DigitalTwinCore.Entities;
using DigitalTwinCore.Services;
using DigitalTwinCore.Test.MockServices;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
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
    public class GetTwinVersionsTests : BaseInMemoryTest
    {
        public GetTwinVersionsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task TwinExists_GetTwinVersions_ReturnsTwins()
        {
            var userId = Guid.NewGuid();
            var siteId = Guid.NewGuid();
            var twinId = "dummy";

            using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);

            var serverArrangement = server.Arrange();

            var context = serverArrangement.CreateDbContext<DigitalTwinDbContext>();

            context.SiteSettings.Add(new SiteSettingEntity { SiteId = siteId, InstanceUri = "https://localhost" });
            context.SaveChanges();
                        
            var dtsp = serverArrangement.MainServices.GetRequiredService<IDigitalTwinServiceProvider>();
            var dts = await dtsp.GetForSiteAsync(siteId) as TestDigitalTwinService;

            serverArrangement.SetTwinVersionsDto(new TwinHistoryDto()
            {
                Versions = new List<TwinVersionDto>() { new TwinVersionDto() }
            });

            using var client = server.CreateClient(null, userId);

            var response = await client.GetAsync($"admin/sites/{siteId}/twins/{twinId}/history");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task NoTwinExists_GetTwinVersions_ReturnsNotFound()
        {
            var userId = Guid.NewGuid();
            var siteId = Guid.NewGuid();
            var twinId = "dummy";

            using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);

            var serverArrangement = server.Arrange();

            var context = serverArrangement.CreateDbContext<DigitalTwinDbContext>();

            context.SiteSettings.Add(new SiteSettingEntity { SiteId = siteId, InstanceUri = "https://localhost" });
            context.SaveChanges();
                        
            var dtsp = serverArrangement.MainServices.GetRequiredService<IDigitalTwinServiceProvider>();
            var dts = await dtsp.GetForSiteAsync(siteId) as TestDigitalTwinService;
            
            using var client = server.CreateClient(null, userId);

            var response = await client.GetAsync($"admin/sites/{siteId}/twins/{twinId}/history");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
