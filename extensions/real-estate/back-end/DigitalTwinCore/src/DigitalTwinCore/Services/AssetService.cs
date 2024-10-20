
// Use AsParallel to use multiple cores for CPU-bound processing within a single request
// #define USE_PARALLEL_PROCESSING

using DigitalTwinCore.Constants;
using DigitalTwinCore.Dto;
using DigitalTwinCore.DTO;
using DigitalTwinCore.Exceptions;
using DigitalTwinCore.Models;
using DigitalTwinCore.Models.Connectors;
using DTDLParser;
using DTDLParser.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Willow.Infrastructure.Exceptions;
using TwinWithRelationships = DigitalTwinCore.Models.TwinWithRelationships;

namespace DigitalTwinCore.Services
{
    public interface IAssetService
    {
        Task<Page<Device>> GetDevicesByConnectorAsync(Guid siteId, Guid connectorId, bool? includePoints, string continuationToken = null);
        Task<Page<Device>> GetDevicesAsync(Guid siteId, bool? includePoints, string continuationToken = null);
        Task<Page<Point>> GetPointsByTagAsync(Guid siteId, string tag, string continuationToken = null);
        Task<int> GetPointsByConnectorCountAsync(Guid siteId, Guid connectorId);
        Task<int> GetPointsCountAsync(Guid siteId);
        Task<Page<Point>> GetPointsByConnectorAsync(Guid siteId, Guid connectorId, bool includeAssets = true, string continuationToken = null);
        Task<Page<Point>> GetPointsAsync(Guid siteId, bool includeAssets, string continuationToken = null);
        Task<IEnumerable<LightCategoryDto>> GetCategories(Guid siteId, Guid? floorId, bool isLiveDataOnly);
        Task<List<Category>> GetCategoriesAndAssetsAsync(Guid siteId, bool isCategoryOnly, List<string> modelNames, Guid? floorId = null);
        Task<Page<Category>> GetCategoriesAndAssetsAsync(Guid siteId, bool isCategoryOnly, List<string> modelNames, Guid? floorId = null, string continuationToken = null);
        Task<Asset> GetAssetByUniqueId(Guid siteId, Guid id);
        Task<Asset> GetAssetById(Guid siteId, string id);
        Task<Asset> GetAssetByForgeViewerId(Guid siteId, string forgeViewerId);
        Task<List<Asset>> GetAssetsAsync(Guid siteId,
                                         Guid? categoryId,
                                         Guid? floorId,
                                         string searchKeyword,
                                         bool liveDataOnly = false,
                                         bool includeExtraProperties = false,
                                         int startItemIndex = 0,
                                         int pageSize = int.MaxValue);
        Task<Page<Asset>> GetAssets(Guid siteId,
                                    Guid? categoryId,
                                    Guid? floorId,
                                    string searchKeyword,
                                    bool liveDataOnly = false,
                                    bool includeExtraProperties = false,
                                    string continuationToken = null);
        Task<IEnumerable<AssetNameDto>> GetAssetNames(Guid siteId, IEnumerable<Guid> ids);
        Task<List<Document>> GetDocumentsForAssetAsync(Guid siteId, Guid assetId);
        Task<Point> GetPointByUniqueIdAsync(Guid siteId, Guid pointId);
        Task<Point> GetPointByTrendIdAsync(Guid siteId, Guid trendId);
        Task<List<Point>> GetPointsByTrendIdsAsync(Guid siteId, List<Guid> trendIds);
        Task<Point> GetPointByExternalIdAsync(Guid siteId, string externalId);
        Task<Point> GetPointByTwinIdAsync(Guid siteId, string twinId);
        Task<List<Point>> GetPointsByTagAsync(Guid siteId, string tag);
        Task<List<(TwinWithRelationships PointTwin, Point Point, Asset Asset)>> GetPointAssetPairsByPointIdsAsync(Guid siteId,
                                                                                                                  List<Guid> pointUniqIds,
                                                                                                                  List<string> pointExternalIds,
                                                                                                                  List<Guid> pointTrendIds,
                                                                                                                  bool includePointsWithNoAssets = false);
        Task<Page<(TwinWithRelationships PointTwin, Point Point, Asset Asset)>> GetPointAssetPairsByPointIdsAsync(Guid siteId,
                                                                                                                  List<Guid> pointUniqIds,
                                                                                                                  List<string> pointExternalIds,
                                                                                                                  List<Guid> pointTrendIds,
                                                                                                                  bool includePointsWithNoAssets = false,
                                                                                                                  string continuationToken = null);
        Task<IEnumerable<LiveDataIngestPointDto>> GetSimplePointAssetPairsByPointIdsAsync(
            Guid siteId,
            List<Guid> pointUniqIds,
            List<string> pointExternalIds,
            List<Guid> pointTrendIds,
            bool includePointsWithNoAssets = false);
        Task<List<Point>> GetPointsAsync(Guid siteId, bool includeAssets, int startItemIndex = 0, int pageSize = int.MaxValue);
        Task<AssetRelationshipsDto> GetAssetRelationshipsAsync(Guid siteId, Guid id);
        Task<List<Device>> GetDevicesAsync(Guid siteId, bool? includePoints);
        Task<Device> GetDeviceByUniqueIdAsync(Guid siteId, Guid id, bool? includePoints);
        Task<Device> GetDeviceByExternalPointIdAsync(Guid siteId, string externalPointId);
        Task<Device> UpdateDeviceMetadataAsync(Guid siteId, Guid deviceId, DeviceMetadataDto device);
        Task<List<Device>> GetDevicesByConnectorAsync(Guid siteId, Guid connectorId, bool? includePoints);
        Task<List<Point>> GetPointsByConnectorAsync(Guid siteId, Guid connectorId, bool includeAssets = true);
        ILogger Logger { get; }
        Task<List<Point>> GetAssetPointsAsync(Guid siteId, Guid assetId);
        Task<TwinHistoryDto> GetTwinHistory(Guid siteId, string twinId);

        Task<TwinFieldsDto> GetTwinFields(Guid siteId, string twinId);

		Task<List<TwinSimpleDto>> GetSimpleTwinsDataAsync(IEnumerable<TwinsForMultiSitesRequest> request, CancellationToken cancellationToken);
        Task<List<PointTwinDto>> GetPointsByTwinIdsAsync(Guid siteId, List<string> pointTwinIds);

        Task<List<TwinGeometryViewerIdDto>> GetTwinsWithGeometryIdAsync(
            GetTwinsWithGeometryIdRequest request);
    }

    public class AssetService : IAssetService
    {
        private const string AssetResourceType = "Asset";
        private const string DeviceResourceType = "Device";

        private static readonly string[] IgnoredProperties = {
            Properties.UniqueId,
            Properties.GeometryViewerId,
            Properties.Tags,
            Properties.Name,
            Properties.Communication
        };

        private readonly IDigitalTwinServiceProvider _digitalTwinServiceFactory;
        private readonly ILogger<AssetService> _logger;
        private readonly ITagMapperService _tagMapperService;

        public AssetService(
            IDigitalTwinServiceProvider digitalTwinServiceFactory,
            ILogger<AssetService> logger,
            ITagMapperService tagMapperService)
        {
            _digitalTwinServiceFactory = digitalTwinServiceFactory;
            _logger = logger;
            _tagMapperService = tagMapperService;
        }

        public ILogger Logger => _logger;

        public async Task<List<Category>> GetCategoriesAndAssetsAsync(Guid siteId, bool isCategoryOnly, List<string> modelNames, Guid? floorId = null)
        {
            var digitalTwinService = await _digitalTwinServiceFactory.GetForSiteAsync(siteId);
            var modelParser = await digitalTwinService.GetModelParserAsync();

            var interfaceInfos = new List<InterfaceInfo>();
            // Default to return assets
            if (modelNames.Count == 0)
                modelNames = new List<string> { DtdlTopLevelModelName.Asset };

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

            var twins = await GetSiteTwinsAsync(digitalTwinService);
            // We treat floorID w/a Guid of all 0's to mean "without a floor assigned"
            twins = !floorId.HasValue ? twins : twins.Where(t =>
                    (floorId == Guid.Empty && t.GetFloorId() == null) || t.GetFloorId() == floorId).ToList();

            var categories = await MapToCategoriesAsync(digitalTwinService, interfaceInfos, twins, floorId, isCategoryOnly);
            if (floorId.HasValue && categories?.Count == 0)
            {
                // Currently DtCore has no way of knowing what all the valid floor IDs are for a site --
                //   if no categories are returned because there are no twins at all for that floor,
                //   then that may mean that the floorId isn't valid -- or it could mean there are simply
                //   no twins defined for the floor yet.
                // In the future, we should actually validate the floorId, possibly by storing 
                //   Site-hasFloors->floorId relationships in the model or use a cached recursive 
                //   inverse relationship ADT SQL query.
                // TODO: throw ResourceNotFoundException
                if (!await FloorFoundAnywhere(siteId, twins, floorId))
                {
                    _logger.LogWarning("No Twins exist for floor '{FloorId}' -- floorId may be invalid", floorId);
                }
            }
            return categories;
        }

        // Note this is only called in cases where we were about to return an empty list of twins
        private async Task<bool> FloorFoundAnywhere(Guid siteId, List<TwinWithRelationships> twins, Guid? floorId)
        {
            var digitalTwinService = await _digitalTwinServiceFactory.GetForSiteAsync(siteId);
            return twins.Any(t => t.GetFloorId(digitalTwinService.SiteAdtSettings.LevelModelIds) == floorId);
        }

        private async Task<List<TwinWithRelationships>> GetSiteTwinsAsync(IDigitalTwinService digitalTwinService)
        {
            var siteTwins = await digitalTwinService.GetSiteTwinsAsync();
            return siteTwins.Content?.ToList();
        }

        // This is the functionality we want for the below fn's, but w/o creating string garbage:
        // return  m1.AbsoluteUri.Split(':').Last().Split(';').First() == m2.AbsoluteUri.Split(':').Last().Split(';').First();
        private bool CompareModelNameWithoutPrefixOrVersion(Dtmi m1, Dtmi m2)
        {
            var (s1, s2) = (m1.Versionless, m2.Versionless);
            var (start1, start2) = (s1.LastIndexOf(":") + 1, s2.LastIndexOf(":") + 1);
            return 0 == string.Compare(s1, start1, s2, start2, Math.Max(s1.Length, s2.Length));
        }

        private bool CompareModelNameWithoutPrefixOrVersion(string m1, string m2)
        {
            var (start1, start2) = (m1.LastIndexOf(":") + 1, m2.LastIndexOf(":") + 1);
            var (semi1, semi2) = (m1.LastIndexOf(";"), m2.LastIndexOf(";"));
            // We should always have a ";n" version present if it is a valid model -- model verification should fail at upload
            // var (last1, last2) = ((semi1 == -1 ? m1.Length : semi1),  (semi2 == -1 ? m2.Length : semi2));
            var (last1, last2) = (semi1 + 1, semi2 + 1);
            return 0 == string.Compare(m1, start1, m2, start2, Math.Max(last1 - start1, last2 - start2));
        }

        private string[] MapToTopLevelModelIds(IDigitalTwinService digitalTwinService, string modelName) =>
            modelName switch
            {
                DtdlTopLevelModelName.Asset => digitalTwinService.SiteAdtSettings.AssetModelIds,
                DtdlTopLevelModelName.Space => digitalTwinService.SiteAdtSettings.SpaceModelIds,
                DtdlTopLevelModelName.BuildingComponent => digitalTwinService.SiteAdtSettings.BuildingComponentModelIds,
                DtdlTopLevelModelName.Structure => digitalTwinService.SiteAdtSettings.StructureModelIds,
                _ => Array.Empty<string>(),
            };

        private async Task<List<Category>> MapToCategoriesAsync(
                IDigitalTwinService digitalTwinService,
                List<InterfaceInfo> children,
                List<TwinWithRelationships> twins,
                Guid? floorId,
                bool isCategoryOnly)
        {
            var output = new List<Category>();
            if (twins?.Count == 0)
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
                    CompareModelNameWithoutPrefixOrVersion(
                        category.Model.Id, category.Children.Single().Model.Id))
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

        private async Task<List<Asset>> MapAssetsAsync(
            IDigitalTwinService digitalTwinService,
            IEnumerable<TwinWithRelationships> twins,
            bool includePoints = true)
        {
            var output = new List<Asset>();
            if (twins == null)
            {
                return new List<Asset>();
            }

            // This is part of a solution to the timeout caused by long queries,
            //   such as getAssets/assetTree which return Points which requires this call,
            //   which in turn goes through all the twins in search of reverse relationships.
            //  Note: Specifying the floorId will now mitigate this long query issue.
            // Using multiple cores for a single request may or may not be scalable long-term
            //   depending on the type of compute resource and the # of reqs/sec and request duration.
#if USE_PARALLEL_PROCESSING
            var modelParser = await digitalTwinService.GetModelParserAsync();
            output = twins.AsParallel().Select(async twin =>
                           await MapAssetAsync(digitalTwinService, twin, includePoints, modelParser))
                .Select(task => task.Result)
                .ToList();
#else
            foreach (var twin in twins)
            {
                try
                {
                    output.Add(await MapAssetAsync(digitalTwinService, twin, includePoints));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error mapping twin {TwinId} ({UniqueId}) to asset", twin.Id, twin.UniqueId);
                    // TODO: should we just throw here?
                }
            }
#endif
            return output;
        }


        private async Task<Asset> MapAssetAsync(
            IDigitalTwinService digitalTwinService,
            TwinWithRelationships twin,
            bool includePoints = true,
            IDigitalTwinModelParser modelParser = null)
        {
            modelParser ??= await digitalTwinService.GetModelParserAsync();
            var points = !includePoints ? null : await GetPointsAsync(digitalTwinService, twin);
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

        // Ensure that all optional asset properties are populated
        private async Task<Asset> EnsureAssetHasPoints(IDigitalTwinService digitalTwinService, Asset asset)
        {
            if (asset.Points == null)
            {
                var twin = await digitalTwinService.GetTwinByUniqueIdAsync(asset.Id);
                asset.Points = await GetPointsAsync(digitalTwinService, twin);
            }

            return asset;
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
            TwinWithRelationships t)
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

        public async Task<Asset> GetAssetByUniqueId(Guid siteId, Guid id)
        {
            var digitalTwinService = await _digitalTwinServiceFactory.GetForSiteAsync(siteId);

            try
            {
                var twin = await digitalTwinService.GetTwinByUniqueIdAsync(id);
                if (twin == null)
                {
                    throw new ResourceNotFoundException(AssetResourceType, id);
                }

                var asset = await MapAssetAsync(digitalTwinService, twin);
                asset.Points = await GetPointsAsync(digitalTwinService, twin);
                return asset;
            }
            catch (DigitalTwinCoreException)
            {
                throw new ResourceNotFoundException(AssetResourceType, id);
            }
        }

        private async Task<List<Point>> GetPointsAsync(
            IDigitalTwinService digitalTwinService,
            TwinWithRelationships twin)
        {
            try
            {
                var incomingRelationships = await digitalTwinService.GetIncomingRelationshipsAsync(twin.Id);

                var modelParser = await digitalTwinService.GetModelParserAsync();

                var pointTwins = incomingRelationships
                    .Where(i =>
                        (i.Name == Relationships.HostedBy || i.Name == Relationships.IsCapabilityOf)
                            && modelParser.IsDescendantOfAny(digitalTwinService.SiteAdtSettings.PointModelIds, i.Source.Metadata.ModelId))
                    .Select(i => i.Source)
                    .ToList();

                return MapPoints(digitalTwinService.SiteAdtSettings.SiteId, modelParser, pointTwins);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error mapping points to asset for twin {TwinId} ({UniqueId})", twin.Id, twin.UniqueId);
            }
            return new List<Point>();
        }

        // The method just returns minimum point info to the PortalXL to check if a livedata asset or not
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


        private List<Point> MapPoints(Guid siteId, IDigitalTwinModelParser modelParser, List<TwinWithRelationships> twins)
        {
            return twins.Select(t => MapPoint(siteId, modelParser.GetInterface(t.Metadata.ModelId), t)).ToList();
        }

        private Point MapPoint(Guid siteId, DTInterfaceInfo interfaceInfo, TwinWithRelationships twin)
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

            var assets = twin.Relationships.Where(r => r.Name == Relationships.IsCapabilityOf).Select(r => r.Target).ToList<Twin>();
            var devices = twin.Relationships.Where(r => r.Name == Relationships.HostedBy).Select(r => r.Target).ToList<Twin>();

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
                Assets = assets,
                Devices = devices,
                CategoryName = interfaceInfo.GetDisplayName(),
                IsDetected = twin.GetPropertyValue<bool>(Properties.Detected),
                IsEnabled = twin.GetPropertyValue<bool>(Properties.Enabled),
                TrendInterval = trendIntervalValue.HasValue ? TimeSpan.FromSeconds((double)trendIntervalValue.Value) : (TimeSpan?)null,
                Properties = MapAssetProperties(interfaceInfo, twin),
                Communication = MapPointCommunication(twin)
            };
        }

        private PointCommunication MapPointCommunication(TwinWithRelationships twin)
        {
            var communication = twin.GetJObjectProperty(Properties.Communication);
            return (communication != null) ? PointCommunication.MapFromCustomProperty(communication) : null;
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

        public async Task<Asset> GetAssetById(Guid siteId, string id)
        {
            var digitalTwinService = await _digitalTwinServiceFactory.GetForSiteAsync(siteId);

            try
            {
                var twin = await digitalTwinService.GetTwinByIdAsync(id);
                if (twin == null)
                {
                    throw new ResourceNotFoundException(AssetResourceType, id);
                }

                var asset = await MapAssetAsync(digitalTwinService, twin);
                asset.Points = await GetPointsAsync(digitalTwinService, twin);
                return asset;
            }
            catch (DigitalTwinCoreException)
            {
                throw new ResourceNotFoundException(AssetResourceType, id);
            }
        }

        public async Task<Asset> GetAssetByForgeViewerId(Guid siteId, string forgeViewerId)
        {
            var digitalTwinService = await _digitalTwinServiceFactory.GetForSiteAsync(siteId);

            try
            {
                var twin = await digitalTwinService.GetTwinByForgeViewerIdAsync(forgeViewerId);
                if (twin == null)
                {
                    throw new ResourceNotFoundException(AssetResourceType, forgeViewerId);
                }

                var asset = await MapAssetAsync(digitalTwinService, twin);
                asset.Points = await GetPointsAsync(digitalTwinService, twin);
                return asset;
            }
            catch (DigitalTwinCoreException)
            {
                throw new ResourceNotFoundException(AssetResourceType, forgeViewerId);
            }
        }

        public async Task<List<Asset>> GetAssetsAsync(Guid siteId,
                                                      Guid? categoryId,
                                                      Guid? floorId,
                                                      string searchKeyword,
                                                      bool liveDataOnly = false,
                                                      bool includeExtraProperties = false,
                                                      int startItemIndex = 0,
                                                      int pageSize = int.MaxValue)
        {
            var digitalTwinService = await _digitalTwinServiceFactory.GetForSiteAsync(siteId);

            var modelParser = await digitalTwinService.GetModelParserAsync();
            var interfaceInfos = modelParser.GetInterfaceDescendants(digitalTwinService.SiteAdtSettings.AssetModelIds);

            if (categoryId != null)
            {
                interfaceInfos = new Dictionary<string, DTInterfaceInfo>(
                                    interfaceInfos.Where(i => i.Value.GetUniqueId() == categoryId.Value));
            }

            var siteTwins = await GetSiteTwinsAsync(digitalTwinService);
            var twins = siteTwins.Where(t => interfaceInfos.ContainsKey(t.Metadata.ModelId));

            List<Asset> mappedAssets = await MapAssetsAsync(digitalTwinService, twins, includePoints: liveDataOnly);
            if (includeExtraProperties)
            {
                await EnrichDetailedRelationships(digitalTwinService, mappedAssets);
            }

            IEnumerable<Asset> withKeyword = string.IsNullOrWhiteSpace(searchKeyword) ? mappedAssets : mappedAssets.Where(a =>
                            a.Name.Contains(searchKeyword, StringComparison.InvariantCultureIgnoreCase) ||
                            a.Identifier.Contains(searchKeyword, StringComparison.InvariantCultureIgnoreCase));

            // We treat floorID w/a Guid of all 0's to mean "without a floor assigned"
            IEnumerable<Asset> onFloor = floorId == null ? withKeyword
                            : withKeyword.Where(a => (floorId == Guid.Empty && a.FloorId == null) || a.FloorId == floorId);

            if (liveDataOnly)
            {
                onFloor = onFloor.Where(a => a.Points.Any());
            }

            IEnumerable<Asset> pageSlice = pageSize == int.MaxValue ? onFloor
                            : onFloor.Skip(startItemIndex).Take(pageSize);

            // Now that we've done our filtering and paging, fill in the Points, which is an expensive operation
            IEnumerable<Asset> withPoints = pageSlice //  floorId == null ? onFloor : onFloor
                                                      // AsParallel
                                            .Select(async a => await EnsureAssetHasPoints(digitalTwinService, a))
                                            .Select(t => t.Result);

            var assets = withPoints.ToList();

            if (floorId.HasValue && assets.Count == 0 && withKeyword.Any())
            {
                // See comment in GetCategoriesAndAssetsAsync
                if (!await FloorFoundAnywhere(siteId, siteTwins, floorId))
                {
                    _logger.LogWarning("No Twins exist for floor '{FloorId}' -- floorId may be invalid", floorId);
                }
            }

            return assets;
        }

        private static async Task EnrichDetailedRelationships(IDigitalTwinService digitalTwinService, List<Asset> mappedAssets) // Investa-specific
        {
            var relationships = new string[] {
                Relationships.HasDocument,
                Relationships.HasWarranty,
                Relationships.ManufacturedBy,
                Relationships.LocatedIn,
                Relationships.OwnedBy,
                Relationships.ServicedBy,
                Relationships.MaintenanceResponsibility,
                Relationships.InstalledBy
            };
            foreach (var asset in mappedAssets)
            {
                var validRelationships = asset.Relationships.Where(r => relationships.Contains(r.Name, StringComparer.InvariantCultureIgnoreCase)).ToList();
                foreach (var relationship in validRelationships)
                {
                    var twin = await digitalTwinService.GetTwinByUniqueIdAsync(relationship.TargetId, true);
                    if (twin is null) continue;
                    if (asset.DetailedRelationships.ContainsKey(relationship.Name))
                    {
                        asset.DetailedRelationships[relationship.Name].Add(twin);
                    }
                    else
                    {
                        asset.DetailedRelationships.Add(relationship.Name, new List<TwinWithRelationships>() { twin });
                    }
                }
            }
        }

        public async Task<List<Document>> GetDocumentsForAssetAsync(Guid siteId, Guid assetId)
        {
            var digitalTwinService = await _digitalTwinServiceFactory.GetForSiteAsync(siteId);

            try
            {
                var twin = await digitalTwinService.GetTwinByUniqueIdAsync(assetId);
                if (twin == null)
                {
                    throw new ResourceNotFoundException(AssetResourceType, assetId);
                }

                List<Document> ret;
                var foundDocuments = twin.Relationships.Where(rel => rel.Name == Relationships.HasDocument).Select(rel => rel.Target).ToList();
                if (foundDocuments.Any())
                {
                    ret = MapDocuments(foundDocuments);
                }
                else
                {
                    var incomingRelationships = await digitalTwinService.GetIncomingRelationshipsAsync(twin.Id);

                    var modelParser = await digitalTwinService.GetModelParserAsync();

                    var documentTwins = incomingRelationships
                        .Where(i =>
                            i.Name == Relationships.IsDocumentOf &&
                            modelParser.IsDescendantOfAny(digitalTwinService.SiteAdtSettings.DocumentModelIds, i.Source.Metadata.ModelId))
                        .Select(i => i.Source)
                        .ToList();

                    ret = MapDocuments(documentTwins);
                }

                return ret;
            }
            catch (DigitalTwinCoreException)
            {
                throw new ResourceNotFoundException(AssetResourceType, assetId);
            }
        }

        public async Task<Point> GetPointByUniqueIdAsync(Guid siteId, Guid pointId)
        {
            var digitalTwinService = await _digitalTwinServiceFactory.GetForSiteAsync(siteId);

            var modelParser = await digitalTwinService.GetModelParserAsync();

            var twin = await digitalTwinService.GetTwinByUniqueIdAsync(pointId);

            if (twin == null)
                return null;

            return MapPoint(siteId, modelParser.GetInterface(twin.Metadata.ModelId), twin);
        }

        public async Task<Point> GetPointByTrendIdAsync(Guid siteId, Guid trendId)
        {
            var points = await GetPointsByTrendIdsAsync(siteId, new List<Guid> { trendId });
            return points.SingleOrDefault();
        }


        public async Task<List<Point>> GetPointsByTrendIdsAsync(Guid siteId, List<Guid> trendIds)
        {
            var digitalTwinService = await _digitalTwinServiceFactory.GetForSiteAsync(siteId);
            var modelParser = await digitalTwinService.GetModelParserAsync();

            var twins = await digitalTwinService.GetTwinsWithRelationshipsAsync(siteId);
            var pointTwins = twins.Where(t => modelParser.IsDescendantOfAny(
                   digitalTwinService.SiteAdtSettings.PointModelIds, t.Metadata.ModelId));

            var points = pointTwins.Where(t =>
            {
                var prop = t.GetPropertyValue<Guid>(Properties.TrendID);
                return prop != null && trendIds.Contains(prop.Value);
            });

            var mappedPoints = points
                    .Select(t => MapPoint(siteId, modelParser.GetInterface(t.Metadata.ModelId), t)).ToList();

            return mappedPoints;
        }

        public async Task<List<Point>> GetPointsByTagAsync(Guid siteId, string tag)
        {
            var digitalTwinService = await _digitalTwinServiceFactory.GetForSiteAsync(siteId);

            var modelParser = await digitalTwinService.GetModelParserAsync();
            var interfaceInfos = modelParser.GetInterfaceDescendants(digitalTwinService.SiteAdtSettings.PointModelIds);

            var points = (await GetSiteTwinsAsync(digitalTwinService))
                .Where(t => interfaceInfos.ContainsKey(t.Metadata.ModelId))
                .Select(t => MapPoint(siteId, modelParser.GetInterface(t.Metadata.ModelId), t))
                .ToList();

            return points.Where(p => p.Tags.Select(t => t.Name.ToUpperInvariant()).Contains(tag.ToUpperInvariant()))
                         .ToList();
        }

        // Return tuples of points and any associated asset for IDs of various types.
        // The input lists are modified -- any items left in the list were not found and unable to be processed.
        public async Task<List<(TwinWithRelationships PointTwin, Point Point, Asset Asset)>> GetPointAssetPairsByPointIdsAsync(
            Guid siteId,
            List<Guid> pointUniqIds,
            List<string> pointExternalIds,
            List<Guid> pointTrendIds,
            bool includePointsWithNoAssets = false)
        {
            var digitalTwinService = await _digitalTwinServiceFactory.GetForSiteAsync(siteId);

            var modelParser = await digitalTwinService.GetModelParserAsync();

            var found = new Dictionary<string, TwinWithRelationships>();

            foreach (var uid in new List<Guid>(pointUniqIds ?? Enumerable.Empty<Guid>()))
            {
                var uidTwin = await digitalTwinService.GetTwinByUniqueIdAsync(uid);
                if (uidTwin == null) continue;
                found[uid.ToString()] = uidTwin;
                pointUniqIds?.Remove(uid);
            }
            foreach (var eid in new List<string>(pointExternalIds ?? Enumerable.Empty<string>()))
            {
                var eidTwin = await digitalTwinService.GetTwinByExternalIdAsync(eid);
                if (eidTwin == null) continue;
                found[eid] = eidTwin;
                pointExternalIds?.Remove(eid);
            }
            foreach (var tid in new List<Guid>(pointTrendIds ?? Enumerable.Empty<Guid>()))
            {
                var tidTwin = await digitalTwinService.GetTwinByTrendIdAsync(tid);
                if (tidTwin == null) continue;
                found[tid.ToString()] = tidTwin;
                pointTrendIds?.Remove(tid);
            }

            if (pointUniqIds?.Any() ?? false)
                _logger?.LogWarning("No Capability found for the following uniqueIds: {uids}", JsonSerializer.Serialize(pointUniqIds));

            if (pointExternalIds?.Any() ?? false)
                _logger?.LogWarning("No Capability found for the following externalIds: {eids}", JsonSerializer.Serialize(pointExternalIds));

            if (pointTrendIds?.Any() ?? false)
                _logger?.LogWarning("No Capability found for the following trendIds: {tids}", JsonSerializer.Serialize(pointTrendIds));


            var result = new List<(TwinWithRelationships, Point, Asset)>();
            var disabledPoints = new List<string>();
            var noAssetPoints = new List<string>();
            var multipleAssetPoints = new List<(string, string[])>();

            foreach (var pointTwin in found.Values)
            {
                var capabilityRels = pointTwin.Relationships
                    .Where(r => r.Name == Relationships.IsCapabilityOf);
                // var interfaceInfos = modelParser.GetInterfaceDescendants(digitalTwinService.SiteAdtSettings.PointModelIds);
                var capabilityTargets = capabilityRels
                    // Note: We can have live-data for non-Assets, such as Zones, so don't check for subclass of Asset
                    //.Where(r => modelParser.IsDescendantOfAny(digitalTwinService.SiteAdtSettings.AssetModelIds, r.Target.Metadata.ModelId))
                    .Select(r => r.Target).ToList();

                if (capabilityTargets.Count == 0)
                {
                    noAssetPoints.Add(pointTwin.Id);
                }
                else if (capabilityTargets.Count > 1)
                {
                    multipleAssetPoints.Add((pointTwin.Id, capabilityTargets.Select(c => c.Id).ToArray()));
                }

                var point = MapPoint(siteId, modelParser.GetInterface(pointTwin.Metadata.ModelId), pointTwin);

                if (point.IsEnabled == false)
                {
                    disabledPoints.Add(pointTwin.Id);
                    continue;
                }

                if (capabilityTargets.Count == 0 && includePointsWithNoAssets)
                {
                    result.Add((pointTwin, point, null));
                }
                else if (capabilityTargets.Count >= 1)
                {
                    // Note it's possible to have >1 twin associated with a CapabilityOf - we're only returning the first one here
                    var assetTwin = capabilityTargets.First();
                    var asset = await MapAssetAsync(digitalTwinService, assetTwin);
                    result.Add((pointTwin, point, asset));
                }
            }

            if (disabledPoints.Any())
            {
                _logger?.LogInformation("The following points were disabled {disabledPoints}",
                    JsonSerializer.Serialize(disabledPoints));
            }
            if (noAssetPoints.Any())
            {
                _logger?.LogWarning("No related 'asset' twins found via isCapabilityOf for the following twins: {twins}",
                    JsonSerializer.Serialize(noAssetPoints));
            }
            if (multipleAssetPoints.Any())
            {
                _logger?.LogWarning("Multiple 'asset' twins  found via isCapabilityOf - only first of each  will currently be used: {multipleTwins}",
                    JsonSerializer.Serialize(multipleAssetPoints));
            }

            return result;
        }

        public async Task<AssetRelationshipsDto> GetAssetRelationshipsAsync(Guid siteId, Guid id)
        {
            var digitalTwinService = await _digitalTwinServiceFactory.GetForSiteAsync(siteId);

            try
            {
                var twin = await digitalTwinService.GetTwinByUniqueIdAsync(id);
                if (twin == null)
                {
                    throw new ResourceNotFoundException(AssetResourceType, id);
                }
                var incomingRelationships = await digitalTwinService.GetIncomingRelationshipsAsync(twin.Id);

                return new AssetRelationshipsDto
                {
                    Relationships = AssetRelationshipDto.MapFrom(twin.Relationships),
                    IncomingRelationships = AssetIncomingRelationshipDto.MapFrom(incomingRelationships)
                };
            }
            catch (DigitalTwinCoreException)
            {
                throw new ResourceNotFoundException(AssetResourceType, id);
            }
        }

        public async Task<List<Device>> GetDevicesAsync(Guid siteId, bool? includePoints)
        {
            var digitalTwinService = await _digitalTwinServiceFactory.GetForSiteAsync(siteId);
            var modelParser = await digitalTwinService.GetModelParserAsync();

            var output = new List<Device>();
            var deviceTwins = (await GetSiteTwinsAsync(digitalTwinService))
                                .Where(t => modelParser.IsDescendantOfAny(digitalTwinService.SiteAdtSettings.DeviceModelIds,
                                                                            t.Metadata.ModelId));

            foreach (var deviceTwin in deviceTwins)
            {
                output.Add(await MapDeviceAsync(digitalTwinService, modelParser, deviceTwin, includePoints));
            }
            return output;
        }

        public async Task<List<Device>> GetDevicesByConnectorAsync(Guid siteId, Guid connectorId, bool? includePoints)
        {
            var output = (await GetDevicesAsync(siteId, includePoints))
                .Where(d => d.ConnectorId == connectorId).ToList();

            return output;
        }

        private async Task<Device> MapDeviceAsync(
            IDigitalTwinService digitalTwinService,
            IDigitalTwinModelParser modelParser,
            TwinWithRelationships deviceTwin, bool? includePoints)
        {
            return new Device
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
                Points = (includePoints.GetValueOrDefault()) ? await GetPointsAsync(digitalTwinService, deviceTwin) : null,
                ConnectorId = MapDeviceConnectorId(deviceTwin)
            };
        }

        private Guid? MapDeviceConnectorId(TwinWithRelationships deviceTwin)
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

        public async Task<Device> GetDeviceByUniqueIdAsync(Guid siteId, Guid id, bool? includePoints)
        {
            var digitalTwinService = await _digitalTwinServiceFactory.GetForSiteAsync(siteId);

            try
            {
                var twin = await digitalTwinService.GetTwinByUniqueIdAsync(id);
                if (twin == null)
                {
                    throw new ResourceNotFoundException(DeviceResourceType, id);
                }

                var modelParser = await digitalTwinService.GetModelParserAsync();
                if (!modelParser.IsDescendantOfAny(digitalTwinService.SiteAdtSettings.DeviceModelIds, twin.Metadata.ModelId))
                {
                    throw new ResourceNotFoundException(DeviceResourceType, id);
                }

                return await MapDeviceAsync(digitalTwinService, modelParser, twin, includePoints);
            }
            catch (DigitalTwinCoreException)
            {
                throw new ResourceNotFoundException(DeviceResourceType, id);
            }
        }

        public async Task<Device> GetDeviceByExternalPointIdAsync(Guid siteId, string externalPointId)
        {
            var devices = await GetDevicesAsync(siteId, true);
            return devices.SingleOrDefault(t =>
                t.Points.Any(p => p.Properties.ContainsKey(Properties.ExternalID) &&
                    p.Properties[Properties.ExternalID].Value as string == externalPointId));
        }

        public async Task<Device> UpdateDeviceMetadataAsync(Guid siteId, Guid deviceId, DeviceMetadataDto deviceMetadata)
        {
            var digitalTwinService = await _digitalTwinServiceFactory.GetForSiteAsync(siteId);
            var modelParser = await digitalTwinService.GetModelParserAsync();

            var existingTwin = await digitalTwinService.GetTwinByUniqueIdAsync(deviceId);
            if (existingTwin == null || !modelParser.IsDescendantOfAny(digitalTwinService.SiteAdtSettings.DeviceModelIds, existingTwin.Metadata.ModelId))
            {
                throw new ResourceNotFoundException(DeviceResourceType, deviceId);
            }

            //TODO: Model BACnet device address
            existingTwin.CustomProperties["address"] = deviceMetadata.Address;
            var outputTwin = await digitalTwinService.AddOrUpdateTwinAsync(existingTwin);

            return await MapDeviceAsync(digitalTwinService, modelParser, outputTwin, true);
        }

        public async Task<List<Point>> GetPointsAsync(Guid siteId, bool includeAssets, int startItemIndex = 0, int pageSize = int.MaxValue)
        {
            var digitalTwinService = await _digitalTwinServiceFactory.GetForSiteAsync(siteId);
            var modelParser = await digitalTwinService.GetModelParserAsync();

            var output = new List<Device>();
            var pointTwins = (await GetSiteTwinsAsync(digitalTwinService))
                                .Where(t => modelParser.IsDescendantOfAny(digitalTwinService.SiteAdtSettings.PointModelIds,
                                                                            t.Metadata.ModelId)).ToList();

            return MapPoints(siteId, modelParser, pointTwins);
        }

        public async Task<List<Point>> GetPointsByConnectorAsync(Guid siteId, Guid connectorId, bool includeAssets = true)
        {
            var allPoints = await GetPointsAsync(siteId, includeAssets, 0, int.MaxValue);
            var points = allPoints
               .Where(p => p.Devices.Any(d => d.GetStringProperty(Properties.ConnectorID)?.ToLowerInvariant() == connectorId.ToString().ToLowerInvariant())
                        || p.Properties.ContainsKey(Properties.ConnectorID)
                        && p.Properties[Properties.ConnectorID].Value.ToString().ToLowerInvariant() == connectorId.ToString().ToLowerInvariant());

            return points.ToList();
        }

        public Task<Page<Point>> GetPointsAsync(Guid siteId, bool includeAssets, string continuationToken = null)
        {
            throw new NotImplementedException();
        }

        public Task<Page<Point>> GetPointsByConnectorAsync(Guid siteId, Guid connectorId, bool includeAssets = true, string continuationToken = null)
        {
            throw new NotImplementedException();
        }

        public async Task<int> GetPointsCountAsync(Guid siteId)
        {
            return (await GetPointsAsync(siteId, false, 0, int.MaxValue)).Count(x => x.IsEnabled.GetValueOrDefault());
        }

        public async Task<int> GetPointsByConnectorCountAsync(Guid siteId, Guid connectorId)
        {
            return (await GetPointsByConnectorAsync(siteId, connectorId, false)).Count(p => p.IsEnabled.GetValueOrDefault());
        }

        public Task<Page<Point>> GetPointsByTagAsync(Guid siteId, string tag, string continuationToken = null)
        {
            throw new NotImplementedException();
        }

        public Task<Page<Device>> GetDevicesAsync(Guid siteId, bool? includePoints, string continuationToken = null)
        {
            throw new NotImplementedException();
        }

        public Task<Page<Device>> GetDevicesByConnectorAsync(Guid siteId, Guid connectorId, bool? includePoints, string continuationToken = null)
        {
            throw new NotImplementedException();
        }

        public Task<Page<Asset>> GetAssets(Guid siteId, Guid? categoryId, Guid? floorId, string searchKeyword, bool liveDataOnly = false, bool includeExtraProperties = false, string continuationToken = null)
        {
            throw new NotImplementedException();
        }

        public Task<Page<Category>> GetCategoriesAndAssetsAsync(Guid siteId, bool isCategoryOnly, List<string> modelNames, Guid? floorId = null, string continuationToken = null)
        {
            throw new NotImplementedException();
        }

        public Task<Page<(TwinWithRelationships PointTwin, Point Point, Asset Asset)>> GetPointAssetPairsByPointIdsAsync(Guid siteId, List<Guid> pointUniqIds, List<string> pointExternalIds, List<Guid> pointTrendIds, bool includePointsWithNoAssets = false, string continuationToken = null)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<LiveDataIngestPointDto>> GetSimplePointAssetPairsByPointIdsAsync(Guid siteId, List<Guid> pointUniqIds, List<string> pointExternalIds, List<Guid> pointTrendIds, bool includePointsWithNoAssets = false)
        {
            throw new NotImplementedException();
        }

        public Task<List<TwinGeometryViewerIdDto>> GetTwinsWithGeometryIdAsync(
            GetTwinsWithGeometryIdRequest request)
        {
            throw new NotImplementedException();
        }
        public Task<IEnumerable<LightCategoryDto>> GetCategories(Guid siteId, Guid? floorId, bool isLiveDataOnly)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<AssetNameDto>> GetAssetNames(Guid siteId, IEnumerable<Guid> ids)
        {
            throw new NotImplementedException();
        }

        public Task<List<Point>> GetAssetPointsAsync(Guid siteId, Guid assetId)
        {
            throw new NotImplementedException();
        }

        public Task<Point> GetPointByExternalIdAsync(Guid siteId, string externalId)
        {
            throw new NotImplementedException();
        }

        public Task<Point> GetPointByTwinIdAsync(Guid siteId, string twinId)
        {
            throw new NotImplementedException();
        }

        public Task<TwinHistoryDto> GetTwinHistory(Guid siteId, string twinId)
        {
            throw new NotImplementedException();
        }

        public Task<TwinFieldsDto> GetTwinFields(Guid siteId, string twinId)
        {
            throw new NotImplementedException();
        }

		public Task<List<TwinSimpleDto>> GetSimpleTwinsDataAsync(IEnumerable<TwinsForMultiSitesRequest> request, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

        public Task<List<PointTwinDto>> GetPointsByTwinIdsAsync(Guid siteId, List<string> pointTwinIds)
        {
            throw new NotImplementedException();
        }

        public Task<List<TwinGeometryViewerIdDto>> GetGeometryViewerIdsByTwinIdsAsync(Guid siteId,
            List<string> twinIds)
        {
            throw new NotImplementedException();
        }
    }
}
