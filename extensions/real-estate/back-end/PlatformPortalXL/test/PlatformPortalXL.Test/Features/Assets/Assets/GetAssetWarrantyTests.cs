using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq.Contrib.HttpClient;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;
using Willow.Platform.Localization;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Assets.Assets
{
    public class GetAssetWarrantyTests : BaseInMemoryTest
    {
        public GetAssetWarrantyTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task ThereAreWarranty_GetWarranty_ReturnsNotFound()
        {
            var siteId = Guid.NewGuid();
            var assetId = Guid.NewGuid();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/assets/{assetId}/documents")
                    .ReturnsJson(new List<DigitalTwinDocument>());
                var response = await client.GetAsync($"sites/{siteId}/assets/{assetId}/warranty");

                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task ThereAreWarranty_GetWarranty_ReturnsWarranty()
        {
            var siteId = Guid.NewGuid();
            var assetId = Guid.NewGuid();
            var warrantyId = "123";

            var dtmiWillowPrefix = "dtmi:com:willowinc:";
            var modelId = dtmiWillowPrefix + "Warranty;1";

            var expectedDocuments = Fixture.Build<DigitalTwinDocument>()
                .With(x => x.ModelId, modelId)
                .With(x => x.TwinId, warrantyId)
                .CreateMany(1).ToList();

            var expectedProperties = new Dictionary<string, DigitalTwinProperty>()
            {
                { "name", new DigitalTwinProperty() { DisplayName = "name", Value = "Plumbing Water Heater", Kind = DigitalTwinPropertyKind.Property } },
                { "uniqueID", new DigitalTwinProperty() { DisplayName = "uniqueID", Value = "78bc1aed-47db-4d8c-a331-fe2ee4dab919", Kind = DigitalTwinPropertyKind.Property } },
                { "siteID", new DigitalTwinProperty() { DisplayName = "siteID", Value = "e719ac18-192b-4174-91db-b3a624f1f1a4", Kind = DigitalTwinPropertyKind.Property } },
                { "code",  new DigitalTwinProperty() { DisplayName = "code", Value = "DOC-WRT-00111", Kind = DigitalTwinPropertyKind.Property } },
                { "guarantor", new DigitalTwinProperty() { DisplayName = "guarantor", Value = "bnewton@fluidcontracting.com.au", Kind = DigitalTwinPropertyKind.Property } },
                { "duration", new DigitalTwinProperty() { DisplayName = "duration", Value = "00:00:00", Kind = DigitalTwinPropertyKind.Property }},
                { "startDate", new DigitalTwinProperty() { DisplayName = "startDate", Value = "2018-09-18T00:00:00.000Z", Kind = DigitalTwinPropertyKind.Property } },
                { "endDate", new DigitalTwinProperty() { DisplayName = "endDate", Value = "2018909-18T00:00:00.000Z", Kind = DigitalTwinPropertyKind.Property } },
            };

            var expectedWarranty = Fixture.Build<DigitalTwinAsset>()
                    .With(x => x.Properties, expectedProperties)
                    .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/assets/{assetId}/documents")
                    .ReturnsJson(expectedDocuments);

                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/assets/twinId/{warrantyId}")
                    .ReturnsJson(expectedWarranty);

                var response = await client.GetAsync($"sites/{siteId}/assets/{assetId}/warranty");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<AssetDetailDto>>();

                var expectedAssets = new List<DigitalTwinAsset>() { expectedWarranty }.Select(DigitalTwinAsset.MapToModel);
                var expectedResult = AssetDetailDto.MapFromModels(expectedAssets, new PassThruAssetLocalizer());

                foreach(var r in result)
                {
                    foreach(var p in r.Properties)
                    {
                        //p.Value = System.Text.Json.JsonSerializer.Deserialize<object>(System.Text.Json.JsonSerializer.Serialize(p.Value));
                        p.Value = p.Value.ToString();
                    }
                }

                result.Should().BeEquivalentTo(expectedResult);
            }
        }

        [Fact]
        public async Task NoWarranty_GetWarranty_ReturnsNotFound()
        {
            var siteId = Guid.NewGuid();
            var assetId = Guid.NewGuid();

            var dtmiWillowPrefix = "dtmi:com:willowinc:";
            var modelId = dtmiWillowPrefix + "ProductData;1";

            var expectedDocuments = Fixture.Build<DigitalTwinDocument>()
                .With(x => x.ModelId, modelId)
                .CreateMany(1).ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/assets/{assetId}/documents")
                    .ReturnsJson(expectedDocuments);

                var response = await client.GetAsync($"sites/{siteId}/assets/{assetId}/warranty");

                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }
    }
}
