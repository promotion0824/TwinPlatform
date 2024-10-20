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
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Workflow.Tests;
using Xunit;
using Xunit.Abstractions;

namespace DigitalTwinCore.Test.Controllers.Admin.TwinsController
{
    public class GetTwinFieldsTests : BaseInMemoryTest
    {
        public GetTwinFieldsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task TwinExists_GetTwinFields_ReturnsTwins()
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

            var expectedTwinFields = new TwinFieldsDto()
            {
                ExpectedFields = new List<string>() { "TrendId" }
            };

            serverArrangement.SetTwinFieldsDto(expectedTwinFields);

            using var client = server.CreateClient(null, userId);

            var response = await client.GetAsync($"admin/sites/{siteId}/twins/{twinId}/fields");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<TwinFieldsDto>();
            result.Should().BeEquivalentTo(expectedTwinFields);
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

            var response = await client.GetAsync($"admin/sites/{siteId}/twins/{twinId}/fields");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
