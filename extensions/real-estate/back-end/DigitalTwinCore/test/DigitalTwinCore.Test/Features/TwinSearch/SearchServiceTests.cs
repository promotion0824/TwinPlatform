using AutoFixture;
using AutoFixture.Xunit2;
using DigitalTwinCore.Features.TwinsSearch.Dtos;
using DigitalTwinCore.Features.TwinsSearch.Models;
using DigitalTwinCore.Features.TwinsSearch.Services;
using DigitalTwinCore.Services;
using DigitalTwinCore.Services.AdtApi;
using DigitalTwinCore.Services.Adx;
using DTDLParser;
using DTDLParser.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace DigitalTwinCore.Test.Features.TwinSearch
{
    public class SearchServiceTests
    {
        // DTInterfaceInfo class is internal. We need to find a way to solve this limitation with the new version.
//         [Theory]
//         [AutoData]
//         public async Task SearchFirstPage(
//             SearchRequest request,
//             SiteAdtSettings[] siteSettings,
//             SearchTwin[] twins,
//             Dtmi categoryDtmi,
//             string categoryId)
//         {
//             var descendants = new Dictionary<string, DTInterfaceInfo>();
//
//             var categoryDtInterfaceInfo = new DTInterfaceInfo(categoryDtmi, null, null);
//
//             descendants.Add(categoryId, categoryDtInterfaceInfo);
//
//             request.QueryId = null;
//             request.CategoryId = categoryDtInterfaceInfo.GetUniqueId();
//
//             var relationships = twins.SelectMany(x => x.OutRelationships).ToArray();
//             var twinsData = twins.Select(x => new
//             {
//                 x.Id,
//                 x.SiteId,
//                 x.UniqueId,
//                 x.ModelId,
//                 x.ExternalId,
//                 x.Name
//             }).ToArray();
//
//             var adxHelper = new Mock<IAdxHelper>();
//             var siteAdtSettingsProvider = new Mock<ISiteAdtSettingsProvider>();
//             var digitalTwinServiceProvider = new Mock<IDigitalTwinServiceProvider>();
//             var digitalTwinService = new Mock<IDigitalTwinService>();
//             var digitalTwinModelParser = new Mock<IDigitalTwinModelParser>();
//             var logger = new Mock<ILogger<SearchService>>();
//             var twinsReader = Helpers.CreateDataReader(twinsData);
//             var countReader = Helpers.CreateDataReader(new[] { new SearchTwinCount { Count = request.PageSize + 1 } });
//             var relationshipsReader = Helpers.CreateDataReader(relationships);
//
//             digitalTwinModelParser.Setup(x => x.GetInterfaceDescendants(It.IsAny<string[]>())).Returns(descendants);
//
//             digitalTwinService.Setup(x => x.GetModelParserAsync()).ReturnsAsync(digitalTwinModelParser.Object);
//             digitalTwinService.SetupGet(x => x.SiteAdtSettings).Returns(siteSettings[0]);
//
//             digitalTwinServiceProvider.Setup(x => x.GetForSiteAsync(It.IsAny<Guid>()))
//                 .ReturnsAsync(digitalTwinService.Object);
//
//             var setupSequence = siteAdtSettingsProvider.SetupSequence(x => x.GetForSiteAsync(It.IsAny<Guid>()));
//             foreach (var siteSetting in siteSettings)
//             {
//                 setupSequence.ReturnsAsync(siteSetting);
//             }
//
//             adxHelper.SetupSequence(x => x.Query(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
//                 .ReturnsAsync(twinsReader.Object).ReturnsAsync(countReader.Object).ReturnsAsync(relationshipsReader.Object)
//                 .ReturnsAsync(relationshipsReader.Object).ReturnsAsync(twinsReader.Object);
//
//             var sut = new SearchService(adxHelper.Object, siteAdtSettingsProvider.Object, digitalTwinServiceProvider.Object, logger.Object);
//             var response = await sut.Search(request, new CancellationToken());
//
//             var expectedQuery = @$".set stored_query_result {response.QueryId} with (previewCount = {request.PageSize}) <| union database('{siteSettings[0].AdxDatabase}').ActiveTwins, database('{siteSettings[1].AdxDatabase}').ActiveTwins, database('{siteSettings[2].AdxDatabase}').ActiveTwins
// | where SiteId in ('{siteSettings[0].SiteId}','{siteSettings[1].SiteId}','{siteSettings[2].SiteId}')
// | where Name contains '{request.Term}'
// {BuildFileTypesQuery(request.FileTypes)}
// | where (SiteId == '{siteSettings[0].SiteId}' and ModelId in ('{categoryId}')) or (SiteId == '{siteSettings[1].SiteId}' and ModelId in ('{categoryId}')) or (SiteId == '{siteSettings[2].SiteId}' and ModelId in ('{categoryId}'))
// | order by Name asc
// | project Num=row_number(), Id, Name, SiteId, FloorId, ModelId, UniqueId, ExternalId, Raw
// ";
//
//             adxHelper.Verify(x => x.Query(It.IsAny<string>(), expectedQuery, It.IsAny<CancellationToken>()));
//
//             response.NextPage.Should().Be(1);
//             response.Twins.Should().HaveSameCount(twins);
//         }

        [Theory]
        [AutoData]
        public async Task SearchFirstPage_With_No_Category(SearchRequest request, SiteAdtSettings[] siteSettings, SearchTwin[] twins)
        {
            request.QueryId = null;
            request.CategoryId = null;
            request.ModelId = null;
            request.IsCapabilityOfModelId = null;

            var relationships = twins.SelectMany(x => x.OutRelationships.Select(x => new
            {
                x.SourceId,
                x.TargetId,
                x.Name,
                x.ModelId,
                x.TwinName
            })).ToArray();
            var twinsData = twins.Select(x => new
            {
                x.Id,
                x.SiteId,
                x.UniqueId,
                x.ModelId,
                x.ExternalId,
                x.Name
            }).ToArray();

            var adxHelper = new Mock<IAdxHelper>();
            var siteAdtSettingsProvider = new Mock<ISiteAdtSettingsProvider>();
            var digitalTwinServiceProvider = new Mock<IDigitalTwinServiceProvider>();
            var digitalTwinService = new Mock<IDigitalTwinService>();
            var digitalTwinModelParser = new Mock<IDigitalTwinModelParser>();
            var logger = new Mock<ILogger<SearchService>>();
            var dataReader = Helpers.CreateDataReader(twinsData);
            var dataCountReader = Helpers.CreateDataReader(new[] { new SearchTwinCount { Count = (request.PageSize + 1) } });
            var relationshipsReader = Helpers.CreateDataReader(relationships);
            var descendants = new Dictionary<string, DTInterfaceInfo>();

            digitalTwinModelParser.Setup(x => x.GetInterfaceDescendants(It.IsAny<string[]>())).Returns(descendants);

            digitalTwinService.Setup(x => x.GetModelParserAsync()).ReturnsAsync(digitalTwinModelParser.Object);
            digitalTwinService.SetupGet(x => x.SiteAdtSettings).Returns(siteSettings[0]);

            digitalTwinServiceProvider.Setup(x => x.GetForSiteAsync(It.IsAny<Guid>()))
                .ReturnsAsync(digitalTwinService.Object);

			var setupSequence = siteAdtSettingsProvider.SetupSequence(x => x.GetForSitesAsync(It.IsAny<Guid[]>()));
			setupSequence.ReturnsAsync(siteSettings.ToList());

			adxHelper.SetupSequence(x => x.Query(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(dataReader.Object).ReturnsAsync(dataCountReader.Object).ReturnsAsync(relationshipsReader.Object)
                .ReturnsAsync(relationshipsReader.Object).ReturnsAsync(dataReader.Object);

            var sut = new SearchService(adxHelper.Object, siteAdtSettingsProvider.Object, digitalTwinServiceProvider.Object, logger.Object);

            var response = await sut.Search(request, new CancellationToken());

            var expectedQuery = @$".set stored_query_result {response.QueryId} with (previewCount = {request.PageSize}) <| union database('{siteSettings[0].AdxDatabase}').ActiveTwins, database('{siteSettings[1].AdxDatabase}').ActiveTwins, database('{siteSettings[2].AdxDatabase}').ActiveTwins
| where SiteId in ('{siteSettings[0].SiteId}','{siteSettings[1].SiteId}','{siteSettings[2].SiteId}')
| where Name contains '{request.Term}'
{BuildFileTypesQuery(request.FileTypes)}
| order by Name asc
| project Num=row_number(), Id, Name, SiteId, FloorId, ModelId, UniqueId, ExternalId, Raw
";

            adxHelper.Verify(x => x.Query(It.IsAny<string>(), expectedQuery, It.IsAny<CancellationToken>()));

            response.NextPage.Should().Be(1);
            response.Twins.Should().HaveSameCount(twinsData);
        }

        // DTInterfaceInfo class is internal. We need to find a way to solve this limitation with the new version.
//         [Theory]
//         [AutoData]
//         public async Task SearchFirstPage_With_No_Term(SearchRequest request, SiteAdtSettings[] siteSettings, SearchTwin[] twins, Dtmi categoryDtmi, string categoryId)
//         {
//             var descendants = new Dictionary<string, DTInterfaceInfo>();
//             
//             var categoryDtInterfaceInfo = new DTInterfaceInfo(categoryDtmi, null, null);
//
//             descendants.Add(categoryId, categoryDtInterfaceInfo);
//
//             request.QueryId = null;
//             request.Term = null;
//             request.FileTypes = new string[] { };
//             request.CategoryId = categoryDtInterfaceInfo.GetUniqueId();
//             
//             var relationships = twins.SelectMany(x => x.OutRelationships).ToArray();
//             var twinsData = twins.Select(x => new
//             {
//                 x.Id,
//                 x.SiteId,
//                 x.UniqueId,
//                 x.ModelId,
//                 x.ExternalId,
//                 x.Name
//             }).ToArray();
//
//             var adxHelper = new Mock<IAdxHelper>();
//             var siteAdtSettingsProvider = new Mock<ISiteAdtSettingsProvider>();
//             var digitalTwinServiceProvider = new Mock<IDigitalTwinServiceProvider>();
//             var digitalTwinService = new Mock<IDigitalTwinService>();
//             var digitalTwinModelParser = new Mock<IDigitalTwinModelParser>();
//             var logger = new Mock<ILogger<SearchService>>();
//             var dataReader = Helpers.CreateDataReader(twinsData);
//             var dataCountReader = Helpers.CreateDataReader(new[] { new SearchTwinCount { Count = (request.PageSize + 1) } });
//             var relationshipsReader = Helpers.CreateDataReader(relationships);
//             digitalTwinModelParser.Setup(x => x.GetInterfaceDescendants(It.IsAny<string[]>())).Returns(descendants);
//
//             digitalTwinService.Setup(x => x.GetModelParserAsync()).ReturnsAsync(digitalTwinModelParser.Object);
//             digitalTwinService.SetupGet(x => x.SiteAdtSettings).Returns(siteSettings[0]);
//
//             digitalTwinServiceProvider.Setup(x => x.GetForSiteAsync(It.IsAny<Guid>()))
//                 .ReturnsAsync(digitalTwinService.Object);
//
//             var setupSequence = siteAdtSettingsProvider.SetupSequence(x => x.GetForSiteAsync(It.IsAny<Guid>()));
//             foreach (var siteSetting in siteSettings)
//             {
//                 setupSequence.ReturnsAsync(siteSetting);
//             }
//
//             adxHelper.SetupSequence(x => x.Query(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
//                 .ReturnsAsync(dataReader.Object).ReturnsAsync(dataCountReader.Object).ReturnsAsync(relationshipsReader.Object)
//                 .ReturnsAsync(relationshipsReader.Object).ReturnsAsync(dataReader.Object);
//
//             var sut = new SearchService(adxHelper.Object, siteAdtSettingsProvider.Object, digitalTwinServiceProvider.Object, logger.Object);
//
//             var response = await sut.Search(request, new CancellationToken());
//
//             var expectedQuery = @$".set stored_query_result {response.QueryId} with (previewCount = {request.PageSize}) <| union database('{siteSettings[0].AdxDatabase}').ActiveTwins, database('{siteSettings[1].AdxDatabase}').ActiveTwins, database('{siteSettings[2].AdxDatabase}').ActiveTwins
// | where SiteId in ('{siteSettings[0].SiteId}','{siteSettings[1].SiteId}','{siteSettings[2].SiteId}')
// | where (SiteId == '{siteSettings[0].SiteId}' and ModelId in ('{categoryId}')) or (SiteId == '{siteSettings[1].SiteId}' and ModelId in ('{categoryId}')) or (SiteId == '{siteSettings[2].SiteId}' and ModelId in ('{categoryId}'))
// | order by Name asc
// | project Num=row_number(), Id, Name, SiteId, FloorId, ModelId, UniqueId, ExternalId, Raw
// ";
//
//             adxHelper.Verify(x => x.Query(It.IsAny<string>(), expectedQuery, It.IsAny<CancellationToken>()));
//
//             response.NextPage.Should().Be(1);
//             response.Twins.Should().HaveSameCount(twinsData);
//         }

        [Theory]
        [AutoData]
        public async Task SearchFirstPage_With_Only_ModelId(SearchRequest request, SiteAdtSettings[] siteSettings, SearchTwin[] twins, Dtmi categoryDtmi, string modelId)
        {
            var descendants = await GetAssetModelInfo(modelId);

            request.QueryId = null;
            request.Term = null;
            request.FileTypes = new string[] { };
            request.CategoryId = null;
            request.ModelId = modelId;
            request.IsCapabilityOfModelId = null;

            var relationships = twins.SelectMany(x => x.OutRelationships.Select(x => new
            {
                x.SourceId,
                x.TargetId,
                x.Name,
                x.ModelId,
                x.TwinName
            })).ToArray();
            var twinsData = twins.Select(x => new
            {
                x.Id,
                x.SiteId,
                x.UniqueId,
                x.ModelId,
                x.ExternalId,
                x.Name
            }).ToArray();

            var adxHelper = new Mock<IAdxHelper>();
            var siteAdtSettingsProvider = new Mock<ISiteAdtSettingsProvider>();
            var digitalTwinServiceProvider = new Mock<IDigitalTwinServiceProvider>();
            var digitalTwinService = new Mock<IDigitalTwinService>();
            var digitalTwinModelParser = new Mock<IDigitalTwinModelParser>();
            var logger = new Mock<ILogger<SearchService>>();
            var dataReader = Helpers.CreateDataReader(twinsData);
            var dataCountReader = Helpers.CreateDataReader(new[] { new SearchTwinCount { Count = (request.PageSize + 2) } });
            var relationshipsReader = Helpers.CreateDataReader(relationships);
            digitalTwinModelParser.Setup(x => x.GetInterfaceDescendants(It.IsAny<string[]>())).Returns(descendants);

            digitalTwinService.Setup(x => x.GetModelParserAsync()).ReturnsAsync(digitalTwinModelParser.Object);
            digitalTwinService.SetupGet(x => x.SiteAdtSettings).Returns(siteSettings[0]);

            digitalTwinServiceProvider.Setup(x => x.GetForSiteAsync(It.IsAny<Guid>()))
                .ReturnsAsync(digitalTwinService.Object);

			var setupSequence = siteAdtSettingsProvider.SetupSequence(x => x.GetForSitesAsync(It.IsAny<Guid[]>()));
			setupSequence.ReturnsAsync(siteSettings.ToList());

			adxHelper.SetupSequence(x => x.Query(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(dataReader.Object).ReturnsAsync(dataCountReader.Object).ReturnsAsync(relationshipsReader.Object)
                .ReturnsAsync(relationshipsReader.Object).ReturnsAsync(dataReader.Object);

            var sut = new SearchService(adxHelper.Object, siteAdtSettingsProvider.Object, digitalTwinServiceProvider.Object, logger.Object);

            var response = await sut.Search(request, new CancellationToken());

            var expectedModelId = descendants.Keys.First();
            var expectedQuery = @$".set stored_query_result {response.QueryId} with (previewCount = {request.PageSize}) <| union database('{siteSettings[0].AdxDatabase}').ActiveTwins, database('{siteSettings[1].AdxDatabase}').ActiveTwins, database('{siteSettings[2].AdxDatabase}').ActiveTwins
| where SiteId in ('{siteSettings[0].SiteId}','{siteSettings[1].SiteId}','{siteSettings[2].SiteId}')
| where ModelId in ('{expectedModelId}')
| order by Name asc
| project Num=row_number(), Id, Name, SiteId, FloorId, ModelId, UniqueId, ExternalId, Raw
";

            adxHelper.Verify(x => x.Query(It.IsAny<string>(), expectedQuery, It.IsAny<CancellationToken>()));

            response.NextPage.Should().Be(1);
            response.Twins.Should().HaveSameCount(twinsData);
        }

        [Theory]
        [AutoData]
        public async Task SearchFirstPage_Empty(SearchRequest request, SiteAdtSettings[] siteSettings, SearchTwin[] twins)
        {
            request.QueryId = null;
            request.FileTypes = new string[] { };
            request.Term = null;
            request.CategoryId = null;
            request.ModelId = null;
            request.IsCapabilityOfModelId = null;

            var relationships = twins.SelectMany(x => x.OutRelationships.Select(x => new
            {
                x.SourceId,
                x.TargetId,
                x.Name,
                x.ModelId,
                x.TwinName
            })).ToArray();
            var twinsData = twins.Select(x => new
            {
                x.Id,
                x.SiteId,
                x.UniqueId,
                x.ModelId,
                x.ExternalId,
                x.Name
            }).ToArray();

            var adxHelper = new Mock<IAdxHelper>();
            var siteAdtSettingsProvider = new Mock<ISiteAdtSettingsProvider>();
            var digitalTwinServiceProvider = new Mock<IDigitalTwinServiceProvider>();
            var digitalTwinService = new Mock<IDigitalTwinService>();
            var digitalTwinModelParser = new Mock<IDigitalTwinModelParser>();
            var logger = new Mock<ILogger<SearchService>>();
            var dataReader = Helpers.CreateDataReader(twinsData);
            var dataCountReader = Helpers.CreateDataReader(new[] { new SearchTwinCount { Count = (request.PageSize + 1) } });
            var relationshipsReader = Helpers.CreateDataReader(relationships);
            var descendants = new Dictionary<string, DTInterfaceInfo>();

            digitalTwinModelParser.Setup(x => x.GetInterfaceDescendants(It.IsAny<string[]>())).Returns(descendants);

            digitalTwinService.Setup(x => x.GetModelParserAsync()).ReturnsAsync(digitalTwinModelParser.Object);
            digitalTwinService.SetupGet(x => x.SiteAdtSettings).Returns(siteSettings[0]);

            digitalTwinServiceProvider.Setup(x => x.GetForSiteAsync(It.IsAny<Guid>()))
                .ReturnsAsync(digitalTwinService.Object);

			var setupSequence = siteAdtSettingsProvider.SetupSequence(x => x.GetForSitesAsync(It.IsAny<Guid[]>()));
			setupSequence.ReturnsAsync(siteSettings.ToList());

			adxHelper.SetupSequence(x => x.Query(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(dataReader.Object).ReturnsAsync(dataCountReader.Object).ReturnsAsync(relationshipsReader.Object)
                .ReturnsAsync(relationshipsReader.Object).ReturnsAsync(dataReader.Object);

            var sut = new SearchService(adxHelper.Object, siteAdtSettingsProvider.Object, digitalTwinServiceProvider.Object, logger.Object);

            var response = await sut.Search(request, new CancellationToken());

            var expectedQuery = @$".set stored_query_result {response.QueryId} with (previewCount = {request.PageSize}) <| union database('{siteSettings[0].AdxDatabase}').ActiveTwins, database('{siteSettings[1].AdxDatabase}').ActiveTwins, database('{siteSettings[2].AdxDatabase}').ActiveTwins
| where SiteId in ('{siteSettings[0].SiteId}','{siteSettings[1].SiteId}','{siteSettings[2].SiteId}')
| order by Name asc
| project Num=row_number(), Id, Name, SiteId, FloorId, ModelId, UniqueId, ExternalId, Raw
";

            adxHelper.Verify(x => x.Query(It.IsAny<string>(), expectedQuery, It.IsAny<CancellationToken>()));

            response.NextPage.Should().Be(1);
            response.Twins.Should().HaveSameCount(twinsData);
        }

        [Theory]
        [AutoData]
        public async Task SearchNextPage(SearchRequest request, SiteAdtSettings[] siteSettings)
        {
            var adxHelper = new Mock<IAdxHelper>();
            var siteAdtSettingsProvider = new Mock<ISiteAdtSettingsProvider>();
            var digitalTwinServiceProvider = new Mock<IDigitalTwinServiceProvider>();
            var logger = new Mock<ILogger<SearchService>>();
            var twins = new Fixture().CreateMany<SearchTwin>(request.PageSize).ToArray();
            var relationships = twins.SelectMany(x => x.OutRelationships.Select(x => new
            {
                x.SourceId,
                x.TargetId,
                x.Name,
                x.ModelId,
                x.TwinName
            })).ToArray();
            var twinsData = twins.Select(x => new
            {
                x.Id,
                x.SiteId,
                x.UniqueId,
                x.ModelId,
                x.ExternalId,
                x.Name
            }).ToArray();
            var dataReader = Helpers.CreateDataReader(twinsData);
            var nextPage = request.Page + 1;
            var dataCountReader = Helpers.CreateDataReader(new[] { new SearchTwinCount { Count = ((request.PageSize * nextPage) + 1) } });
            var relationshipsReader = Helpers.CreateDataReader(relationships);
			var setupSequence = siteAdtSettingsProvider.SetupSequence(x => x.GetForSitesAsync(It.IsAny<Guid[]>()));
			setupSequence.ReturnsAsync(siteSettings.ToList());

			adxHelper.SetupSequence(x => x.Query(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(dataReader.Object).ReturnsAsync(dataCountReader.Object).ReturnsAsync(relationshipsReader.Object)
                .ReturnsAsync(relationshipsReader.Object).ReturnsAsync(dataReader.Object);

            var sut = new SearchService(adxHelper.Object, siteAdtSettingsProvider.Object, digitalTwinServiceProvider.Object, logger.Object);

            var response = await sut.Search(request, new CancellationToken());

            var expectedQuery = @$"stored_query_result(""{request.QueryId}"")
| where SiteId in ('{siteSettings[0].SiteId}','{siteSettings[1].SiteId}','{siteSettings[2].SiteId}')
| where Num between({request.Page * request.PageSize + 1} .. {(request.Page + 1) * request.PageSize})
";

            adxHelper.Verify(x => x.Query(It.IsAny<string>(), expectedQuery, It.IsAny<CancellationToken>()));

            response.NextPage.Should().Be(nextPage);
            response.Twins.Should().HaveSameCount(twinsData);
        }

        [Theory]
        [AutoData]
        public async Task BulkQuery_No_SiteTwinPairs(BulkQueryRequest request, SiteAdtSettings[] siteSettings, SearchTwin[] twins)
        {
            request.Twins = new SiteTwinPair[0];

            var adxHelper = new Mock<IAdxHelper>();
            var siteAdtSettingsProvider = new Mock<ISiteAdtSettingsProvider>();
            var digitalTwinServiceProvider = new Mock<IDigitalTwinServiceProvider>();
            var logger = new Mock<ILogger<SearchService>>();
            var relationships = twins.SelectMany(x => x.OutRelationships).ToArray();
            var twinsData = twins.Select(x => new
            {
                x.Id,
                x.SiteId,
                x.UniqueId,
                x.ModelId,
                x.ExternalId,
                x.Name
            }).ToArray();
            var dataReader = Helpers.CreateDataReader(twinsData);
            var relationshipsReader = Helpers.CreateDataReader(relationships);
			var setupSequence = siteAdtSettingsProvider.SetupSequence(x => x.GetForSitesAsync(It.IsAny<Guid[]>()));
			setupSequence.ReturnsAsync(siteSettings.ToList());

            adxHelper
                .SetupSequence(x => 
                    x.Query(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()
                   )
                )
                .ReturnsAsync(dataReader.Object)
                .ReturnsAsync(relationshipsReader.Object)
                .ReturnsAsync(relationshipsReader.Object);

            var sut = new SearchService(
                adxHelper.Object,
                siteAdtSettingsProvider.Object,
                digitalTwinServiceProvider.Object,
                logger.Object
            );

            var response = await sut.BulkQuery(request, new CancellationToken());

            var expectedQuery = @$"stored_query_result(""{request.QueryId}"")
| where SiteId in ('{siteSettings[0].SiteId}','{siteSettings[1].SiteId}','{siteSettings[2].SiteId}')
";

            adxHelper.Verify(x => x.Query(It.IsAny<string>(), expectedQuery, It.IsAny<CancellationToken>()));
            response.Should().HaveSameCount(twinsData);
        }

        [Theory]
        [AutoData]
        public async Task BulkQuery_With_SiteTwinPairs(BulkQueryRequest request, SiteAdtSettings[] siteSettings, SearchTwin[] twins)
        {
            var adxHelper = new Mock<IAdxHelper>();
            var siteAdtSettingsProvider = new Mock<ISiteAdtSettingsProvider>();
            var digitalTwinServiceProvider = new Mock<IDigitalTwinServiceProvider>();
            var logger = new Mock<ILogger<SearchService>>();
            var relationships = twins.SelectMany(x => x.OutRelationships).ToArray();
            var twinsData = twins.Select(x => new
            {
                x.Id,
                x.SiteId,
                x.UniqueId,
                x.ModelId,
                x.ExternalId,
                x.Name
            }).ToArray();
            var dataReader = Helpers.CreateDataReader(twinsData);
            var relationshipsReader = Helpers.CreateDataReader(relationships);
			var setupSequence = siteAdtSettingsProvider.SetupSequence(x => x.GetForSitesAsync(It.IsAny<Guid[]>()));
			setupSequence.ReturnsAsync(siteSettings.ToList());

			adxHelper
                .SetupSequence(x =>
                    x.Query(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()
                   )
                )
                .ReturnsAsync(dataReader.Object)
                .ReturnsAsync(relationshipsReader.Object)
                .ReturnsAsync(relationshipsReader.Object);

            var sut = new SearchService(
                adxHelper.Object,
                siteAdtSettingsProvider.Object,
                digitalTwinServiceProvider.Object,
                logger.Object
            );

            var response = await sut.BulkQuery(request, new CancellationToken());

            var expectedQuery = @$"stored_query_result(""{request.QueryId}"")
| where SiteId in ('{siteSettings[0].SiteId}','{siteSettings[1].SiteId}','{siteSettings[2].SiteId}')
| where
(SiteId == '{request.Twins[0].SiteId}' and Id in ('{request.Twins[0].TwinId}')) or (SiteId == '{request.Twins[1].SiteId}' and Id in ('{request.Twins[1].TwinId}')) or (SiteId == '{request.Twins[2].SiteId}' and Id in ('{request.Twins[2].TwinId}'))
";

            adxHelper.Verify(x => x.Query(It.IsAny<string>(), expectedQuery, It.IsAny<CancellationToken>()));
            response.Should().HaveSameCount(twinsData);
        }

        private async Task<Dictionary<string, DTInterfaceInfo>> GetAssetModelInfo(string displayName)
        {
            var modelDefinition = @$"{{""@id"":""dtmi:com:willowinc:Asset;1"",""@type"":""Interface"",""displayName"":{{""en"":""{displayName}""}},""extends"":[],""contents"":[],""@context"":[""dtmi:dtdl:context;2""]}}";
            var modelInfos = await new ModelParser().ParseAsync((new string[] { modelDefinition }).ToAsyncEnumerable());

            return modelInfos.Values.OfType<DTInterfaceInfo>().ToDictionary(i => i.Id.AbsoluteUri);
        }

        private string BuildFileTypesQuery(string[] fileTypes)
        {
            string query = "";
            if (fileTypes.Any())
            {
                query = $"| where Name hassuffix '{fileTypes.First()}'";
                for (int i = 1; i < fileTypes.Length; i++)
                {
                    query += $" or Name hassuffix '{fileTypes[i]}'";
                }
            }

            return query;
        }
    }
}
