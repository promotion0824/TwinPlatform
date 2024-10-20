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
using System.Threading.Tasks;
using DigitalTwinCore.Constants;
using Willow.Tests.Infrastructure;
using Workflow.Tests;
using Xunit;
using Xunit.Abstractions;

namespace DigitalTwinCore.Test.Controllers.AssetsController
{
    public class GetAssetDocumentsTests : BaseInMemoryTest
    {
        public GetAssetDocumentsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact(Skip = "Fix later")]
        public async Task NotUsingBuildingSpecificModels_GetAssetDocuments_ReturnsDocument()
        {
            var userId = Guid.NewGuid();
            var siteId = Guid.NewGuid();
            var floor1Id = Guid.NewGuid();
            var floor2Id = Guid.NewGuid();

            using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);

            var serverArrangement = server.Arrange();

            var context = serverArrangement.CreateDbContext<DigitalTwinDbContext>();

            context.SiteSettings.Add(new SiteSettingEntity { SiteId = siteId, InstanceUri = "https://localhost", SiteCodeForModelId = null });
            context.SaveChanges();

            var dtsp = serverArrangement.MainServices.GetRequiredService<IDigitalTwinServiceProvider>();
            var dts = await dtsp.GetForSiteAsync(siteId) as TestDigitalTwinService;

            var setup = new AdtSetupHelper(dts);
            setup.SetupModels();
            setup.SetupTwins(null, siteId, floor1Id, floor2Id);

            var expectedAsset = AdtSetupHelper.CreateTwin("AirHandlingUnit", "Test Asset", siteId);
            var twinId = setup.AddTwin(expectedAsset);
            setup.AddRelationship(twinId, AdtSetupHelper.MakeId(siteId, "Level1"), Relationships.LocatedIn);

            var expectedDocument = AdtSetupHelper.CreateTwin("Warranty", "Test Warranty", siteId);
            expectedDocument.CustomProperties.Add(Properties.Url, "http://localhost/document.pdf");
            var documentId = setup.AddTwin(expectedDocument);
            setup.AddRelationship(documentId, twinId, Relationships.IsDocumentOf);
            dts.Reload();

            using var client = server.CreateClient(null, userId);

            var response = await client.GetAsync($"sites/{siteId}/assets/{expectedAsset.UniqueId}/documents");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<List<DocumentDto>>();

            result.Count.Should().Be(1);
            var document = result.Single();
            document.Name.Should().Be(expectedDocument.GetStringProperty(Properties.Name));
            document.Id.Should().Be(expectedDocument.UniqueId);
            document.Uri.Should().Be("http://localhost/document.pdf");
        }

        [Fact(Skip = "Fix later")]
        public async Task InvalidAssetId__GetAssetDocuments_ReturnsNotFound()
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
            setup.SetupTwins(null, siteId, Guid.NewGuid(), Guid.NewGuid());
            dts.Reload();

            using var client = server.CreateClient(null, userId);

            var response = await client.GetAsync($"sites/{siteId}/assets/{Guid.NewGuid()}/documents");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact(Skip = "Fix later")]
        public async Task AssetOfDifferentSite_GetAssetDocuments_ReturnsNotFound()
        {
            var userId = Guid.NewGuid();
            var siteId = Guid.NewGuid();
            var otherSiteId = Guid.NewGuid();

            using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);

            var serverArrangement = server.Arrange();

            var context = serverArrangement.CreateDbContext<DigitalTwinDbContext>();

            context.SiteSettings.Add(new SiteSettingEntity { SiteId = siteId, InstanceUri = "https://localhost", SiteCodeForModelId = null });
            context.SaveChanges();

            var dtsp = serverArrangement.MainServices.GetRequiredService<IDigitalTwinServiceProvider>();
            var dts = await dtsp.GetForSiteAsync(siteId) as TestDigitalTwinService;

            var setup = new AdtSetupHelper(dts);
            setup.SetupModels();
            setup.SetupTwins(null, siteId, Guid.NewGuid(), Guid.NewGuid());
            setup.SetupTwins(null, otherSiteId);

            var assetOfDifferentSite = AdtSetupHelper.CreateTwin("AirHandlingUnit", "Test Asset", otherSiteId);
            var twinId = setup.AddTwin(assetOfDifferentSite);
            setup.AddRelationship(twinId, AdtSetupHelper.MakeId(otherSiteId, "Level1"), Relationships.LocatedIn);

            var expectedDocument = AdtSetupHelper.CreateTwin("Warranty", "Test Warranty", siteId);
            expectedDocument.CustomProperties.Add(Properties.Url, "http://localhost/document.pdf");
            var documentId = setup.AddTwin(expectedDocument);
            setup.AddRelationship(documentId, twinId, Relationships.IsDocumentOf);
            dts.Reload();

            using var client = server.CreateClient(null, userId);

            var response = await client.GetAsync($"sites/{siteId}/assets/{assetOfDifferentSite.UniqueId}/documents");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }


        [Fact(Skip = "Fix later")]
        public async Task UsingBuildingSpecificModels_GetAssetDocuments_ReturnsDocument()
        {
            var userId = Guid.NewGuid();
            var siteId = Guid.NewGuid();

            using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);

            var serverArrangement = server.Arrange();

            var context = serverArrangement.CreateDbContext<DigitalTwinDbContext>();

            context.SiteSettings.Add(new SiteSettingEntity { SiteId = siteId, InstanceUri = "https://localhost", SiteCodeForModelId = "BuildingSpecific" });
            context.SaveChanges();

            var dtsp = serverArrangement.MainServices.GetRequiredService<IDigitalTwinServiceProvider>();
            var dts = await dtsp.GetForSiteAsync(siteId) as TestDigitalTwinService;

            var setup = new AdtSetupHelper(dts);
            setup.SetupModels();
            setup.SetupTwins(null);
            setup.SetupTwins("BuildingSpecific", siteId);

            var expectedAsset = AdtSetupHelper.CreateTwinWithSiteCode("BuildingSpecific", "AirHandlingUnit", "Test Asset", siteId);
            var twinId = setup.AddTwin(expectedAsset);
            setup.AddRelationship(twinId, AdtSetupHelper.MakeId(siteId, "Level1"), Relationships.LocatedIn);

            var expectedDocument = AdtSetupHelper.CreateTwinWithSiteCode("BuildingSpecific", "Warranty", "Test Warranty", siteId);
            expectedDocument.CustomProperties.Add(Properties.Url, "http://localhost/document.pdf");
            var documentId = setup.AddTwin(expectedDocument);
            setup.AddRelationship(documentId, twinId, Relationships.IsDocumentOf);
            dts.Reload();

            using var client = server.CreateClient(null, userId);

            var response = await client.GetAsync($"sites/{siteId}/assets/{expectedAsset.UniqueId}/documents");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<List<DocumentDto>>();

            result.Count.Should().Be(1);
            var document = result.Single();
            document.Name.Should().Be(expectedDocument.GetStringProperty(Properties.Name));
            document.Id.Should().Be(expectedDocument.UniqueId);
            document.Uri.Should().Be("http://localhost/document.pdf");
        }
    }
}
