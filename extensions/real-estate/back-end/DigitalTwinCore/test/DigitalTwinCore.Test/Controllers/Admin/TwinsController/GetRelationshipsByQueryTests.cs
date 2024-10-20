using DigitalTwinCore.Dto;
using DigitalTwinCore.Models;
using DigitalTwinCore.Services;
using DigitalTwinCore.Services.AdtApi;
using DigitalTwinCore.Services.Adx;
using DigitalTwinCore.Services.Cacheless;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Workflow.Tests;
using Xunit;
using Xunit.Abstractions;
using AutoFixture.Xunit2;
using Azure.DigitalTwins.Core;
using AutoFixture;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace DigitalTwinCore.Test.Controllers.Admin.TwinsController
{
    public class GetRelationshipsByQueryTests : BaseInMemoryTest
    {
        private CachelessAdtService digitalTwinService;
        private Mock<IAdtApiService> adtApiServiceMock;
        private Mock<IAdxHelper> adxHelperMock;
        private Mock<ILogger<CachelessAdtService>> loggerMock;
        private readonly IConfiguration configuration;

        private const string LevelTwinId = "Site - L10";
        private const string SiteTwinId = "Site";

        public GetRelationshipsByQueryTests(ITestOutputHelper output) : base(output)
        {
            adtApiServiceMock = new Mock<IAdtApiService>();
            adxHelperMock = new Mock<IAdxHelper>();
            loggerMock = new Mock<ILogger<CachelessAdtService>>();

            var inMemorySettings = new Dictionary<string, string> { { "MaxTwinsAllowedToQuery", "50" } };
            configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
        }

        [Theory]
        [AutoData]
        public async Task TwinWithRelationshipsExists_GetLocationRelationships_ReturnsRelationships(string twinId)
        {
            var expectedTwins = CreateTargetTwins();
            Azure.Page<BasicDigitalTwin> page = Azure.Page<BasicDigitalTwin>.FromValues(expectedTwins.AsReadOnly(), continuationToken: null, Mock.Of<Azure.Response>());
            Azure.AsyncPageable<BasicDigitalTwin> asyncPageable = Azure.AsyncPageable<BasicDigitalTwin>.FromPages(new[] { page });
            adtApiServiceMock.Setup(x => x.QueryTwins<BasicDigitalTwin>(It.IsAny<AzureDigitalTwinsSettings>(), It.IsAny<string>()))
                .Returns(asyncPageable);
            adtApiServiceMock.Setup(x => x.GetModels(It.IsAny<AzureDigitalTwinsSettings>()))
                .Returns(new List<AdtModel>());

            var levelRelationships = CreateRelationships(LevelTwinId, "isPartOf", SiteTwinId, 1);
            var siteRelationships = CreateRelationships(SiteTwinId, numOfRelationships: 2);
            adtApiServiceMock.SetupSequence(x => x.GetRelationships(It.IsAny<AzureDigitalTwinsSettings>(), It.IsAny<string>()))
              .ReturnsAsync(levelRelationships).ReturnsAsync(siteRelationships);

            digitalTwinService = new CachelessAdtService(
                adtApiServiceMock.Object,
                loggerMock.Object,
                adxHelperMock.Object,
                new MemoryCache(new MemoryCacheOptions()),
                configuration
            );
            await digitalTwinService.Load(Fixture.Create<SiteAdtSettings>(), new MemoryCache(new MemoryCacheOptions()));

            var expectedQuery = @$"select TargetTwin from DIGITALTWINS match (SourceTwin)-[:locatedIn|isPartOf*..5]->(TargetTwin) where SourceTwin.$dtId = '{twinId}' AND (IS_OF_MODEL(TargetTwin, 'dtmi:com:willowinc:Level;1') OR IS_OF_MODEL(TargetTwin, 'dtmi:com:willowinc:Building;1')) ";
            var expectedRelationship = RelationshipDto.MapFrom(Relationship.MapFrom(levelRelationships.First()));
            expectedRelationship.Target = TwinDto.MapFrom(Twin.MapFrom(System.Text.Json.JsonSerializer.Deserialize<BasicDigitalTwin>(expectedTwins[1].Contents.First().Value.ToString())));

            var result = await digitalTwinService.GetTwinRelationshipsByQuery(twinId, new string[] { "locatedIn", "isPartOf" }, new string[] { "dtmi:com:willowinc:Level;1", "dtmi:com:willowinc:Building;1" }, 5, null, null);

            adtApiServiceMock.Verify(x => x.QueryTwins<BasicDigitalTwin>(It.IsAny<AzureDigitalTwinsSettings>(), expectedQuery));

            result.Count.Should().Be(1);
            RelationshipDto.MapFrom(result.Single()).Should().BeEquivalentTo(expectedRelationship, config => config.Excluding(r => r.Target.Etag).Excluding(r => r.Source));
        }

        private List<BasicDigitalTwin> CreateTargetTwins()
        {
            var expectedTwins = Fixture.Build<BasicDigitalTwin>()
                                       .Without(t => t.Contents)
                                       .CreateMany(2).ToList();
            expectedTwins[0].Contents = new Dictionary<string, object>() {
                    { "TargetTwin",
                      $@"{{""$dtId"":""{LevelTwinId}"",""uniqueID"":""8483a1f1 - 3c6a - 4d64 - 9ddc - 407848aa624a"",""code"":""L10"",""name"":""Level 10"",""siteID"":""404bd33c - a697 - 4027 - b6a6 - 677e30a53d07"",""area"":{{""$metadata"":{{}}}},""capacity"":{{""$metadata"":{{}}}},""occupancy"":{{""$metadata"":{{}}}},""temperature"":{{""$metadata"":{{}}}},""humidity"":{{""$metadata"":{{}}}},""CO2"":{{""$metadata"":{{}}}},""$metadata"":{{""$model"":""dtmi:com:willowinc:Level;1"",""uniqueID"":{{""lastUpdateTime"":""2022-03-07T02:33:26.8440140Z""}},""code"":{{""lastUpdateTime"":""2022-03-07T02:33:26.8440140Z""}},""name"":{{""lastUpdateTime"":""2022-03-07T02:33:26.8440140Z""}},""siteID"":{{""lastUpdateTime"":""2022-03-07T02:33:26.8440140Z""}}}}}}"
                    }};
            expectedTwins[1].Contents = new Dictionary<string, object>() {
                    { "TargetTwin",
                      $@"{{""$dtId"":""{SiteTwinId}"",""$etag"":""W/\u0022c105be82-7918-492f-ac56-d325377f22c1\u0022"",""type"":""Commercial Retail"",""coordinates"":{{""latitude"":-33.86742,""longitude"":151.21178}},""constructionCompletionDate"":""2019-01-01"",""uniqueID"":""404bd33c-a697-4027-b6a6-677e30a53d07"",""siteID"":""404bd33c-a697-4027-b6a6-677e30a53d07"",""address"":{{""city"":""Sydney"",""region"":""New South Wales"",""country"":""Australia"",""postalCode"":""2000"",""$metadata"":{{""city"":{{""lastUpdateTime"":""2022-03-07T02:44:00.3401684Z""}},""region"":{{""lastUpdateTime"":""2022-03-07T02:44:00.3401684Z""}},""country"":{{""lastUpdateTime"":""2022-03-07T02:44:00.3401684Z""}},""postalCode"":{{""lastUpdateTime"":""2022-03-07T02:44:00.3401684Z""}}}}}},""area"":{{""grossArea"":48250,""rentableArea"":45823,""$metadata"":{{""grossArea"":{{""lastUpdateTime"":""2022-03-07T02:44:00.3401684Z""}},""rentableArea"":{{""lastUpdateTime"":""2022-03-07T02:44:00.3401684Z""}}}}}},""timeZone"":{{""$metadata"":{{}}}},""capacity"":{{""$metadata"":{{}}}},""occupancy"":{{""$metadata"":{{}}}},""temperature"":{{""$metadata"":{{}}}},""humidity"":{{""$metadata"":{{}}}},""CO2"":{{""$metadata"":{{}}}},""$metadata"":{{""$model"":""dtmi:com:willowinc:Building;1"",""type"":{{""lastUpdateTime"":""2022-03-07T02:44:00.3401684Z""}},""coordinates"":{{""lastUpdateTime"":""2022-03-07T02:44:00.3401684Z""}},""constructionCompletionDate"":{{""lastUpdateTime"":""2022-03-07T02:44:00.3401684Z""}},""uniqueID"":{{""lastUpdateTime"":""2022-03-07T02:44:00.3401684Z""}},""siteID"":{{""lastUpdateTime"":""2022-03-07T02:44:00.3401684Z""}}}}}}"
                    }};
            return expectedTwins;
        }

        private List<BasicRelationship> CreateRelationships(string twinId, string relationshipName = null, string targetTwinId = null, int numOfRelationships = 1)
        {
            var relationships = Fixture.Build<BasicRelationship>()
                                       .With(r => r.SourceId, twinId)
                                       .CreateMany(numOfRelationships).ToList();

            if (!string.IsNullOrEmpty(relationshipName))
                relationships.ForEach(r => r.Name = relationshipName);

            if (!string.IsNullOrEmpty(targetTwinId))
                relationships.ForEach(r => r.TargetId = targetTwinId);

            return relationships;
        }
    }
}
