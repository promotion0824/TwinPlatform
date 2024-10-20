using DigitalTwinCore.Dto;
using DigitalTwinCore.Entities;
using DigitalTwinCore.Models;
using DigitalTwinCore.Services;
using DigitalTwinCore.Test.MockServices;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using DigitalTwinCore.Constants;
using Willow.Tests.Infrastructure;
using Workflow.Tests;
using Xunit;
using Xunit.Abstractions;

namespace DigitalTwinCore.Test.Controllers.Admin.TwinsController
{
    public class GetTwinsWithRelationshipsTests : BaseInMemoryTest
    {
        public GetTwinsWithRelationshipsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact(Skip = "Fix later")]
        public async Task TwinsExist_GetTwins_ReturnsTwins()
        {
            var userId = Guid.NewGuid();
            var siteId = Guid.NewGuid();

            using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);

            var serverArrangement = server.Arrange();

            var context = serverArrangement.CreateDbContext<DigitalTwinDbContext>();

            context.SiteSettings.Add(new SiteSettingEntity { SiteId = siteId, InstanceUri = "https://localhost", SiteCodeForModelId = "B121" });
            context.SaveChanges();

            var dtsp = serverArrangement.MainServices.GetRequiredService<IDigitalTwinServiceProvider>();
            var dts = await dtsp.GetForSiteAsync(siteId) as TestDigitalTwinService;

            var setup = new AdtSetupHelper(dts);
            setup.SetupModels();

            var expectedTwins = new List<Twin>();

            var twin = AdtSetupHelper.CreateTwin("AirHandlingUnit", "MS-Test Asset", siteId);
            var twinId = setup.AddTwin(twin);
            expectedTwins.Add(twin);

            var siteTwin = AdtSetupHelper.CreateTwin("Land", "MS-Test Site", siteId);
            var siteTwinId = setup.AddTwin(siteTwin);
            expectedTwins.Add(siteTwin);

            setup.AddRelationship(twinId, siteTwinId, Relationships.LocatedIn);

            dts.Reload();

            using var client = server.CreateClient(null, userId);

            var response = await client.GetAsync($"admin/sites/{siteId}/twins/withrelationships");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<Page<TwinWithRelationshipsDto>>();

            result.Content.Count().Should().Be(2);

            foreach (var resultTwin in result.Content)
            {
                var expectedTwin = expectedTwins.Single(t => t.Id == resultTwin.Twin.Id);
                resultTwin.Twin.Id.Should().Be(expectedTwin.Id);
                resultTwin.Twin.Metadata.ModelId.Should().Be(expectedTwin.Metadata.ModelId);
                foreach (var property in resultTwin.Twin.CustomProperties)
                {
                    var expectedValue = expectedTwin.CustomProperties[property.Key].ToString();
                    ((JsonElement)property.Value).GetString().Should().Be(expectedValue);
                }
                if (resultTwin.Twin.Metadata.ModelId.Contains("AirHandlingUnit"))
                {
                    resultTwin.Relationships.Count.Should().Be(1);
                    resultTwin.Relationships.Single().TargetId.Should().Be(siteTwinId);
                }
                else
                {
                    resultTwin.Relationships.Count.Should().Be(0);
                }
            }
        }

        [Fact(Skip = "Fix later")]
        public async Task NoTwinsExists_GetTwins_ReturnsNoTwins()
        {
            var userId = Guid.NewGuid();
            var siteId = Guid.NewGuid();

            using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);

            var serverArrangement = server.Arrange();

            var context = serverArrangement.CreateDbContext<DigitalTwinDbContext>();

            context.SiteSettings.Add(new SiteSettingEntity { SiteId = siteId, InstanceUri = "https://localhost", SiteCodeForModelId = "B121" });
            context.SaveChanges();

            var dtsp = serverArrangement.MainServices.GetRequiredService<IDigitalTwinServiceProvider>();
            var dts = await dtsp.GetForSiteAsync(siteId) as TestDigitalTwinService;

            var setup = new AdtSetupHelper(dts);
            setup.SetupModels();
            dts.Reload();

            using var client = server.CreateClient(null, userId);

            var response = await client.GetAsync($"admin/sites/{siteId}/twins/withrelationships");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<Page<TwinWithRelationshipsDto>>();
            result.Content.Should().BeEmpty();
        }
    }
}
