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
using System.Threading.Tasks;
using DigitalTwinCore.Constants;
using Willow.Tests.Infrastructure;
using Workflow.Tests;
using Xunit;
using Xunit.Abstractions;

namespace DigitalTwinCore.Test.Controllers.Admin.RelationshipsController
{
    public class PostDocumentTests : BaseInMemoryTest
    {
        public PostDocumentTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task RelationshipExists_PostRelationship_UpdatesRelationship()
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
            setup.SetupTwins(null, siteId);

            var expectedAsset = AdtSetupHelper.CreateTwin("AirHandlingUnit", "Test Asset", siteId);
            var twinId = setup.AddTwin(expectedAsset);
            var existingRelationship = setup.AddRelationship(twinId, AdtSetupHelper.MakeId(siteId, "Level1"), Relationships.LocatedIn);
            dts.Reload();

            using var client = server.CreateClient(null, userId);

            var expectedRelationship = new RelationshipDto
            {
                Id = existingRelationship.Id,
                Name = existingRelationship.Name,
                SourceId = existingRelationship.SourceId,
                TargetId = AdtSetupHelper.MakeId(siteId, "Level2")
            };

            var response = await client.PostAsJsonAsync($"admin/sites/{siteId}/twins/{twinId}/relationships/{expectedRelationship.Id}", expectedRelationship);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location.Should().Be($"/admin/sites/{siteId}/twins/{twinId}/relationships/{expectedRelationship.Id}");
            var result = await response.Content.ReadAsAsync<RelationshipDto>();
            result.Should().BeEquivalentTo(expectedRelationship, config => config.Excluding(r => r.Target).Excluding(r => r.Source));
        }


        [Fact]
        public async Task NoRelationshipExists_PostRelationship_CreatesRelationship()
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
            setup.SetupTwins(null, siteId);

            var expectedAsset = AdtSetupHelper.CreateTwin("AirHandlingUnit", "Test Asset", siteId);
            var twinId = setup.AddTwin(expectedAsset);
            dts.Reload();

            using var client = server.CreateClient(null, userId);

            var expectedRelationship = new RelationshipDto
            {
                Id = Guid.NewGuid().ToString(),
                Name = Relationships.LocatedIn,
                SourceId = twinId,
                TargetId = AdtSetupHelper.MakeId(siteId, "Level2")
            };

            var response = await client.PostAsJsonAsync($"admin/sites/{siteId}/twins/{twinId}/relationships/{expectedRelationship.Id}", expectedRelationship);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location.Should().Be($"/admin/sites/{siteId}/twins/{twinId}/relationships/{expectedRelationship.Id}");
            var result = await response.Content.ReadAsAsync<RelationshipDto>();
            result.Should().BeEquivalentTo(expectedRelationship, config => config.Excluding(r => r.Target).Excluding(r => r.Source));
        }

    }
}
