using DigitalTwinCore.Entities;
using DigitalTwinCore.Services;
using DigitalTwinCore.Test.MockServices;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Threading.Tasks;
using DigitalTwinCore.Constants;
using Willow.Tests.Infrastructure;
using Workflow.Tests;
using Xunit;
using Xunit.Abstractions;

namespace DigitalTwinCore.Test.Controllers.Admin.RelationshipsController
{
    public class DeleteRelationshipTests : BaseInMemoryTest
    {
        public DeleteRelationshipTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task RelationshipExists_DeleteRelationship_DeletesRelationship()
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
            var expectedRelationship = setup.AddRelationship(twinId, AdtSetupHelper.MakeId(siteId, "Level1"), Relationships.LocatedIn);
            dts.Reload();

            using var client = server.CreateClient(null, userId);

            var response = await client.DeleteAsync($"admin/sites/{siteId}/twins/{twinId}/relationships/{expectedRelationship.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }


        [Fact]
        public async Task NoRelationshipExists_DeleteRelationship_ReturnsNotFound()
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

            var response = await client.DeleteAsync($"admin/sites/{siteId}/twins/{twinId}/relationships/{Guid.NewGuid()}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

    }
}
