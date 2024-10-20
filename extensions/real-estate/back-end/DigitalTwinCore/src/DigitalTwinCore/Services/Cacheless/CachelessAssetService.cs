using Azure.DigitalTwins.Core;
using DigitalTwinCore.Constants;
using DigitalTwinCore.Dto;
using DigitalTwinCore.DTO;
using DigitalTwinCore.DTO.Adx;
using DigitalTwinCore.Exceptions;
using DigitalTwinCore.Extensions;
using DigitalTwinCore.Features.DirectoryCore;
using DigitalTwinCore.Features.DirectoryCore.Dtos;
using DigitalTwinCore.Infrastructure.Extensions;
using DigitalTwinCore.Models;
using DigitalTwinCore.Models.Connectors;
using DigitalTwinCore.Serialization;
using DigitalTwinCore.Services.AdtApi;
using DigitalTwinCore.Services.Adx;
using DigitalTwinCore.Services.Query;
using DTDLParser;
using DTDLParser.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Willow.Common;
using Willow.Infrastructure.Exceptions;

namespace DigitalTwinCore.Services.Cacheless
{
    public class CachelessAssetService : IAssetService
    {
        private readonly IDigitalTwinServiceProvider _digitalTwinServiceFactory;
        private readonly IAdxHelper _adxHelper;
        private readonly ITagMapperService _tagMapperService;
        private readonly ILogger<CachelessAssetService> _logger;
        private readonly IAdtApiService _adtApiService;
        private readonly IMemoryCache _memoryCache;
        private readonly IDirectoryCoreClient _directoryCoreClient;
		private readonly ISiteAdtSettingsProvider _siteAdtSettingsProvider;

        private const string AssetResourceType = "Asset";
        private const string DeviceResourceType = "Device";

        private readonly string[] _assetDetailRelationships = new string[]
        {
            Relationships.HasDocument,
            Relationships.HasWarranty,
            Relationships.ManufacturedBy,
            Relationships.LocatedIn,
            Relationships.OwnedBy,
            Relationships.ServicedBy,
            Relationships.MaintenanceResponsibility,
            Relationships.InstalledBy
        };

		public CachelessAssetService(
			IDigitalTwinServiceProvider digitalTwinServiceFactory,
			IAdxHelper adxHelper,
			ITagMapperService tagMapperService,
			ILogger<CachelessAssetService> logger,
			IAdtApiService adtApiService,
			IMemoryCache memoryCache,
			IDirectoryCoreClient directoryCoreClient,
			ISiteAdtSettingsProvider siteAdtSettingsProvider)
		{
			_digitalTwinServiceFactory = digitalTwinServiceFactory;
			_adxHelper = adxHelper;
			_tagMapperService = tagMapperService;
			_logger = logger;
			_adtApiService = adtApiService;
			_memoryCache = memoryCache;
			_directoryCoreClient = directoryCoreClient;
			_siteAdtSettingsProvider = siteAdtSettingsProvider;
		}

		public ILogger Logger => _logger;

        #region Assets
        public async Task<IEnumerable<LightCategoryDto>> GetCategories(Guid siteId, Guid? floorId, bool isLiveDataOnly)
        {
            var digitalTwinService = await _digitalTwinServiceFactory.GetForSiteAsync(siteId);
            var modelParser = await digitalTwinService.GetModelParserAsync();

            var interfaceInfos = await GetAssetInterfaceInfos(digitalTwinService, modelParser);
            var assetIds = await GetModelIds(digitalTwinService, modelParser);
            var assetCount = await GetAdxAssetCount(siteId, assetIds, floorId, isLiveDataOnly);
            var categories = await MapToLightCategories(interfaceInfos, assetCount);

            return categories;
        }

        private async Task<IEnumerable<string>> GetModelIds(IDigitalTwinService digitalTwinService, IDigitalTwinModelParser modelParser)
        {
            var modelIds = digitalTwinService.SiteAdtSettings.AssetModelIds.ToList();
            modelIds.AddRange(digitalTwinService.SiteAdtSettings.BuildingComponentModelIds);
            modelIds.AddRange(digitalTwinService.SiteAdtSettings.SpaceModelIds);
            modelIds.AddRange(digitalTwinService.SiteAdtSettings.StructureModelIds);

            var models = modelParser.GetInterfaceDescendants(modelIds.ToArray());

            return models.Keys;
        }

        private async Task<IEnumerable<(string, long)>> GetAdxAssetCount(
            Guid siteId,
            IEnumerable<string> modelIds,
            Guid? floorId,
            bool isLiveDataOnly)
        {
            var qb = (AdxQueryBuilder.Create()
                .Select(AdxConstants.ActiveTwinsFunction)
                .Where()
                .Property("SiteId", siteId.ToString()) as IAdxQueryFilterGroup)
                .And()
                .PropertyIn("ModelId", modelIds);

            if (floorId != null)
            {
                qb.And().Property("FloorId", floorId.ToString());
            }

            if (isLiveDataOnly)
            {
                (((qb as IAdxQuerySelector).Join(
                    AdxQueryBuilder.Create()
                        .Select(AdxConstants.ActiveRelationshipsFunction)
                        .Where()
                        .PropertyIn("Name", new string[] { "isCapabilityOf", "hostedBy" })
                        .GetQuery(),
                    "Id",
                    "TargetId",
                    "leftouter")
                .Join(AdxConstants.ActiveTwinsFunction, "SourceId", "Id", "leftouter") as IAdxQueryWhere)
                .Where()
                .IsNotEmpty("Id2") as IAdxQuerySelector)
                .Project("Id", "Id2", "ModelId");
            }

            (qb as IAdxQuerySelector)
                .Summarize()
                .SetProperty("Count")
                .CountDistinct("Id")
                .By("ModelId");

            var digitalTwinService = await _digitalTwinServiceFactory.GetForSiteAsync(siteId);

            var assetCount = new List<(string, long)>();
            using var reader = await _adxHelper.Query(digitalTwinService, qb.GetQuery());
            while (reader.Read())
            {
                assetCount.Add((reader["ModelId"] as string, (long)reader["Count"]));
            }

            return assetCount;
        }

        private async Task<ICollection<LightCategoryDto>> MapToLightCategories(
            List<InterfaceInfo> interfaceInfos,
            IEnumerable<(string, long)> assetCount)
        {
            var output = new List<LightCategoryDto>();
            if (assetCount.Count() == 0)
            {
                return output;
            }

            foreach (var interfaceInfo in interfaceInfos)
            {
                var modelAssets = assetCount.SingleOrDefault(ac => ac.Item1 == interfaceInfo.Model.Id.AbsoluteUri);
                var count = (modelAssets == default) ? 0 : modelAssets.Item2;

                var childCategories = await MapToLightCategories(interfaceInfo.Children, assetCount); // Recursive.
                output.Add(new LightCategoryDto
                {
                    Id = interfaceInfo.Model.GetUniqueId(),
                    Name = interfaceInfo.DisplayName,
                    ModelId = interfaceInfo.Model.Id.AbsoluteUri,
                    ChildCategories = childCategories,
                    AssetCount = count,
                    HasChildren = childCategories.Count > 0
                });
            }

            return output.Where(o => o.ChildCategories.Any() || o.AssetCount > 0).ToList();
        }

        public async Task<List<Category>> GetCategoriesAndAssetsAsync(
            Guid siteId,
            bool isCategoryOnly,
            List<string> modelNames,
            Guid? floorId = null)
        {
            var assetCategories = await GetAdxAssetCategories(siteId, isCategoryOnly, modelNames, floorId);

            return assetCategories.ToList();
        }

        private async Task<IEnumerable<Category>> GetAdxAssetCategories(
            Guid siteId,
            bool isCategoryOnly,
            List<string> modelNames,
            Guid? floorId)
        {
            var digitalTwinService = await _digitalTwinServiceFactory.GetForSiteAsync(siteId);
            var modelParser = await digitalTwinService.GetModelParserAsync();

            var interfaceInfos = await GetAssetInterfaceInfos(digitalTwinService, modelParser, modelNames);

            var assets = await GetAdxAssetsByCategories(siteId, floorId, interfaceInfos.Select(ii => ii.Model.GetUniqueId()));

            var categories = await MapToCategories(interfaceInfos, assets.ToList(), isCategoryOnly);

            return categories;
        }

        private async Task<IEnumerable<Asset>> GetAdxAssetsByCategories(
            Guid siteId,
            Guid? floorId,
            IEnumerable<Guid> modelUniqueIds)
        {
            var models = await GetAssetModelIds(siteId, modelUniqueIds);

            var siteAssetsQb = (AdxQueryBuilder.Create()
                .Select(AdxConstants.ActiveTwinsFunction)
                .Where()
                .Property("SiteId", siteId.ToString()) as IAdxQueryFilterGroup)
                .And()
                .PropertyIn("ModelId", models);

            if (floorId != null)
            {
                siteAssetsQb.And().Property("FloorId", floorId.ToString());
            }

            var rawAssetsQb = (((((((((AdxQueryBuilder.Create() as IAdxQueryFilterGroup)
                .OpenGroupParentheses() as IAdxQuerySelector)
                .Select("siteassets") as IAdxQuerySelector)
                .Join(
                    AdxConstants.ActiveRelationshipsFunction,
                    "Id",
                    "SourceId",
                    "leftouter")
                .Join(AdxConstants.ActiveTwinsFunction, "TargetId", "Id", "leftouter")
                .Project(
                    "Id",
                    "FloorId",
                    "Raw",
                    "RelValue = pack(\"Rel\", Raw1, \"Target\", Raw2)") as IAdxQueryFilterGroup)
                .CloseGroupParentheses() as IAdxQuerySelector)
                .Union() as IAdxQueryFilterGroup)
                .OpenGroupParentheses() as IAdxQuerySelector)
                .Select("siteassets") as IAdxQuerySelector)
                .Join(
                    AdxQueryBuilder.Create()
                        .Select(AdxConstants.ActiveRelationshipsFunction)
                        .Where()
                        .PropertyIn("Name", new string[] { "isCapabilityOf", "hostedBy" })
                        .GetQuery(),
                    "Id",
                    "TargetId",
                    "leftouter")
                .Join(
                    AdxConstants.ActiveTwinsFunction,
                    "SourceId",
                    "Id",
                    "leftouter")
                .Project("Id", "FloorId", "Raw", "PtsValue = Raw2") as IAdxQueryFilterGroup)
                .CloseGroupParentheses();

            var query = ((AdxQueryBuilder.Create()
                .Let("siteassets", siteAssetsQb.GetQuery())
                .Let("rawassets", rawAssetsQb.GetQuery())
                .Select("rawassets") as IAdxQuerySelector)
                .Summarize()
                .SetProperty("Raw").TakeAny(true, "Raw")
                .SetProperty("RelData").MakeSet(true, "RelValue")
                .SetProperty("PtsData").MakeSet(from: "PtsValue")
                .By("Id", "FloorId") as IAdxQuerySelector)
                .Summarize()
                .SetProperty("Raw").TakeAny(true, "Raw")
                .SetProperty("Details").MakeBag(from: "pack(\"Relationships\", RelData, \"Points\", PtsData)")
                .By("Id", "FloorId")
                .GetQuery();

            var adxAssetResults = new List<(Twin, Guid?, IEnumerable<(BasicRelationship, Twin)>, IEnumerable<Twin>, Dictionary<string, IEnumerable<Twin>>)>();
            var adxReadTasks = new List<Task>();

            var digitalTwinService = await _digitalTwinServiceFactory.GetForSiteAsync(siteId);

            using var reader = await _adxHelper.Query(digitalTwinService, query);
            while (reader.Read())
            {
                adxReadTasks.Add(ReadAssetResults(adxAssetResults, reader));
            }

            await Task.WhenAll(adxReadTasks);

            return await MapAdxAssets(siteId, adxAssetResults);
        }

        private static Task ReadAssetResults(List<(Twin, Guid?, IEnumerable<(BasicRelationship, Twin)>, IEnumerable<Twin>, Dictionary<string, IEnumerable<Twin>>)> adxAssetResults, IDataReader reader)
        {
            var twin = JsonConvert.DeserializeObject<Twin>(reader["Raw"].ToString(), new TwinJsonConverter());

            var packedDetails = (reader["Details"] as JObject).ToObject<AdxDetailedRelationshipsPacked>();
            var unpackedRelationships = packedDetails.Relationships
                .Where(r => r.Rel != null && r.Target != null && !string.IsNullOrWhiteSpace(r.Rel.ToString()) && !string.IsNullOrWhiteSpace(r.Target.ToString()))
                .Select(r => (JsonConvert.DeserializeObject<BasicRelationship>(r.Rel.ToString()), JsonConvert.DeserializeObject<Twin>(r.Target.ToString(), new TwinJsonConverter())));
            var unpackedPoints = packedDetails.Points
                .Where(p => p != null && !string.IsNullOrWhiteSpace(p.ToString()))
                .Select(p => JsonConvert.DeserializeObject<Twin>(p.ToString(), new TwinJsonConverter()));

            adxAssetResults.Add((twin, reader["FloorId"] as Guid?, unpackedRelationships, unpackedPoints, new Dictionary<string, IEnumerable<Twin>>()));

            return Task.CompletedTask;
        }

        public async Task<Page<Category>> GetCategoriesAndAssetsAsync(
            Guid siteId,
            bool isCategoryOnly,
            List<string> modelNames,
            Guid? floorId = null,
            string continuationToken = null)
        {
            var digitalTwinService = await _digitalTwinServiceFactory.GetForSiteAsync(siteId);
            var modelParser = await digitalTwinService.GetModelParserAsync();
            var interfaceInfos = await GetAssetInterfaceInfos(digitalTwinService, modelParser, modelNames);

            var page = (floorId == null)
                ? await digitalTwinService.GetSiteTwinsAsync(continuationToken)
                : await digitalTwinService.GetFloorTwinsAsync(floorId.Value, continuationToken);
            
            var categories = await MapToCategoriesAsync(digitalTwinService, interfaceInfos, page.Content.ToList(), floorId, isCategoryOnly);

            return new Page<Category>
            {
                ContinuationToken = page.ContinuationToken,
                Content = categories
            };
        }
        public async Task<List<TwinGeometryViewerIdDto>> GetTwinsWithGeometryIdAsync(GetTwinsWithGeometryIdRequest request)
        {
            try
            {
                var digitalTwinService = await _digitalTwinServiceFactory.GetForSiteAsync(request.SiteId);

                Page<TwinWithRelationships> page=null;
                var result = new List<TwinGeometryViewerIdDto>();
                do
                {
                    page = (request.ModuleTypeNamePaths != null && request.ModuleTypeNamePaths.Any())
                        ? await digitalTwinService.GetTwinsByModuleTypeNameAsync(request.ModuleTypeNamePaths,page?.ContinuationToken)
                        : (request.FloorId.HasValue
                            ? await digitalTwinService.GetFloorTwinsAsync(request.FloorId.Value, page?.ContinuationToken)
                            : await digitalTwinService.GetSiteTwinsAsync(page?.ContinuationToken));
                    if (page.Content.Any(c => c.GeometryViewerId.HasValue))
                        result.AddRange(page.Content.Where(c => c.GeometryViewerId.HasValue)
                            .Select(TwinGeometryViewerIdDto.MapFromModel));

                } while (!string.IsNullOrEmpty(page.ContinuationToken));

                return result;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot get GeometryViewerId of twins");
            }
            return null;
        }

        

        private async Task<List<InterfaceInfo>> GetAssetInterfaceInfos(IDigitalTwinService digitalTwinService, IDigitalTwinModelParser modelParser, List<string> modelNames = null)
        {
            var interfaceInfos = new List<InterfaceInfo>();
            modelNames ??= new List<string>();
            // Default to return assets and spaces [rooms]
            if (modelNames.Count == 0)
            {
                modelNames.Add(DtdlTopLevelModelName.Asset);
                modelNames.Add(DtdlTopLevelModelName.Space);
            }

            foreach (var modelName in modelNames)
            {
                foreach (var id in MapToTopLevelModelIds(digitalTwinService, modelName))
                {
                    var interfaceInfo = modelParser.GetInterfaceHierarchy(id);
                    if (interfaceInfo?.Children?.Any() == true)
                    {
                        interfaceInfos.AddRange(interfaceInfo.Children);
                    }
                }
            }

            return interfaceInfos;
        }

        public async Task<Asset> GetAssetByUniqueId(Guid siteId, Guid id)
        {
            var digitalTwinService = await _digitalTwinServiceFactory.GetForSiteAsync(siteId);
            var twin = await digitalTwinService.GetTwinByUniqueIdAsync(id);
            if (twin == null)
                throw new ResourceNotFoundException(AssetResourceType, id);
            // MapAsset gets the floor and points and relationships asyncrounously.
            var asset = await MapAsset(digitalTwinService, twin);

            return asset;
        }

        public async Task<Asset> GetAssetById(Guid siteId, string id)
        {
            var digitalTwinService = await _digitalTwinServiceFactory.GetForSiteAsync(siteId);

            var twin = await digitalTwinService.GetTwinByIdAsync(id);
            if (twin == null)
                throw new ResourceNotFoundException(AssetResourceType, id);

            var asset = await MapAsset(digitalTwinService, twin);

            return asset;
        }

        public async Task<Asset> GetAssetByForgeViewerId(Guid siteId, string forgeViewerId)
        {
            var digitalTwinService = await _digitalTwinServiceFactory.GetForSiteAsync(siteId);

            var twin = await digitalTwinService.GetTwinByForgeViewerIdAsync(forgeViewerId);
            if (twin == null)
                throw new ResourceNotFoundException(AssetResourceType, forgeViewerId);

            var asset = await MapAsset(digitalTwinService, twin);

            return asset;
        }

        public async Task<List<Asset>> GetAssetsAsync(
            Guid siteId,
            Guid? categoryId,
            Guid? floorId,
            string searchKeyword,
            bool liveDataOnly = false,
            bool includeExtraProperties = false,
            int startItemIndex = 0,
            int pageSize = int.MaxValue)
        {
            var assets = await GetSiteAdxAssets(
                siteId,
                floorId,
                (categoryId != null) ? new List<Guid> { categoryId.Value } : null,
                searchKeyword,
                liveDataOnly,
                includeExtraProperties,
                startItemIndex,
                pageSize);

            return assets.ToList();
        }

        public async Task<Page<Asset>> GetAssets(
            Guid siteId,
            Guid? categoryId,
            Guid? floorId,
            string searchKeyword,
            bool liveDataOnly = false,
            bool includeExtraProperties = false,
            string continuationToken = null)
        {
            var digitalTwinService = await _digitalTwinServiceFactory.GetForSiteAsync(siteId);

            var floors = await GetFloors(siteId);
            if (floors.Count == 0)
                throw new NotFoundException($"Site {siteId} has no floors.");

            IAdtQueryBuilder qb = AdtQueryBuilder.Create();
            if (floorId != null)
            {
                qb = (qb as IAdtQuerySelector).Select("Asset")
                    .FromDigitalTwins()
                    .Match(new string[] { "locatedIn", "isPartOf" }, source: "Asset", target: "Floor", "*..2")
                    .Where()
                    .WithStringProperty("Floor.$dtId", floors.Single(f => f.UniqueId == floorId).Id)
                    .And();
            }
            else
            {
                qb = (qb as IAdtQuerySelector).Select("Asset", "Floor.uniqueID")
                    .FromDigitalTwins()
                    .Match(new string[] { "locatedIn", "isPartOf" }, source: "Asset", target: "Floor", "*..2")
                    .Where()
                    .WithAnyModel(new string[] { WillowInc.LevelModelId }, "Floor")
                    .And()
                    .WithPropertyIn("Floor.$dtId", floors.Select(f => f.Id))
                    .And();
            }

            var modelParser = await digitalTwinService.GetModelParserAsync();

            if (categoryId != null)
            {
                var interfaceInfos = modelParser.GetInterfaceDescendants(digitalTwinService.SiteAdtSettings.AssetModelIds);
                var categoryDtmi = interfaceInfos.Single(i => i.Value.GetUniqueId() == categoryId.Value).Value.Id.ToString();

                qb = (qb as IAdtQueryFilterGroup).WithAnyModel(new string[] { categoryDtmi }, "Asset")
                    .And();
            }
            else
            {
                var models = new List<string>(digitalTwinService.SiteAdtSettings.AssetModelIds);
                models.AddRange(digitalTwinService.SiteAdtSettings.BuildingComponentModelIds);
                qb = (qb as IAdtQueryFilterGroup)
                    .WithAnyModel(models, "Asset")
                    .And();
            }

            if (!string.IsNullOrWhiteSpace(searchKeyword))
            {
                (qb as IAdtQueryFilterGroup)
                    .OpenGroupParenthesis()
                    .Contains("Asset.name", searchKeyword)
                    .Or()
                    .Contains("Asset.$dtId", searchKeyword)
                    .CloseGroupParenthesis()
                    .And();
            }

            var query = (qb as IAdtQueryFilterGroup).WithStringProperty("Asset.siteID", siteId.ToString()).GetQuery();

            var pageable = _adtApiService.QueryTwins<BasicDigitalTwin>(digitalTwinService.SiteAdtSettings.InstanceSettings, query);

            var azurePage = await pageable.AsPages(continuationToken).FirstAsync();

            var assetTwins = azurePage.Values.Select(t => (t.GetValue("Asset"), floorId ?? t.GetGuidValue("uniqueID")));

            var pointTasks = new Dictionary<string, Task<List<Point>>>();
            foreach (var twin in assetTwins)
            {
                pointTasks.Add(twin.Item1.Id, GetAssetPoints(digitalTwinService, twin.Item1.Id));
            }
            await Task.WhenAll(pointTasks.Select(pt => pt.Value));

            if (liveDataOnly)
            {
                assetTwins = assetTwins.Where(t => pointTasks[t.Item1.Id].Result.Count > 0);
            }

            var assets =
                await MapAssets(
                    digitalTwinService,
                    modelParser,
                    assetTwins.Select(x => (TwinWithRelationships.MapFrom(x.Item1), x.Item2)).ToList(),
                    pointTasks.ToDictionary(t => t.Key, t => t.Value.Result));

            if (includeExtraProperties)
            {
                await EnrichDetailedRelationships(digitalTwinService, assets);
            }

            return new Page<Asset> { Content = assets, ContinuationToken = azurePage.ContinuationToken };
        }

        public async Task<AssetRelationshipsDto> GetAssetRelationshipsAsync(Guid siteId, Guid id)
        {
            var digitalTwinService = await _digitalTwinServiceFactory.GetForSiteAsync(siteId);

            try
            {
                var twin = await digitalTwinService.GetTwinByUniqueIdAsync(id);
                if (twin == null)
                    throw new ResourceNotFoundException(AssetResourceType, id);

                var relationshipTask = digitalTwinService.GetRelationships(twin.Id);
                var incomingRelationshipTask = digitalTwinService.GetIncomingRelationshipsAsync(twin.Id);

                return new AssetRelationshipsDto
                {
                    Relationships = AssetRelationshipDto.MapFrom(await relationshipTask),
                    IncomingRelationships = AssetIncomingRelationshipDto.MapFrom(await incomingRelationshipTask)
                };
            }
            catch (DigitalTwinCoreException)
            {
                throw new ResourceNotFoundException(AssetResourceType, id);
            }
        }

        public async Task<IEnumerable<Asset>> GetSiteAdxAssets(
            Guid siteId,
            Guid? floorId = null,
            IEnumerable<Guid> categoryIds = null,
            string searchKeyword = null,
            bool liveDataOnly = false,
            bool includeExtraProperties = false,
            int startItemIndex = 0,
            int pageSize = int.MaxValue)
        {
            var models = await GetAssetModelIds(siteId, categoryIds);

            var siteAssetsQb = (AdxQueryBuilder.Create()
                .Select(AdxConstants.ActiveTwinsFunction)
                .Where()
                .Property("SiteId", siteId.ToString()) as IAdxQueryFilterGroup)
                .And()
                .PropertyIn("ModelId", models);

            if (floorId != null)
            {
                siteAssetsQb.And().Property("FloorId", floorId.ToString());
            }

            if (!string.IsNullOrWhiteSpace(searchKeyword))
            {
                (((siteAssetsQb
                    .And()
                    .OpenGroupParentheses() as IAdxQueryFilterGroup)
                    .Contains("Id", searchKeyword) as IAdxQueryFilterGroup)
                    .Or()
                    .Contains("Name", searchKeyword) as IAdxQueryFilterGroup)
                    .CloseGroupParentheses();
            }

            var siteAssetQuery = (siteAssetsQb as IAdxQuerySelector)
                .ProjectKeep("Id", "FloorId", "Raw")
                .GetQuery();

            siteAssetsQb = AdxQueryBuilder.Create().Materialize(siteAssetQuery);

            var rawAssetJoinQb = AdxQueryBuilder.Create()
                    .Select(AdxConstants.ActiveRelationshipsFunction);

            if (includeExtraProperties)
            {
                rawAssetJoinQb
                    .Where()
                    .PropertyIn("Name", _assetDetailRelationships);
            }

            var rawAssetJoinQuery = (rawAssetJoinQb as IAdxQuerySelector)
                .ProjectKeep("TargetId", "Name", "Raw", "SourceId")
                .GetQuery();

            var rawAssetsQb = (((((((((AdxQueryBuilder.Create() as IAdxQueryFilterGroup)
                .OpenGroupParentheses() as IAdxQuerySelector)
                .Select("siteassets") as IAdxQuerySelector)
                .Join(
                    rawAssetJoinQuery,
                    "Id",
                    "SourceId",
                    "leftouter")
                .Join(
                    (AdxQueryBuilder.Create()
                        .Select(AdxConstants.ActiveTwinsFunction) as IAdxQuerySelector)
                        .ProjectKeep("Id", "FloorId", "Raw", "Name")
                        .GetQuery(),
                    "TargetId",
                    "Id",
                    "leftouter")
                .Project(
                    "Id",
                    "FloorId",
                    "Raw",
                    "RelName = Name1",
                    "RelTarget = Raw2",
                    "RelValue = pack(\"Rel\", Raw1, \"Target\", Raw2)") as IAdxQueryFilterGroup)
                .CloseGroupParentheses() as IAdxQuerySelector)
                .Union() as IAdxQueryFilterGroup)
                .OpenGroupParentheses() as IAdxQuerySelector)
                .Select("siteassets") as IAdxQuerySelector)
                .Join(
                    (AdxQueryBuilder.Create()
                        .Select(AdxConstants.ActiveRelationshipsFunction)
                        .Where()
                        .PropertyIn("Name", new string[] { "isCapabilityOf", "hostedBy" }) as IAdxQuerySelector)
                        .ProjectKeep("TargetId", "Name", "Raw", "SourceId")
                        .GetQuery(),
                    "Id",
                    "TargetId",
                    "leftouter")
                .Join(
                    (AdxQueryBuilder.Create()
                        .Select(AdxConstants.ActiveTwinsFunction) as IAdxQuerySelector)
                        .ProjectKeep("Id", "FloorId", "Raw")
                        .GetQuery(),
                    "SourceId",
                    "Id",
                    "leftouter")
                .Project("Id", "FloorId", "Raw", "PtsValue = Raw2") as IAdxQueryFilterGroup)
                .CloseGroupParentheses();

            if (liveDataOnly)
            {
                (rawAssetsQb as IAdxQueryWhere)
                    .Where()
                    .IsNotEmpty("PtsValue");
            }

            var query = AdxQueryBuilder.Create()
                .Let("siteassets", siteAssetsQb.GetQuery())
                .Let("rawassets", rawAssetsQb.GetQuery());

            ((query
                .Select("rawassets") as IAdxQuerySelector)
                .Summarize()
                .SetProperty("RelData").MakeSet(true, "RelValue")
                .SetProperty("PtsData").MakeSet(true, "PtsValue")
                .TakeAny(false, "Raw")
                .By("Id", "FloorId") as IAdxQuerySelector)
                .Summarize()
                .SetProperty("Details").MakeBag(true, "pack(\"Relationships\", RelData, \"Points\", PtsData)")
                .TakeAny(false, "Raw")
                .By("Id", "FloorId");

            if (includeExtraProperties)
            {
                query
                    .Join(
                        ((((AdxQueryBuilder.Create() as IAdxQueryFilterGroup)
                            .OpenGroupParentheses() as IAdxQuerySelector)
                            .Select("rawassets") as IAdxQuerySelector)
                            .Summarize()
                            .SetProperty("Targets").MakeSet(from: "RelTarget")
                            .By("Id, RelName") as IAdxQuerySelector)
                            .Summarize()
                            .SetProperty("ExtraProperties").MakeBag(from: "pack(RelName, Targets)")
                            .By("Id")
                            .CloseGroupParentheses()
                            .GetQuery(),
                        "Id",
                        "Id",
                        "leftouter");
            }

            (query as IAdxQueryFilterGroup)
                .Sort("Id");

            var digitalTwinService = await _digitalTwinServiceFactory.GetForSiteAsync(siteId);

            var adxAssetResults = new List<(Twin, Guid?, IEnumerable<(BasicRelationship, Twin)>, IEnumerable<Twin>, Dictionary<string, IEnumerable<Twin>>)>();
            using var reader = await _adxHelper.Query(digitalTwinService, query.GetQuery());
            var counter = 0;
            while (reader.Read())
            {
                // Map only a single page for performance reasons.
                if (adxAssetResults.Count >= pageSize)
                {
                    break;
                }

                if (counter >= startItemIndex)
                {
                    var twin = JsonConvert.DeserializeObject<Twin>(reader["Raw"].ToString(), new TwinJsonConverter());

                    var packedDetails = (reader["Details"] as JObject).ToObject<AdxDetailedRelationshipsPacked>();
                    var unpackedRelationships = packedDetails.Relationships
                        .Where(r => r.Rel != null && r.Target != null && !string.IsNullOrWhiteSpace(r.Rel.ToString()) && !string.IsNullOrWhiteSpace(r.Target.ToString()))
                        .Select(r => (JsonConvert.DeserializeObject<BasicRelationship>(r.Rel.ToString()), JsonConvert.DeserializeObject<Twin>(r.Target.ToString(), new TwinJsonConverter())));
                    var unpackedPoints = packedDetails.Points
                        .Where(p => p != null && !string.IsNullOrWhiteSpace(p.ToString()))
                        .Select(p => JsonConvert.DeserializeObject<Twin>(p.ToString(), new TwinJsonConverter()));

                    var unpackedExtraProperties = new Dictionary<string, IEnumerable<Twin>>();
                    if (includeExtraProperties)
                    {
                        var packedExtraProperties = (reader["ExtraProperties"] as JObject).ToObject<Dictionary<string, ICollection<object>>>();
                        unpackedExtraProperties = packedExtraProperties
                            .Where(i => i.Value.Count > 0)
                            .ToDictionary(k => k.Key, v => v.Value.Select(t => JsonConvert.DeserializeObject<Twin>(t.ToString(), new TwinJsonConverter())));
                    }

                    adxAssetResults.Add((twin, reader["FloorId"] as Guid?, unpackedRelationships, unpackedPoints, unpackedExtraProperties));
                }

                counter++;
            }

            var mapped = await MapAdxAssets(siteId, adxAssetResults);

            return mapped;
        }

        private async Task<IEnumerable<string>> GetAssetModelIds(Guid siteId, IEnumerable<Guid> categoryIds = null, string modelId = null)
        {
            var modelIds = await _digitalTwinServiceFactory.GetModelIds(new List<Guid> { siteId }, categoryIds?.ToArray(), modelId).ToArrayAsync();

            return modelIds.Single(x => x.Key == siteId).Value;
        }

        private async Task EnrichDetailedRelationships(IDigitalTwinService digitalTwinService, List<Asset> assets) // Investa-specific
        {
            var relationshipTasks = new Dictionary<string, Task<List<BasicRelationship>>>();
            foreach (var asset in assets)
            {
                relationshipTasks.Add(asset.Identifier, _adtApiService.GetRelationships(digitalTwinService.SiteAdtSettings.InstanceSettings, asset.Identifier));
            }
            var relationshipResults = relationshipTasks.ToDictionary(t => t.Key, t => t.Value.Result);

            var enrichable = assets.Where(a =>
                _assetDetailRelationships.Any(adr =>
                    relationshipResults[a.Identifier].Select(r => r.Name).Contains(adr, StringComparer.InvariantCultureIgnoreCase)));

            foreach (var asset in enrichable)
            {
                var validRelationships = relationshipResults[asset.Identifier].Where(r => _assetDetailRelationships.Contains(r.Name, StringComparer.InvariantCultureIgnoreCase));

                var enrichedRelationships = await digitalTwinService.GetRelationships(validRelationships);

                var nonEmpty = enrichedRelationships.Where(r => r.Target != null);
                foreach (var rel in nonEmpty)
                {
                    if (asset.DetailedRelationships.ContainsKey(rel.Name))
                    {
                        asset.DetailedRelationships[rel.Name].Add(rel.Target);
                    }
                    else
                    {
                        asset.DetailedRelationships.Add(rel.Name, new List<TwinWithRelationships>() { rel.Target });
                    }
                }
            }
        }

        public async Task<IEnumerable<AssetNameDto>> GetAssetNames(Guid siteId, IEnumerable<Guid> ids)
        {
            var query = ((AdxQueryBuilder.Create()
                .Select(AdxConstants.ActiveTwinsFunction)
                .Where()
                .Property("SiteId", siteId.ToString()) as IAdxQueryFilterGroup)
                .And()
                .PropertyIn("UniqueId", ids.Select(i => i.ToString())) as IAdxQuerySelector)
                .Project("UniqueId", "Name", "FloorId")
                .GetQuery();

            var digitalTwinService = await _digitalTwinServiceFactory.GetForSiteAsync(siteId);

            var assetNames = new List<AssetNameDto>();
            using var reader = await _adxHelper.Query(digitalTwinService, query);
            while (reader.Read())
            {
                assetNames.Add(new AssetNameDto
                {
                    Id = Guid.Parse(reader["UniqueId"].ToString()),
                    Name = reader["Name"] as string,
                    FloorId = reader["FloorId"] as Guid?
                });
            }

            return assetNames;
        }

		public async Task<List<TwinSimpleDto>> GetSimpleTwinsDataAsync(IEnumerable<TwinsForMultiSitesRequest> request, CancellationToken cancellationToken)
		{
			var result = new List<TwinSimpleDto>();
			if (request==null || !request.Any())
			{
				return result;
			}
			var database = await _siteAdtSettingsProvider.GetFirstConfiguredAdxDatabaseOrDefault(request.Select(x => x.SiteId));
			if (string.IsNullOrEmpty(database))
			{
				return result;
			}

			var query = GetAssetNamesForMultiSitesQuery(request);
			using var reader = await _adxHelper.Query(database, query, cancellationToken);
			while (reader.Read())
			{
				Guid? floorId = null;
				if (Guid.TryParse(reader["FloorId"]?.ToString(), out var parsedFloorId))
				{
					floorId = parsedFloorId;
				}
				result.Add(new TwinSimpleDto
				{
					Id = reader["Id"] as string,
					UniqueId = Guid.Parse(reader["UniqueId"].ToString()),
					Name = reader["Name"] as string,
					SiteId = Guid.Parse(reader["SiteId"].ToString()),
					FloorId = floorId,
                    ModelId = reader["ModelId"] as string
                });
			}

			return result;
		}
		private string GetAssetNamesForMultiSitesQuery(IEnumerable<TwinsForMultiSitesRequest> request)
		{
			var queryBuilder = new StringBuilder();
			queryBuilder.Append(AdxConstants.ActiveTwinsFunction);
			queryBuilder.AppendLine("| where");
			queryBuilder.AppendLine(
			   string.Join(" or ",
			   request.Select(x =>
				   $"( SiteId == '{x.SiteId}' and Id in ({string.Join(',', x.TwinIds.Select(i=> $"\"{i}\""))}))"
			   ))
		   );
			queryBuilder.AppendLine("| project Id, SiteId, UniqueId, Name, FloorId, ModelId");

			return queryBuilder.ToString();
		}
		#endregion

		#region Documents
		public async Task<List<Document>> GetDocumentsForAssetAsync(Guid siteId, Guid assetId)
        {
            var digitalTwinService = await _digitalTwinServiceFactory.GetForSiteAsync(siteId);

            try
            {
                var twin = await digitalTwinService.GetTwinByUniqueIdAsync(assetId);
                if (twin == null)
                    throw new ResourceNotFoundException(AssetResourceType, assetId);

                var query = $"select Document from DIGITALTWINS match (Parent)-[:hasDocument]->(Document) where Parent.$dtId = '{twin.Id}'";
                var docTwins = await digitalTwinService.GetTwinsByQueryAsync(query, "Document");

                return MapDocuments(docTwins);
            }
            catch (DigitalTwinCoreException)
            {
                throw new ResourceNotFoundException(AssetResourceType, assetId);
            }
        }

        private List<Document> MapDocuments(List<TwinWithRelationships> twins)
        {
            return twins.Select(MapDocument).ToList();
        }

        private Document MapDocument(TwinWithRelationships twin)
        {
            var url = twin.GetStringProperty(Properties.Url);
            return new Document
            {
                ModelId = twin.Metadata.ModelId,
                TwinId = twin.Id,
                Id = twin.UniqueId,
                Name = twin.DisplayName,
                Uri = string.IsNullOrEmpty(url) ? null : new Uri(url)
            };
        }
        #endregion

        #region Mapping
        private async Task<List<Category>> MapToCategories(
            IEnumerable<InterfaceInfo> children,
            List<Asset> assets,
            bool isCategoryOnly)
        {
            var output = new List<Category>();
            if (assets.Count == 0)
            {
                return output;
            }

            foreach (var category in children)
            {
                var categoryAssets = assets.Where(t => t.CategoryId == category.Model.GetUniqueId());

                categoryAssets = isCategoryOnly
                    ? categoryAssets.Select(a => new Asset { TwinId = a.TwinId, Id = a.Id, Name = a.Name, Points = null }).ToList()
                    : categoryAssets;

                output.Add(new Category
                {
                    Id = category.Model.GetUniqueId(),
                    Name = category.DisplayName,
                    Categories = await MapToCategories(category.Children, assets, isCategoryOnly), // Recursive.
                    Assets = categoryAssets.ToList()
                });
            }

            return output.Where(o => o.Categories.Any() || o.Assets.Any()).ToList();
        }

        private async Task<List<Category>> MapToCategoriesAsync(
            IDigitalTwinService digitalTwinService,
            IEnumerable<InterfaceInfo> children,
            IReadOnlyCollection<TwinWithRelationships> twins,
            Guid? floorId,
            bool isCategoryOnly)
        {
            var output = new List<Category>();
            if (twins.Count == 0)
            {
                return output;
            }

            foreach (var category in children)
            {
                var assetTwins = twins.Where(t => t.Metadata.ModelId == category.Model.Id.AbsoluteUri);

                var assets = new List<Asset>();
                assets = isCategoryOnly
                    ? assetTwins.Select(t => new Asset { TwinId = t.Id, Id = t.UniqueId, Name = t.DisplayName, Points = null }).ToList()
                    : await MapAssetsAsync(digitalTwinService, assetTwins);

                if (category.Children.Count == 1 &&
                    !assets.Any() &&
                    CompareModelNameWithoutPrefixOrVersion(category.Model.Id, category.Children.Single().Model.Id))
                {
                    // TODO: Investigate -- this code never gets hit for entire asset tree
                    var childCategory = category.Children.Single();
                    var childAssetTwins = twins.Where(t => t.Metadata.ModelId == childCategory.Model.Id.AbsoluteUri);
                    output.Add(new Category
                    {
                        Id = childCategory.Model.GetUniqueId(),
                        Name = childCategory.DisplayName,
                        Categories = await MapToCategoriesAsync(digitalTwinService, childCategory.Children, twins, floorId, isCategoryOnly),
                        Assets = isCategoryOnly
                                ? childAssetTwins.Select(t => new Asset { Id = t.UniqueId, Name = t.DisplayName }).ToList()
                                : await MapAssetsAsync(digitalTwinService, childAssetTwins)
                    });
                }
                else
                {
                    var withPoints = assets.Select(async a => { a.Points ??= await GetPointsSimpleAsync(digitalTwinService, a.TwinId); return a; })
                                        .Select(a => a.Result);

                    output.Add(new Category
                    {
                        Id = category.Model.GetUniqueId(),
                        Name = category.DisplayName,
                        Categories = await MapToCategoriesAsync(digitalTwinService, category.Children, twins, floorId, isCategoryOnly),
                        Assets = withPoints.ToList()
                    });
                }
            }

            return output.Where(o => o.Categories.Any() || o.Assets.Any()).ToList();
        }

        private async Task<IEnumerable<Asset>> MapAdxAssets(
            Guid siteId,
            IEnumerable<(Twin, Guid?, IEnumerable<(BasicRelationship, Twin)>, IEnumerable<Twin>, Dictionary<string, IEnumerable<Twin>>)> items)
        {
            var digitalTwinService = await _digitalTwinServiceFactory.GetForSiteAsync(siteId);
            var modelParser = await digitalTwinService.GetModelParserAsync();

            var assets = new List<Asset>();
            foreach (var item in items)
            {
                var asset = await MapAdxAsset(siteId, digitalTwinService, modelParser, item);
                assets.Add(asset);
            }

            return assets;
        }

        private async Task<Asset> MapAdxAsset(
            Guid siteId,
            IDigitalTwinService digitalTwinService,
            IDigitalTwinModelParser modelParser,
            (Twin, Guid?, IEnumerable<(BasicRelationship, Twin)>, IEnumerable<Twin>, Dictionary<string, IEnumerable<Twin>>) item)
        {
            var (twin, floorId, detailedRelationships, points, extraProperties) = item;

            var interfaceInfo = modelParser.GetInterface(twin.Metadata.ModelId);
            var categoryId = interfaceInfo.GetUniqueId();
            var identifier = twin.GetStringProperty(Properties.PhysicalTagNumber)
                                         ?? twin.GetStringProperty(Properties.Code)
                                         ?? twin.Id;
            var tags = _tagMapperService.MapTags(
                                    digitalTwinService.SiteAdtSettings.SiteId,
                                    twin.Metadata.ModelId,
                                    twin.GetJObjectProperty(Properties.Tags)?.ToObject<Dictionary<string, object>>());
            var properties = MapAssetProperties(interfaceInfo, twin);

            var assetRelationships = await MapAdxAssetRelationships(modelParser, interfaceInfo, detailedRelationships);
            var assetPoints = await MapAdxPoints(siteId, modelParser, points.Select(p => (p, new List<Twin>(), new List<Twin>())));

            return new Asset
            {
                ModelId = twin.Metadata.ModelId,
                TwinId = twin.Id,
                Id = twin.UniqueId,
                Name = twin.DisplayName,
                ForgeViewerModelId = twin.GetStringProperty(Properties.GeometryViewerId),
                ModuleTypeNamePath = twin.GetStringProperty(Properties.GeometrySpatialReference),
                FloorId = floorId,
                Identifier = identifier,
                Tags = tags,
                Properties = properties,
                Relationships = assetRelationships,
                DetailedRelationships = extraProperties.ToDictionary(x => x.Key, x => x.Value.Select(t => new TwinWithRelationships
                {
                    Id = twin.Id,
                    CustomProperties = Twin.MapCustomProperties(t.CustomProperties),
                    Metadata = t.Metadata
                }).ToList()),
                Points = assetPoints,
                CategoryId = categoryId,
                CategoryName = interfaceInfo.GetDisplayName()
            };
        }

        private async Task<IEnumerable<AssetRelationship>> MapAdxAssetRelationships(
            IDigitalTwinModelParser modelParser,
            DTInterfaceInfo interfaceInfo,
            IEnumerable<(BasicRelationship, Twin)> detailedRelationships)
        {
            var relationshipTasks = new List<Task<AssetRelationship>>();
            foreach (var rel in detailedRelationships)
            {
                relationshipTasks.Add(MapAdxAssetRelationship(modelParser, interfaceInfo, rel));
            }

            return relationshipTasks.Select(t => t.Result);
        }

        private Task<AssetRelationship> MapAdxAssetRelationship(
            IDigitalTwinModelParser modelParser,
            DTInterfaceInfo interfaceInfo,
            (BasicRelationship, Twin) detailedRelationship)
        {
            var name = interfaceInfo.GetPropertyDisplayName(detailedRelationship.Item1.Name);
            if (detailedRelationship.Item1.Properties.Any(p => p.Key != Dtdl.ETag))
            {
                name += " " + detailedRelationship.Item1.Properties.First(p => p.Key != Dtdl.ETag).Value;
            }

            return Task.FromResult(new AssetRelationship
            {
                Name = detailedRelationship.Item1.Name,
                DisplayName = name,
                TargetId = detailedRelationship.Item2.UniqueId,
                TargetName = detailedRelationship.Item2.DisplayName,
                TargetType = modelParser.GetInterface(detailedRelationship.Item2.Metadata.ModelId).GetDisplayName()
            });
        }

        private async Task<List<Point>> MapAdxPoints(
            Guid siteId,
            IDigitalTwinModelParser modelParser,
            IEnumerable<(Twin, List<Twin>, List<Twin>)> pointTwins)
        {
            var mapped = pointTwins.AsParallel().Select(t =>
                MapAdxPoint(
                    siteId,
                    modelParser.GetInterface(t.Item1.Metadata.ModelId),
                    t));

            return await Task.FromResult(mapped.ToList());
        }

        private Point MapAdxPoint(
            Guid siteId,
            DTInterfaceInfo interfaceInfo,
            (Twin, List<Twin>, List<Twin>) item)
        {
            var twin = item.Item1;
            PointValue currentValue = null;
            if (interfaceInfo.Contents.ContainsKey(Properties.LivedataLastValue))
            {
                var currentValueProperty = interfaceInfo.Contents[Properties.LivedataLastValue] as DTPropertyInfo;
                if (currentValueProperty != null)
                {
                    string unit = null;
                    if (currentValueProperty.SupplementalProperties.ContainsKey(SupplementalProperties.Unit))
                    {
                        unit = currentValueProperty.SupplementalProperties[SupplementalProperties.Unit] as string;
                    }
                    currentValue = new PointValue
                    {
                        Unit = unit,
                        Value = twin.GetProperty<object>(Properties.LivedataLastValue)
                    };
                }
            }

            Guid? trendId = twin.UniqueId;
            if (Guid.TryParse(twin.GetStringProperty(Properties.TrendID), out var trendIdParsed))
            {
                trendId = trendIdParsed;
            }

            var trendIntervalValue = twin.GetPropertyValue<double>(Properties.TrendInterval);

            return new Point
            {
                ModelId = twin.Metadata.ModelId,
                TwinId = twin.Id,
                Id = twin.UniqueId,
                TrendId = trendId.GetValueOrDefault(),
                ExternalId = twin.GetStringProperty(Properties.ExternalID),
                Name = twin.DisplayName,
                Description = twin.GetStringProperty(Properties.Description),
                Type = MapPointType(twin.GetStringProperty(Properties.Type)),
                Tags = _tagMapperService.MapTags(siteId, twin.Metadata.ModelId, twin.GetObjectProperty(Properties.Tags)),
                DisplayPriority = twin.GetPropertyValue<decimal>(Properties.DisplayPriority),
                DisplayName = interfaceInfo.GetDisplayName(),
                CurrentValue = currentValue,
                Assets = item.Item2,
                Devices = item.Item3,
                CategoryName = interfaceInfo.GetDisplayName(),
                IsDetected = twin.GetPropertyValue<bool>(Properties.Detected),
                IsEnabled = twin.GetPropertyValue<bool>(Properties.Enabled),
                TrendInterval = trendIntervalValue.HasValue ? TimeSpan.FromSeconds(trendIntervalValue.Value) : (TimeSpan?)null,
                Properties = MapAssetProperties(interfaceInfo, twin),
                Communication = MapPointCommunication(twin)
            };
        }

        private async Task<List<Asset>> MapAssets(
            IDigitalTwinService digitalTwinService,
            IDigitalTwinModelParser modelParser,
            List<(TwinWithRelationships, Guid)> twins,
            Dictionary<string, List<Point>> assetPoints)
        {
            if (twins == null)
                return new List<Asset>();

            var mappingTasks = new List<Task<Asset>>();
            foreach (var twin in twins)
            {
                mappingTasks.Add(
                    MapAsset(
                        digitalTwinService,
                        twin.Item1,
                        modelParser,
                        twin.Item2,
                        assetPoints.TryGetValue(twin.Item1.Id, out var points) ? points : new List<Point>()));
            }

            return mappingTasks.Select(t => t.Result).ToList();
        }

        private async Task<Asset> MapAsset(
            IDigitalTwinService digitalTwinService,
            TwinWithRelationships twin,
            IDigitalTwinModelParser modelParser = null,
            Guid? floorId = null,
            List<Point> points = null)
        {
            // Start all long running tasks.
            var floorTask = (floorId is null) ? digitalTwinService.GetTwinFloor(twin.Id) : null;
            var pointTask = (points is null) ? GetAssetPoints(digitalTwinService, twin.Id) : null;
            var relationshipTask = digitalTwinService.AppendRelationshipsAsync(twin);

            modelParser ??= await digitalTwinService.GetModelParserAsync();
            var interfaceInfo = modelParser.GetInterface(twin.Metadata.ModelId);
            var categoryId = interfaceInfo.GetUniqueId();
            var identifier = twin.GetStringProperty(Properties.PhysicalTagNumber)
                                            ?? twin.GetStringProperty(Properties.Code)
                                            ?? twin.Id;
            var tags = _tagMapperService.MapTags(
                                    digitalTwinService.SiteAdtSettings.SiteId,
                                    twin.Metadata.ModelId, twin.GetObjectProperty(Properties.Tags));
            var properties = MapAssetProperties(interfaceInfo, twin);

            // Append must finish before mapping.
            await relationshipTask;
            var relationships = MapAssetRelationships(modelParser, interfaceInfo, twin);

            return new Asset
            {
                ModelId = twin.Metadata.ModelId,
                TwinId = twin.Id,
                Id = twin.UniqueId,
                Name = twin.DisplayName,
                ForgeViewerModelId = twin.GetStringProperty(Properties.GeometryViewerId),
                ModuleTypeNamePath = twin.GetStringProperty(Properties.GeometrySpatialReference),
                FloorId = floorId ?? (await floorTask)?.UniqueId,
                Identifier = identifier,
                Tags = tags,
                Properties = properties,
                Relationships = relationships,
                Points = points ?? await pointTask,
                CategoryId = categoryId,
                CategoryName = interfaceInfo.GetDisplayName()
            };
        }

        private async Task<List<Asset>> MapAssetsAsync(
            IDigitalTwinService digitalTwinService,
            IEnumerable<TwinWithRelationships> twins,
            bool includePoints = true)
        {
            if (twins == null)
            {
                return new List<Asset>();
            }

            var mappingTasks = new List<Task<Asset>>();
            foreach (var twin in twins)
            {
                mappingTasks.Add(MapAssetAsync(digitalTwinService, twin, includePoints));
            }

            return mappingTasks.Select(t => t.Result).ToList();
        }

        private async Task<Asset> MapAssetAsync(
            IDigitalTwinService digitalTwinService,
            TwinWithRelationships twin,
            bool includePoints = true,
            IDigitalTwinModelParser modelParser = null)
        {
            modelParser ??= await digitalTwinService.GetModelParserAsync();
            var points = !includePoints ? null : await GetAssetPoints(digitalTwinService, twin.Id);
            var floorId = twin.GetFloorId(digitalTwinService.SiteAdtSettings.LevelModelIds);
            var interfaceInfo = modelParser.GetInterface(twin.Metadata.ModelId);
            var categoryId = interfaceInfo.GetUniqueId();
            var identifier = twin.GetStringProperty(Properties.PhysicalTagNumber)
                                         ?? twin.GetStringProperty(Properties.Code)
                                         ?? twin.Id;
            var tags = _tagMapperService.MapTags(
                                    digitalTwinService.SiteAdtSettings.SiteId,
                                    twin.Metadata.ModelId, twin.GetObjectProperty(Properties.Tags));
            var properties = MapAssetProperties(interfaceInfo, twin);
            var relationships = MapAssetRelationships(modelParser, interfaceInfo, twin);

            return new Asset
            {
                ModelId = twin.Metadata.ModelId,
                TwinId = twin.Id,
                Id = twin.UniqueId,
                Name = twin.DisplayName,
                ForgeViewerModelId = twin.GetStringProperty(Properties.GeometryViewerId),
                ModuleTypeNamePath = twin.GetStringProperty(Properties.GeometrySpatialReference),
                FloorId = floorId,
                Identifier = identifier,
                Tags = tags,
                Properties = properties,
                Relationships = relationships,
                Points = points,
                CategoryId = categoryId,
                CategoryName = interfaceInfo.GetDisplayName()
            };
        }

        private List<AssetRelationship> MapAssetRelationships(
            IDigitalTwinModelParser modelParser,
            DTInterfaceInfo interfaceInfo,
            TwinWithRelationships twin)
        {
            return twin.Relationships.Select(r => MapAssetRelationship(modelParser, interfaceInfo, r)).OrderBy(r => r.Name).ToList();
        }

        private AssetRelationship MapAssetRelationship(
            IDigitalTwinModelParser modelParser,
            DTInterfaceInfo interfaceInfo,
            TwinRelationship relationship)
        {
            var name = interfaceInfo.GetPropertyDisplayName(relationship.Name);
            if (relationship.CustomProperties.Any(p => p.Key != Dtdl.ETag))
            {
                name += " " + relationship.CustomProperties.First(p => p.Key != Dtdl.ETag).Value;
            }

            return new AssetRelationship
            {
                Name = relationship.Name,
                DisplayName = name,
                TargetId = relationship.Target.UniqueId,
                TargetName = relationship.Target.DisplayName,
                TargetType = modelParser.GetInterface(relationship.Target.Metadata.ModelId).GetDisplayName()
            };
        }

        private static Dictionary<string, Property> MapAssetProperties(
            DTInterfaceInfo interfaceInfo,
            Twin t)
        {
            var kvps = t.CustomProperties
                .Where(p => !IgnoredProperties.Contains(p.Key))
                .Select(p => new KeyValuePair<string, Property>(
                                p.Key,
                                new Property
                                {
                                    Value = p.Value,
                                    DisplayName = interfaceInfo.GetPropertyDisplayName(p.Key),
                                    Kind = MapPropertyKind(interfaceInfo, p.Key)
                                }))
                .GroupBy(p => p.Key)
                .OrderBy(p => p.Key)
                .ToList();

            var output = new Dictionary<string, Property>();
            foreach (var grouping in kvps)
            {
                if (grouping.Count() > 1)
                {
                    var groupArray = grouping.ToArray();
                    for (var i = 0; i < groupArray.Length; i++)
                    {
                        output.Add($"{grouping.Key} #{i + 1}", groupArray[i].Value);
                    }
                }
                else
                {
                    output.Add(grouping.Key, grouping.Single().Value);
                }
            }

            return output;
        }

        private static PropertyKind MapPropertyKind(DTInterfaceInfo interfaceInfo, string key)
        {
            if (!interfaceInfo.Contents.ContainsKey(key))
            {
                return PropertyKind.Other;
            }

            var contentInfo = interfaceInfo.Contents[key];

            return contentInfo switch
            {
                DTPropertyInfo _ => PropertyKind.Property,
                DTRelationshipInfo _ => PropertyKind.Relationship,
                DTComponentInfo _ => PropertyKind.Component,
                _ => PropertyKind.Other
            };
        }

        private static readonly string[] IgnoredProperties = {
            Properties.UniqueId,
            Properties.GeometryViewerId,
            Properties.Tags,
            Properties.Name,
            Properties.Communication
        };
        #endregion

        #region Models
        private bool CompareModelNameWithoutPrefixOrVersion(Dtmi m1, Dtmi m2)
        {
            var (s1, s2) = (m1.Versionless, m2.Versionless);
            var (start1, start2) = (s1.LastIndexOf(":") + 1, s2.LastIndexOf(":") + 1);
            return 0 == string.Compare(s1, start1, s2, start2, Math.Max(s1.Length, s2.Length));
        }

        private static string[] MapToTopLevelModelIds(IDigitalTwinService digitalTwinService, string modelName) =>
            modelName switch
            {
                DtdlTopLevelModelName.Asset => digitalTwinService.SiteAdtSettings.AssetModelIds,
                DtdlTopLevelModelName.Space => digitalTwinService.SiteAdtSettings.SpaceModelIds,
                DtdlTopLevelModelName.BuildingComponent => digitalTwinService.SiteAdtSettings.BuildingComponentModelIds,
                DtdlTopLevelModelName.Structure => digitalTwinService.SiteAdtSettings.StructureModelIds,
                _ => Array.Empty<string>(),
            };
        #endregion

        #region Devices
        public async Task<Page<Device>> GetDevicesAsync(Guid siteId, bool? includePoints, string continuationToken = null)
        {
            var digitalTwinService = await _digitalTwinServiceFactory.GetForSiteAsync(siteId);

            var query = GetDevicesQuery(siteId, digitalTwinService, x => x.SelectAll()).GetQuery();

            var pageable = _adtApiService.QueryTwins<BasicDigitalTwin>(digitalTwinService.SiteAdtSettings.InstanceSettings, query);

            var page = await pageable.AsPages(continuationToken).FirstAsync();

            var devices = new ConcurrentBag<Device>();

            var mapDevices = page.Values.Select(async t => devices.Add(await MapDeviceAsync(siteId, digitalTwinService, await digitalTwinService.GetModelParserAsync(), Twin.MapFrom(t), includePoints)));

            await Task.WhenAll(mapDevices);

            return new Page<Device> { Content = devices, ContinuationToken = page.ContinuationToken };
        }

        public async Task<List<Device>> GetSiteAdxDevicesAsync(Guid siteId, bool includePoints, Dictionary<string, string> exactMatchFilters = null)
        {
            var digitalTwinService = await _digitalTwinServiceFactory.GetForSiteAsync(siteId);
            var modelParser = await digitalTwinService.GetModelParserAsync();

            var models = modelParser.GetInterfaceDescendants(digitalTwinService.SiteAdtSettings.DeviceModelIds);

            var query = (AdxQueryBuilder.Create()
                .Select(AdxConstants.ActiveTwinsFunction)
                .Where()
                .Property("SiteId", siteId.ToString()) as IAdxQueryFilterGroup)
                .And()
                .PropertyIn("ModelId", models.Select(x => x.Key));

            if (exactMatchFilters != null && exactMatchFilters.Any())
            {
                foreach (var filter in exactMatchFilters)
                    query.And().Property(filter.Key, filter.Value);
            }

            if (includePoints)
                (query as IAdxQuerySelector)
                    .Join(
                        AdxQueryBuilder.Create()
                            .Select(AdxConstants.ActiveRelationshipsFunction)
                            .Where()
                            .PropertyIn("Name", new List<string> { Relationships.HostedBy })
                            .GetQuery(),
                        "Id",
                        "TargetId",
                        "leftouter")
                    .Join(AdxConstants.ActiveTwinsFunction, "SourceId", "Id", "leftouter");

            (query as IAdxQuerySelector).Summarize();
            if (includePoints)
                query.SetProperty("Points").MakeSet(true, "Raw2");

            query.TakeAny(false, "Raw").By("Id");

            var grouped = new List<(Twin, List<Twin>)>();
            using var reader = await _adxHelper.Query(digitalTwinService, query.GetQuery());

            while (reader.Read())
            {
                var device = JsonConvert.DeserializeObject<Twin>(reader["Raw"].ToString(), new TwinJsonConverter());
                var points = new List<Twin>();
                if (includePoints)
                {
                    var pointsArray = reader["Points"] as JArray;
                    if (pointsArray.HasValues)
                    {
                        var twins = pointsArray.Select(x => JsonConvert.DeserializeObject<Twin>(x.ToString(), new TwinJsonConverter()));
                        points.AddRange(twins.Where(x => x != null));
                    }
                }
                grouped.Add((device, points));
            }

            var mapped = grouped.AsParallel().Select(t =>
            {
                var device = MapDeviceAsync(siteId, digitalTwinService, modelParser, t.Item1, false).Result;
                device.Points = t.Item2.Select(x => MapAdxPoint(siteId, modelParser.GetInterface(x.Metadata.ModelId), (x, new List<Twin>(), new List<Twin>()))).ToList();
                return device;
            });

            return await Task.FromResult(mapped.ToList());
        }

        private IAdtQueryFilterGroup GetDevicesQuery(Guid siteId, IDigitalTwinService digitalTwinService, Func<IAdtQuerySelector, IAdtQueryFrom> pickSelector)
        {
            var queryBuilder = pickSelector(AdtQueryBuilder.Create());

            return queryBuilder
                .FromDigitalTwins()
                .Where()
                .WithAnyModel(digitalTwinService.SiteAdtSettings.DeviceModelIds)
                .And()
                .WithStringProperty("siteID", siteId.ToString());
        }

        public async Task<List<Device>> GetDevicesAsync(Guid siteId, bool? includePoints)
        {
            return await GetSiteAdxDevicesAsync(siteId, includePoints.HasValue && includePoints.Value);
        }

        public async Task<Device> GetDeviceByUniqueIdAsync(Guid siteId, Guid id, bool? includePoints)
        {
            var digitalTwinService = await _digitalTwinServiceFactory.GetForSiteAsync(siteId);

            try
            {
                var query = AdtQueryBuilder.Create()
                    .SelectSingle()
                    .FromDigitalTwins()
                    .Where()
                    .WithStringProperty("uniqueID", id.ToString())
                    .GetQuery();

                var twin = await _adtApiService.QueryTwins<BasicDigitalTwin>(digitalTwinService.SiteAdtSettings.InstanceSettings, query).SingleOrDefaultAsync();

                if (twin == null)
                {
                    throw new ResourceNotFoundException(DeviceResourceType, id);
                }

                var modelParser = await digitalTwinService.GetModelParserAsync();

                return await MapDeviceAsync(siteId, digitalTwinService, modelParser, Twin.MapFrom(twin), includePoints);
            }
            catch (DigitalTwinCoreException)
            {
                throw new ResourceNotFoundException(DeviceResourceType, id);
            }
        }

        public async Task<Device> GetDeviceByExternalPointIdAsync(Guid siteId, string externalPointId)
        {
            var digitalTwinService = await _digitalTwinServiceFactory.GetForSiteAsync(siteId);

            var query = GetDevicesQuery(siteId, digitalTwinService, x => x.SelectSingle())
                .And()
                .WithStringProperty("externalID", externalPointId.ToString())
                .GetQuery();

            var twins = _adtApiService.QueryTwins<BasicDigitalTwin>(digitalTwinService.SiteAdtSettings.InstanceSettings, query);
            var twin = await twins.SingleOrDefaultAsync();

            if (twin == null)
                return null;

            var modelParser = await digitalTwinService.GetModelParserAsync();

            return await MapDeviceAsync(siteId, digitalTwinService, modelParser, Twin.MapFrom(twin), true);

        }

        public async Task<Device> UpdateDeviceMetadataAsync(Guid siteId, Guid deviceId, DeviceMetadataDto device)
        {
            var digitalTwinService = await _digitalTwinServiceFactory.GetForSiteAsync(siteId);
            var modelParser = await digitalTwinService.GetModelParserAsync();

            var existingTwin = await digitalTwinService.GetTwinByUniqueIdAsync(deviceId);
            if (existingTwin == null || !modelParser.IsDescendantOfAny(digitalTwinService.SiteAdtSettings.DeviceModelIds, existingTwin.Metadata.ModelId))
            {
                throw new ResourceNotFoundException(DeviceResourceType, deviceId);
            }

            //TODO: Model BACnet device address
            existingTwin.CustomProperties["address"] = device.Address;
            var outputTwin = await digitalTwinService.AddOrUpdateTwinAsync(existingTwin);

            return await MapDeviceAsync(siteId, digitalTwinService, modelParser, outputTwin, true);
        }

        public async Task<Page<Device>> GetDevicesByConnectorAsync(Guid siteId, Guid connectorId, bool? includePoints, string continuationToken = null)
        {
            var digitalTwinService = await _digitalTwinServiceFactory.GetForSiteAsync(siteId);

            var query = GetDevicesQuery(siteId, digitalTwinService, x => x.SelectAll())
                .And()
                .WithStringProperty("connectorID", connectorId.ToString())
                .GetQuery();

            var pageable = _adtApiService.QueryTwins<BasicDigitalTwin>(digitalTwinService.SiteAdtSettings.InstanceSettings, query);

            var page = await pageable.AsPages(continuationToken).FirstAsync();

            var devices = new ConcurrentBag<Device>();

            var mapDevices = page.Values.Select(async t => devices.Add(await MapDeviceAsync(siteId, digitalTwinService, await digitalTwinService.GetModelParserAsync(), Twin.MapFrom(t), includePoints)));

            await Task.WhenAll(mapDevices);

            return new Page<Device> { Content = devices, ContinuationToken = page.ContinuationToken };
        }

        public async Task<List<Device>> GetDevicesByConnectorAsync(Guid siteId, Guid connectorId, bool? includePoints)
        {
            try
            {
                return await GetSiteAdxDevicesAsync(siteId, includePoints.HasValue && includePoints.Value, new Dictionary<string, string> { { "ConnectorId", connectorId.ToString() } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errors when getting devices of Connector {ConnectorId} in Site {SiteId}", connectorId, siteId);
                throw;
            }
        }

        private async Task<Device> MapDeviceAsync(
            Guid siteId,
            IDigitalTwinService digitalTwinService,
            IDigitalTwinModelParser modelParser,
            Twin deviceTwin, bool? includePoints)
        {
            var device = new Device
            {
                TwinId = deviceTwin.Id,
                ModelId = deviceTwin.Metadata.ModelId,
                Id = deviceTwin.UniqueId,
                ExternalId = deviceTwin.GetStringProperty(Properties.ExternalID),
                IsDetected = deviceTwin.GetPropertyValue<bool>(Properties.Detected),
                IsEnabled = deviceTwin.GetPropertyValue<bool>(Properties.Enabled),
                Name = deviceTwin.DisplayName,
                Properties = MapAssetProperties(modelParser.GetInterface(deviceTwin.Metadata.ModelId), deviceTwin),
                RegistrationId = deviceTwin.GetStringProperty(Properties.RegistrationID),
                RegistrationKey = deviceTwin.GetStringProperty(Properties.RegistrationKey),
                ConnectorId = MapDeviceConnectorId(deviceTwin)
            };

            if (!includePoints.HasValue || !includePoints.Value)
                return device;

            var query = AdtQueryBuilder.Create()
                    .Select("source").
                    FromDigitalTwins("source")
                    .JoinRelated("target", "source", Relationships.HostedBy)
                    .Where()
                    .WithStringProperty("target.$dtId", deviceTwin.Id)
                    .And()
                    .WithAnyModel(digitalTwinService.SiteAdtSettings.PointModelIds, "source")
                    .GetQuery();

            var pointTwins = (await _adtApiService.QueryTwins<Dictionary<string, BasicDigitalTwin>>(digitalTwinService.SiteAdtSettings.InstanceSettings, query).ToListAsync())
                .SelectMany(x => x.Values)
                .Select(x => TwinWithRelationships.MapFrom(x))
                .ToList();

            device.Points = await MapPoints(siteId, modelParser, pointTwins, digitalTwinService, false, false);

            return device;
        }

        private Guid? MapDeviceConnectorId(Twin deviceTwin)
        {
            if (deviceTwin.CustomProperties.ContainsKey(Properties.ConnectorID))
            {
                if (Guid.TryParse(deviceTwin.GetStringProperty(Properties.ConnectorID), out var connectorId))
                {
                    return connectorId;
                }
            }
            return null;
        }
        #endregion

        #region Points
        public async Task<Point> GetPointByUniqueIdAsync(Guid siteId, Guid pointId)
        {
            var digitalTwinService = await _digitalTwinServiceFactory.GetForSiteAsync(siteId);
            var query = AdtQueryBuilder.Create().SelectSingle().FromDigitalTwins().Where().WithStringProperty("uniqueID", pointId.ToString()).GetQuery();
            var point = await _adtApiService.QueryTwins<BasicDigitalTwin>(digitalTwinService.SiteAdtSettings.InstanceSettings, query).SingleOrDefaultAsync();

            if (point == null)
                return null;

            var modelParser = await digitalTwinService.GetModelParserAsync();

            return await MapPoint(siteId, modelParser.GetInterface(point.Metadata.ModelId), Twin.MapFrom(point), digitalTwinService);
        }

        public async Task<Point> GetPointByTrendIdAsync(Guid siteId, Guid trendId)
        {
            var page = await GetSitePointsAsync(siteId, filters: new Dictionary<string, List<string>> { { "trendID", new List<string> { trendId.ToString() } } });
            return page.Content.SingleOrDefault();
        }

        public async Task<List<Point>> GetPointsByTrendIdsAsync(Guid siteId, List<Guid> trendIds)
        {
            var filters = new Dictionary<string, List<string>> { { "trendID", trendIds.Select(x => x.ToString()).ToList() } };

            var page = await GetSitePointsAsync(siteId, filters: filters);

            var points = await page.FetchAll<Point>(x => GetSitePointsAsync(siteId, x, filters: filters));

            return points.ToList();
        }

        public async Task<Point> GetPointByExternalIdAsync(Guid siteId, string externalId)
        {
            var page = await GetSitePointsAsync(siteId, filters: new Dictionary<string, List<string>> { { "externalID", new List<string> { externalId.ToString() } } });
            return page.Content.SingleOrDefault();
        }

        public async Task<Point> GetPointByTwinIdAsync(Guid siteId, string twinId)
        {
            var page = await GetSitePointsAsync(siteId, filters: new Dictionary<string, List<string>> { { "$dtId", new List<string> { twinId.ToString() } } });

            return page.Content.SingleOrDefault();
        }

        public async Task<List<Point>> GetPointsByTagAsync(Guid siteId, string tag)
        {
            return await GetSiteAdxPointsAsync(siteId, true, containsFilters: new Dictionary<string, string> { { "Tags", tag } });
        }

        public async Task<Page<Point>> GetPointsByTagAsync(Guid siteId, string tag, string continuationToken = null)
        {
            return await GetSitePointsAsync(siteId, continuationToken, checkDefinedProperties: new List<string> { $"tags.{tag}" });
        }

        public async Task<List<(TwinWithRelationships PointTwin, Point Point, Asset Asset)>> GetPointAssetPairsByPointIdsAsync(
            Guid siteId,
            List<Guid> pointUniqIds,
            List<string> pointExternalIds,
            List<Guid> pointTrendIds,
            bool includePointsWithNoAssets = false)
        {
            var digitalTwinService = await _digitalTwinServiceFactory.GetForSiteAsync(siteId);
            var modelParser = await digitalTwinService.GetModelParserAsync();
            var reader = await GetPointAssetPairsByPointIdsReaderAsync(siteId, pointUniqIds, pointExternalIds, pointTrendIds,
                x => x.SetProperty("Assets")
                .MakeSet(true, "Raw2")
                .TakeAny(false, "Raw")
                .By("Id").GetQuery());

            var pointsPairs = new List<(TwinWithRelationships, Point, Asset)>();

            while (reader.Read())
            {
                var twin = JsonConvert.DeserializeObject<Twin>(reader["Raw"].ToString(), new TwinJsonConverter());
                var assetTwins = new List<Twin>();
                Asset asset = null;
                var assetsArray = reader["Assets"] as JArray;
                if (assetsArray.HasValues)
                {
                    var assets = assetsArray.Where(x => !string.IsNullOrEmpty(x.ToString()));
                    if (assets != null && assets.Any())
                        assetTwins.AddRange(assets.Select(x => JsonConvert.DeserializeObject<Twin>(x.ToString(), new TwinJsonConverter())).Where(x => x != null));
                }
                var point = MapAdxPoint(siteId, modelParser.GetInterface(twin.Metadata.ModelId), (twin, assetTwins, new List<Twin>()));
                var firstAsset = assetTwins.FirstOrDefault();
                if (firstAsset != null)
                    asset = await MapAdxAsset(siteId, digitalTwinService, modelParser, (firstAsset, null, new List<(BasicRelationship, Twin)>(), new List<Twin>(), new Dictionary<string, IEnumerable<Twin>>()));

                if (includePointsWithNoAssets || asset != null)
                    pointsPairs.Add((new TwinWithRelationships
                    {
                        Id = twin.Id,
                        CustomProperties = Twin.MapCustomProperties(twin.CustomProperties),
                        Metadata = twin.Metadata
                    }, point, asset));
            }

            return pointsPairs;
        }

        public async Task<IDataReader> GetPointAssetPairsByPointIdsReaderAsync(
            Guid siteId,
            List<Guid> pointUniqIds,
            List<string> pointExternalIds,
            List<Guid> pointTrendIds,
            Func<IAdxQueryFilterGroup, string> getCustomQuery,
            bool includePointsWithNoAssets = false)
        {
            var digitalTwinService = await _digitalTwinServiceFactory.GetForSiteAsync(siteId);
            var modelParser = await digitalTwinService.GetModelParserAsync();
            var models = modelParser.GetInterfaceDescendants(digitalTwinService.SiteAdtSettings.PointModelIds);

            var query = AdxQueryBuilder.Create()
                .Select(AdxConstants.ActiveTwinsFunction)
                .Where()
                .PropertyIn("ModelId", models.Select(x => x.Key));

            var anyUniqueId = pointUniqIds != null && pointUniqIds.Any();
            var anyExternalId = pointExternalIds != null && pointExternalIds.Any();
            var anyTrendId = pointTrendIds != null && pointTrendIds.Any();
            var anyFilter = anyExternalId || anyTrendId || anyUniqueId;

            if (anyFilter)
                query.And().OpenGroupParentheses();

            if (anyUniqueId)
                query.PropertyIn("UniqueId", pointUniqIds.Select(x => x.ToString()));

            if (anyTrendId)
            {
                if (anyUniqueId)
                    query.Or();
                query.PropertyIn("TrendId", pointTrendIds.Select(x => x.ToString()));
            }

            if (anyExternalId)
            {
                if (anyUniqueId || anyTrendId)
                    query.Or();
                query.PropertyIn("ExternalId", pointExternalIds);
            }

            if (anyFilter)
                query.CloseGroupParentheses();

            (query as IAdxQuerySelector)
                .Join(
                    AdxQueryBuilder.Create()
                        .Select(AdxConstants.ActiveRelationshipsFunction)
                        .Where()
                        .PropertyIn("Name", new List<string> { Relationships.IsCapabilityOf })
                        .GetQuery(),
                    "Id",
                    "SourceId",
                    "leftouter")
                .Join(AdxConstants.ActiveTwinsFunction, "TargetId", "Id", "leftouter")
                .Summarize();

            return await _adxHelper.Query(digitalTwinService, getCustomQuery(query));
        }

        public async Task<IEnumerable<LiveDataIngestPointDto>> GetSimplePointAssetPairsByPointIdsAsync(
            Guid siteId,
            List<Guid> pointUniqIds,
            List<string> pointExternalIds,
            List<Guid> pointTrendIds,
            bool includePointsWithNoAssets = false)
        {
            var pointsPairs = new List<LiveDataIngestPointDto>();
            var reader = await GetPointAssetPairsByPointIdsReaderAsync(siteId, pointUniqIds, pointExternalIds, pointTrendIds,
                x => x.SetProperty("Assets")
                .MakeSet(false, "UniqueId1")
                .By("UniqueId", "TrendId", "ExternalId").GetQuery());

            while (reader.Read())
            {
                var pointsPair = new LiveDataIngestPointDto { AssetId = Guid.Empty };

                pointsPair.ExternalId = reader["ExternalId"] as string;
                var assetsArray = reader["Assets"] as JArray;
                if (assetsArray.HasValues)
                {
                    var assetUniqueId = assetsArray.FirstOrDefault(x => !string.IsNullOrEmpty(x.ToString()));
                    if (assetUniqueId != null)
                        pointsPair.AssetId = Guid.TryParse(assetUniqueId.ToString(), out var parsed) ? parsed : Guid.Empty;
                }

                pointsPair.UniqueId = Guid.Parse(reader["UniqueId"].ToString());
                if (reader["TrendId"] != null)
                    pointsPair.TrendId = Guid.Parse(reader["TrendId"].ToString());

                if (includePointsWithNoAssets || pointsPair.AssetId != Guid.Empty)
                    pointsPairs.Add(pointsPair);
            }

            return pointsPairs;
        }

        public async Task<Page<(TwinWithRelationships PointTwin, Point Point, Asset Asset)>> GetPointAssetPairsByPointIdsAsync(
        Guid siteId,
        List<Guid> pointUniqIds,
        List<string> pointExternalIds,
        List<Guid> pointTrendIds,
        bool includePointsWithNoAssets = false,
        string continuationToken = null)
        {
            var digitalTwinService = await _digitalTwinServiceFactory.GetForSiteAsync(siteId);

            var qb = AdtQueryBuilder.Create()
                .SelectAll()
                .FromDigitalTwins()
                .Where()
                .OpenGroupParenthesis();

            var anyUniques = pointUniqIds != null && pointUniqIds.Count > 0;
            var anyExternals = pointExternalIds != null && pointExternalIds.Count > 0;
            var anyTrends = pointTrendIds != null && pointTrendIds.Count > 0;

            if (anyUniques)
            {
                qb = qb.WithPropertyIn("uniqueID", pointUniqIds.Select(u => u.ToString()));
            }
            if (anyExternals)
            {
                if (anyUniques)
                    qb = qb.Or();

                qb = qb.WithPropertyIn("externalID", pointExternalIds);
            }
            if (anyTrends)
            {
                if (anyUniques || anyExternals)
                    qb = qb.Or();

                qb = qb.WithPropertyIn("trendID", pointTrendIds.Select(e => e.ToString()));
            }

            var query = qb.CloseGroupParenthesis()
                .And()
                .WithAnyModel(digitalTwinService.SiteAdtSettings.PointModelIds)
                .GetQuery();

            var pageable = _adtApiService.QueryTwins<BasicDigitalTwin>(digitalTwinService.SiteAdtSettings.InstanceSettings, query);
            var azurePage = await pageable.AsPages(continuationToken).FirstAsync();

            var twins = azurePage.Values.Select(t => TwinWithRelationships.MapFrom(t));

            var pointAssetTasks = new Dictionary<string, Task<List<TwinWithRelationships>>>();
            foreach (var pointTwin in twins)
            {
                var pointAssetQuery = AdtQueryBuilder.Create()
                    .Select("Asset")
                    .FromDigitalTwins()
                    .Match(new string[] { "isCapabilityOf" }, "Point", "Asset")
                    .Where()
                    .WithStringProperty("Point.$dtId", pointTwin.Id)
                    .GetQuery();
                pointAssetTasks.Add(pointTwin.Id, digitalTwinService.GetTwinsByQueryAsync(pointAssetQuery, "Asset"));
            }

            var modelParser = await digitalTwinService.GetModelParserAsync();
            var disabledPoints = new List<string>();
            var noAssetPoints = new List<string>();
            var multipleAssetPoints = new List<(string, string[])>();
            var mapAssetTasks = new List<(TwinWithRelationships, Point, Task<Asset>)>();
            var result = new List<(TwinWithRelationships, Point, Asset)>();
            foreach (var pointTwin in twins)
            {
                var pointAssets = await pointAssetTasks[pointTwin.Id];
                if (pointAssets.Count == 0)
                {
                    noAssetPoints.Add(pointTwin.Id);
                }
                else if (pointAssets.Count > 1)
                {
                    multipleAssetPoints.Add((pointTwin.Id, pointAssets.Select(c => c.Id).ToArray()));
                }

                var point = await MapPoint(siteId, modelParser.GetInterface(pointTwin.Metadata.ModelId), pointTwin, digitalTwinService, false, false);

                if (point.IsEnabled == false)
                {
                    disabledPoints.Add(pointTwin.Id);
                }
                else if (pointAssets.Count == 0 && includePointsWithNoAssets)
                {
                    result.Add((pointTwin, point, null));
                }
                else if (pointAssets.Count > 0)
                {
                    // Note it's possible to have >1 twin associated with a CapabilityOf - we're only returning the first one here
                    var assetTwin = pointAssets.First();
                    // Map asset will make another call to get floor, better do it async.
                    mapAssetTasks.Add((pointTwin, point, MapAsset(digitalTwinService, assetTwin, modelParser, null, new List<Point>())));
                }
            }

            foreach (var (twin, point, task) in mapAssetTasks)
            {
                var asset = await task;
                result.Add((twin, point, asset));
            }

            if (disabledPoints.Any())
            {
                _logger?.LogInformation("The following points were disabled {disabledPoints}",
                    JsonConvert.SerializeObject(disabledPoints));
            }
            if (noAssetPoints.Any())
            {
                _logger?.LogWarning("No related 'asset' twins found via isCapabilityOf for the following twins: {twins}",
                    JsonConvert.SerializeObject(noAssetPoints));
            }
            if (multipleAssetPoints.Any())
            {
                _logger?.LogWarning("Multiple 'asset' twins  found via isCapabilityOf - only first of each  will currently be used: {multipleTwins}",
                    JsonConvert.SerializeObject(multipleAssetPoints));
            }

            return new Page<(TwinWithRelationships, Point, Asset)> { Content = result, ContinuationToken = azurePage.ContinuationToken };
        }

        public async Task<List<Point>> GetPointsAsync(Guid siteId, bool includeAssets, int startItemIndex = 0, int pageSize = int.MaxValue)
        {
            return await GetSiteAdxPointsAsync(siteId, includeAssets, startItemIndex: startItemIndex, pageSize: pageSize);
        }

        private async Task<List<Point>> GetSiteAdxPointsAsync(Guid siteId,
            bool includeAssets,
            Dictionary<string, string> exactMatchFilters = null,
            Dictionary<string, string> containsFilters = null,
            int startItemIndex = 0,
            int pageSize = int.MaxValue)
        {
            var digitalTwinService = await _digitalTwinServiceFactory.GetForSiteAsync(siteId);
            var modelParser = await digitalTwinService.GetModelParserAsync();

            var models = modelParser.GetInterfaceDescendants(digitalTwinService.SiteAdtSettings.PointModelIds);
            var relationships = new List<string> { Relationships.HostedBy };
            if (includeAssets)
                relationships.Add(Relationships.IsCapabilityOf);

            var query = (AdxQueryBuilder.Create()
                .Select(AdxConstants.ActiveTwinsFunction)
                .Where()
                .Property("SiteId", siteId.ToString()) as IAdxQueryFilterGroup)
                .And()
                .PropertyIn("ModelId", models.Select(x => x.Key));

            if (exactMatchFilters != null && exactMatchFilters.Any())
            {
                foreach (var filter in exactMatchFilters)
                    (query as IAdxQueryFilterGroup).And().Property(filter.Key, filter.Value);
            }

            if (containsFilters != null && containsFilters.Any())
            {
                foreach (var filter in containsFilters)
                    (query as IAdxQueryFilterGroup).And().Contains(filter.Key, filter.Value);
            }

            (query as IAdxQuerySelector)
                .Join(
                    AdxQueryBuilder.Create()
                        .Select(AdxConstants.ActiveRelationshipsFunction)
                        .Where()
                        .PropertyIn("Name", relationships)
                        .GetQuery(),
                    "Id",
                    "SourceId",
                    "leftouter")
                .Join(AdxConstants.ActiveTwinsFunction, "TargetId", "Id", "leftouter")
                .Summarize();

            (query as IAdxQueryFilterGroup)
                .SetProperty("Relation")
                .MakeSet(true, "Raw2")
                .TakeAny(false, "Raw")
                .By("Id", "Name1");

            (query as IAdxQuerySelector).Summarize();

            query.SetProperty("Relations").MakeBag(true, "pack(Name1,Relation)")
                .TakeAny(false, "Raw")
                .By("Id");

            var grouped = new List<(Twin, List<Twin>, List<Twin>)>();
            using var reader = await _adxHelper.Query(digitalTwinService, query.GetQuery());
            var counter = 0;

            while (reader.Read())
            {
                // Map only a single page for performance reasons.
                if (grouped.Count >= pageSize)
                {
                    break;
                }

                if (counter >= startItemIndex)
                {
                    var point = JsonConvert.DeserializeObject<Twin>(reader["Raw"].ToString(), new TwinJsonConverter());
                    var devices = new List<Twin>();
                    var assets = new List<Twin>();
                    var packed = reader["Relations"] as JObject;

                    if (packed != null && packed.HasValues)
                    {
                        var assetsList = packed[Relationships.IsCapabilityOf];
                        if (assetsList != null)
                        {
                            assets.AddRange(assetsList.Select(x => JsonConvert.DeserializeObject<Twin>(x.ToString(), new TwinJsonConverter())));
                        }

                        var devicesList = packed[Relationships.HostedBy];
                        if (devicesList != null)
                        {
                            devices.AddRange(devicesList.Select(x => JsonConvert.DeserializeObject<Twin>(x.ToString(), new TwinJsonConverter())));
                        }
                    }

                    grouped.Add((point, assets, devices));
                }

                counter++;
            }

            return await MapAdxPoints(siteId, modelParser, grouped);
        }

        public async Task<Page<Point>> GetPointsAsync(Guid siteId, bool includeAssets, string continuationToken = null)
        {
            return await GetSitePointsAsync(siteId, continuationToken, includeAssets: includeAssets);
        }

        public async Task<List<Point>> GetPointsByConnectorAsync(Guid siteId, Guid connectorId, bool includeAssets = true)
        {
            return await GetSiteAdxPointsAsync(siteId, includeAssets, new Dictionary<string, string> { { "ConnectorId", connectorId.ToString() } });
        }

        public async Task<Page<Point>> GetPointsByConnectorAsync(Guid siteId, Guid connectorId, bool includeAssets = true, string continuationToken = null)
        {
            return await GetSitePointsAsync(siteId, continuationToken, new Dictionary<string, List<string>> { { "connectorID", new List<string> { connectorId.ToString() } } }, includeAssets: includeAssets);
        }

        public async Task<int> GetPointsCountAsync(Guid siteId)
        {
            var digitalTwinService = await _digitalTwinServiceFactory.GetForSiteAsync(siteId);
            var query = GetPointsQuery(siteId, digitalTwinService.SiteAdtSettings, x => x.SelectCount())
                .And()
                .WithBoolProperty("enabled", true)
                .GetQuery();

            return await GetCountAsync(query, digitalTwinService.SiteAdtSettings.InstanceSettings);
        }

        public async Task<int> GetPointsByConnectorCountAsync(Guid siteId, Guid connectorId)
        {
            var digitalTwinService = await _digitalTwinServiceFactory.GetForSiteAsync(siteId);
            var query = GetPointsQuery(siteId, digitalTwinService.SiteAdtSettings, x => x.SelectCount(), new Dictionary<string, List<string>> { { "connectorID", new List<string> { connectorId.ToString() } } })
                .And()
                .WithBoolProperty("enabled", true)
                .GetQuery();

            return await GetCountAsync(query, digitalTwinService.SiteAdtSettings.InstanceSettings);
        }

        public async Task<int> GetCountAsync(string query, AzureDigitalTwinsSettings settings)
        {
            var pageable = _adtApiService.QueryTwins<CountResult>(settings, query);

            var page = await pageable.AsPages().SingleAsync();

            return page.Values.Single().COUNT;
        }

        private async Task<List<Point>> GetPointsSimpleAsync(
            IDigitalTwinService digitalTwinService,
            string twinId)
        {
            try
            {
                var incomingRelationships = await digitalTwinService.GetIncomingRelationshipsAsync(twinId);

                var modelParser = await digitalTwinService.GetModelParserAsync();

                var pointTwins = incomingRelationships
                    .Where(i =>
                        (i.Name == Relationships.HostedBy || i.Name == Relationships.IsCapabilityOf)
                            && modelParser.IsDescendantOfAny(digitalTwinService.SiteAdtSettings.PointModelIds, i.Source.Metadata.ModelId))
                    .Select(i => i.Source)
                    .ToList();

                return pointTwins.Select(p => new Point { Id = p.UniqueId, Name = p.DisplayName, Tags = new List<Tag>() }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error mapping points to asset for twin {TwinId}", twinId);
            }
            return new List<Point>();
        }

        private IAdtQueryFilterGroup GetPointsQuery(Guid siteId, SiteAdtSettings siteAdtSettings, Func<IAdtQuerySelector, IAdtQueryFrom> pickSelector, Dictionary<string, List<string>> filters = null, List<string> checkDefinedProperties = null)
        {
            var queryBuilder = pickSelector(AdtQueryBuilder.Create())
                .FromDigitalTwins()
                .Where()
                .WithAnyModel(siteAdtSettings.PointModelIds)
                .And()
                .WithStringProperty("siteID", siteId.ToString());

            if (filters != null && filters.Any())
            {
                foreach (var filter in filters)
                    queryBuilder.And().WithPropertyIn(filter.Key, filter.Value);
            }

            if (checkDefinedProperties != null && checkDefinedProperties.Any())
                queryBuilder.And().CheckDefined(checkDefinedProperties);

            return queryBuilder;
        }

        private async Task<List<Point>> GetAssetPoints(IDigitalTwinService digitalTwinService, string assetId, bool includeAssets = false, bool includeDevices = false)
        {
            try
            {
                var query = $"select Point from DIGITALTWINS match (Point)-[:isCapabilityOf|hostedBy]->(Asset) where Asset.$dtId='{assetId}'";
                var twins = await digitalTwinService.GetTwinsByQueryAsync(query, "Point");

                var points = await MapPoints(
                    digitalTwinService.SiteAdtSettings.SiteId,
                    await digitalTwinService.GetModelParserAsync(),
                    twins,
                    digitalTwinService,
                    includeAssets,
                    includeDevices);

                return points;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot get capabilities of {AssetId}.", assetId);
                return new List<Point>();
            }
        }

        public async Task<List<Point>> GetAssetPointsAsync(Guid siteId, Guid assetId)
        {
            var digitalTwinService = await _digitalTwinServiceFactory.GetForSiteAsync(siteId);

            try
            {
                var twin = await digitalTwinService.GetTwinByUniqueIdAsync(assetId);
                if (twin == null)
                    throw new ResourceNotFoundException(AssetResourceType, assetId);

                var points = await GetAssetPoints(digitalTwinService, twin.Id, true, true);

                return points;
            }
            catch (DigitalTwinCoreException)
            {
                throw new ResourceNotFoundException(AssetResourceType, assetId);
            }
        }

        private async Task<Page<Point>> GetSitePointsAsync(Guid siteId, string continuationToken = null, Dictionary<string, List<string>> filters = null, List<string> checkDefinedProperties = null, bool includeAssets = true)
        {
            var digitalTwinService = await _digitalTwinServiceFactory.GetForSiteAsync(siteId);

            var query = GetPointsQuery(siteId, digitalTwinService.SiteAdtSettings, x => x.SelectAll(), filters, checkDefinedProperties).GetQuery();

            var pageable = _adtApiService.QueryTwins<BasicDigitalTwin>(digitalTwinService.SiteAdtSettings.InstanceSettings, query);

            var page = await pageable.AsPages(continuationToken).FirstAsync();

            var points = await MapPoints(siteId, await digitalTwinService.GetModelParserAsync(), page.Values.Select(x => Twin.MapFrom(x)).ToList(), digitalTwinService, includeAssets);

            return new Page<Point> { Content = points, ContinuationToken = page.ContinuationToken };
        }

        private async Task<List<Point>> MapPoints<T>(Guid siteId, IDigitalTwinModelParser modelParser, List<T> twins, IDigitalTwinService digitalTwinService, bool includeAssets = true, bool includeDevices = true) where T : Twin
        {
            var mapped = twins.AsParallel().Select(t =>
                MapPoint(siteId, modelParser.GetInterface(t.Metadata.ModelId),
                        t, digitalTwinService, includeAssets, includeDevices)
                    .Result);
            return await Task.FromResult(mapped.ToList());
        }

        private async Task<Point> MapPoint<T>(Guid siteId, DTInterfaceInfo interfaceInfo, T twin, IDigitalTwinService digitalTwinService, bool includeAssets = true, bool includeDevices = true) where T : Twin
        {
            PointValue currentValue = null;
            if (interfaceInfo.Contents.ContainsKey(Properties.LivedataLastValue))
            {
                var currentValueProperty = interfaceInfo.Contents[Properties.LivedataLastValue] as DTPropertyInfo;
                if (currentValueProperty != null)
                {
                    string unit = null;
                    if (currentValueProperty.SupplementalProperties.ContainsKey(SupplementalProperties.Unit))
                    {
                        unit = currentValueProperty.SupplementalProperties[SupplementalProperties.Unit] as string;
                    }
                    currentValue = new PointValue
                    {
                        Unit = unit,
                        Value = twin.GetProperty<object>(Properties.LivedataLastValue)
                    };
                }
            }

            Guid? trendId = twin.UniqueId;
            if (Guid.TryParse(twin.GetStringProperty(Properties.TrendID), out var trendIdParsed))
            {
                trendId = trendIdParsed;
            }

            // Note that the trendInterval comes into the service as a decimal, but is reported as a TineSpan
            var trendIntervalValue = twin.GetPropertyValue<decimal>(Properties.TrendInterval);

            return new Point
            {
                ModelId = twin.Metadata.ModelId,
                TwinId = twin.Id,
                Id = twin.UniqueId,
                TrendId = trendId.GetValueOrDefault(),
                ExternalId = twin.GetStringProperty(Properties.ExternalID),
                Name = twin.DisplayName,
                Description = twin.GetStringProperty(Properties.Description),
                Type = MapPointType(twin.GetStringProperty(Properties.Type)),
                Tags = _tagMapperService.MapTags(siteId, twin.Metadata.ModelId, twin.GetObjectProperty(Properties.Tags)),
                DisplayPriority = twin.GetPropertyValue<decimal>(Properties.DisplayPriority),
                DisplayName = interfaceInfo.GetDisplayName(),
                CurrentValue = currentValue,
                Assets = await GetTwinsFromRelationshipAsync(siteId, includeAssets, Relationships.IsCapabilityOf, twin.Id),
                Devices = await GetTwinsFromRelationshipAsync(siteId, includeDevices, Relationships.HostedBy, twin.Id),
                CategoryName = interfaceInfo.GetDisplayName(),
                IsDetected = twin.GetPropertyValue<bool>(Properties.Detected),
                IsEnabled = twin.GetPropertyValue<bool>(Properties.Enabled),
                TrendInterval = trendIntervalValue.HasValue ? TimeSpan.FromSeconds((double)trendIntervalValue.Value) : (TimeSpan?)null,
                Properties = MapAssetProperties(interfaceInfo, twin),
                Communication = MapPointCommunication(twin)
            };
        }

        private async Task<List<Twin>> GetTwinsFromRelationshipAsync(Guid siteId, bool include, string relationshipName, string twinId)
        {
            if (!include)
                return new List<Twin>();

            var query = AdxQueryBuilder.Create().Select(AdxConstants.ActiveTwinsFunction)
                .Where()
                .Property("Id", twinId);

            (query as IAdxQuerySelector)
                .Join(
                    AdxQueryBuilder.Create()
                        .Select(AdxConstants.ActiveRelationshipsFunction)
                        .Where()
                        .Property("Name", relationshipName)
                        .GetQuery(),
                    "Id",
                    "SourceId",
                    "leftouter")
                .Join(AdxConstants.ActiveTwinsFunction, "TargetId", "Id", "leftouter")
                .Project("Raw2");

            var digitalTwinService = await _digitalTwinServiceFactory.GetForSiteAsync(siteId);

            using var reader = await _adxHelper.Query(digitalTwinService, query.GetQuery());
            var twins = new List<Twin>();
            while (reader.Read())
            {
                twins.Add(JsonConvert.DeserializeObject<Twin>(reader["Raw2"].ToString(), new TwinJsonConverter()));
            }

            return twins;
        }

        private PointCommunication MapPointCommunication(Twin twin)
        {
            var communication = twin.GetObjectProperty(Properties.Communication);
            return (communication != null) ? PointCommunication.MapFromComponent(communication) : null;
        }

        private static PointType MapPointType(string type)
        {
            return ((type ?? "").ToUpperInvariant()) switch
            {
                Dtos.PointTypes.Analog => PointType.Analog,
                Dtos.PointTypes.Binary => PointType.Binary,
                Dtos.PointTypes.MultiState => PointType.MultiState,
                Dtos.PointTypes.Sum => PointType.Sum,
                _ => PointType.Undefined,
            };
        }
        #endregion

        #region Building
        private async Task<TwinWithRelationships> GetBuilding(Guid siteId)
        {
            return await _memoryCache.GetOrCreateAsync($"Building_{siteId}", async (c) =>
            {
                c.SetPriority(CacheItemPriority.NeverRemove);
                c.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(12);

                var digitalTwinService = await _digitalTwinServiceFactory.GetForSiteAsync(siteId);
                return await digitalTwinService.GetTwinByUniqueIdAsync(siteId);
            });
        }

        private async Task<List<TwinWithRelationships>> GetFloors(Guid siteId)
        {
            return await _memoryCache.GetOrCreateAsync($"Floors_{siteId}", async (c) =>
            {
                c.SetPriority(CacheItemPriority.NeverRemove);
                c.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);

                var digitalTwinService = await _digitalTwinServiceFactory.GetForSiteAsync(siteId);

                var query = AdtQueryBuilder.Create()
                    .SelectAll()
                    .FromDigitalTwins()
                    .Where()
                    .WithStringProperty("siteID", siteId.ToString().ToLower())
                    .And()
                    .WithAnyModel(new string[] { WillowInc.LevelModelId })
                    .GetQuery();

                var floors = await digitalTwinService.GetTwinsByQueryAsync(query);

                return floors;
            });
        }

        #endregion

        #region History

        public async Task<TwinHistoryDto> GetTwinHistory(Guid siteId, string twinId)
        {
            var queryBuilder = ((AdxQueryBuilder.Create()
                .Select(AdxConstants.TwinsTable)
                .Where()
                .Property("SiteId", siteId.ToString()) as IAdxQueryFilterGroup)
                .And()
                .Property("Id", twinId) as IAdxQuerySelector)
                .Project("Raw", "UserId", "ExportTime");
            string query = (queryBuilder as IAdxQueryFilterGroup).Sort("ExportTime", true).GetQuery();

            TwinHistoryDto twinHistory = null;
            var twinVersions = new List<TwinVersionDto>();
            var users = new Dictionary<string, UserDto>();
            var digitalTwinService = await _digitalTwinServiceFactory.GetForSiteAsync(siteId);
            using var reader = await _adxHelper.Query(digitalTwinService, query);
            while (reader.Read())
            {
                var jObject = reader["Raw"] as JObject;
                var twin = new TwinAdx
                {
                    Id = jObject["id"].ToString(),
                    Raw = jObject["customProperties"].ToString(),
                    Metadata = new TwinMetadata { ModelId = jObject["modelId"].ToString() }
                };
                var userId = reader["UserId"] as string;
                UserDto userDto = null;
                if(!string.IsNullOrWhiteSpace(userId) && !users.TryGetValue(userId, out userDto))
                {
                    userDto = await _directoryCoreClient.GetUser(Guid.Parse(userId));
                    users.TryAdd(userId, userDto);
                }
                twinVersions.Add(
                    new TwinVersionDto {
                        User = userDto != null ? userDto : null,
                        Timestamp = (DateTime)reader["ExportTime"],
                        Twin = TwinAdxDto.MapFrom(twin)
                    });
            }

            if(twinVersions.Count > 0)
            {
                twinHistory = new TwinHistoryDto() { Versions = twinVersions };
            }

            return twinHistory;
        }

        #endregion

        public async Task<TwinFieldsDto> GetTwinFields(Guid siteId, string twinId)
        {
            var digitalTwinService = await _digitalTwinServiceFactory.GetForSiteAsync(siteId);

            var query = ((((AdxQueryBuilder.Create()
                .Select(AdxConstants.ActiveValidationResults)
                .Where()
                .Property("TwinId", twinId) as IAdxQueryFilterGroup)
                .And()
                .Property("ResultType", "ErrorMissing") as IAdxQueryFilterGroup)
                .And()
                .IsNotEmpty("ResultInfo") as IAdxQuerySelector)
                .Project("ResultInfo") as IAdxQueryFilterGroup).GetQuery();

            using var reader = await _adxHelper.Query(digitalTwinService, query);

            var twinFields = new TwinFieldsDto
            {
                ExpectedFields = new List<string>()
            };

            while (reader.Read())
            {
                var resultInfo = reader["ResultInfo"] as JObject;
                twinFields.ExpectedFields.Add(resultInfo["propertyName"].ToString());
            }

            return twinFields;
        }
       
        public async Task<List<PointTwinDto>> GetPointsByTwinIdsAsync(Guid siteId, List<string> pointTwinIds)
        {
            var pointTwins = new List<PointTwinDto>();
            if (pointTwinIds is null || !pointTwinIds.Any())
            {
                return pointTwins;
            }
            try
            {
                var digitalTwinService = await _digitalTwinServiceFactory.GetForSiteAsync(siteId);
                pointTwinIds = pointTwinIds.Distinct().OrderBy(x => x).ToList();
                const int batchSize = 50;
                var tasks = new List<Task<List<JsonElement>>>();
                for (int i = 0; i < pointTwinIds.Count; i += batchSize)
                {
                    var batch = pointTwinIds.Skip(i).Take(batchSize).ToList();
                    var getPointTask =  GetPointsByTwinIdsBatchesAsync(batch, digitalTwinService.SiteAdtSettings.InstanceSettings);
                    tasks.Add(getPointTask);
                    
                }
                var results = await Task.WhenAll(tasks);

                foreach (var result in results)
                {
                    var points = result.Select(r => JsonConvert.DeserializeObject<PointTwinDto>(r.GetRawText())).ToList();
                    pointTwins.AddRange(points);
                }   
                return pointTwins;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot get trendIds of points in siteId {SiteId}", siteId);
                return pointTwins;
            }
        }
        // to avoid ADT query limit, we need to query points in batches
        private  Task<List<JsonElement>> GetPointsByTwinIdsBatchesAsync(List<string> pointTwinIds, AzureDigitalTwinsSettings instanceSettings)
        {
            var queryParameter = pointTwinIds.Select(x => $"'{x.Escape()}'").ToList();
            var query = $"select $dtId as pointTwinId, trendID, name, externalID, unit from DIGITALTWINS where $dtId in [{string.Join(',', queryParameter)}]";
            var results =  _adtApiService.QueryTwins(instanceSettings, query);
            return results;
        }
    }
}
