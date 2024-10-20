using Azure.DigitalTwins.Core;
using DigitalTwinCore.Constants;
using DigitalTwinCore.Dto;
using DigitalTwinCore.DTO;
using DigitalTwinCore.Exceptions;
using DigitalTwinCore.Infrastructure;
using DigitalTwinCore.Models;
using DigitalTwinCore.Services.AdtApi;
using DTDLParser;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Willow.Infrastructure.Exceptions;

namespace DigitalTwinCore.Services
{
    public interface IDigitalTwinService
    {
        Task<Guid?> GetRelatedSiteId(string twinId);
        Task<IEnumerable<TwinIncomingRelationship>> GetBasicIncomingRelationshipsAsync(string twinId);
        Task<Page<Twin>> GetTwinsByModelsAsync(Guid siteId, IEnumerable<string> models, bool restrictToSite, string continuationToken = null);
        Task<Page<TwinWithRelationships>> GetTwinsWithRelationshipsAsync(string continuationToken, IEnumerable<string> twinIds = null);
        Task<IEnumerable<TwinWithRelationships>> GetTwinsWithRelationshipsAsync(Guid siteId, IEnumerable<string> twinIds = null);
        Task<Page<Twin>> GetTwinsAsync(IEnumerable<string> twinIds = null, string continuationToken = null);
        IList<Model> GetModels();
        IEnumerable<string> GetModelIdsByQuery(string query);
        Model GetModel(string id);
        Task<Model> AddModel(string modelJson);
        Task DeleteModel(string id);

        Task<List<TwinRelationship>> GetRelationships(string twinId);
        Task<List<TwinRelationship>> GetRelationships(IEnumerable<BasicRelationship> relationships);
        Task<List<TwinRelationship>> GetIncomingRelationshipsAsync(string twinId);
        Task<TwinRelationship> UpdateRelationshipAsync(string twinId, string id, JsonPatchDocument value);
        Task<TwinRelationship> GetRelationshipAsync(string twinId, string id);
        Task<TwinRelationship> GetRelationshipUncachedAsync(string twinId, string id);

        Task<TwinRelationship> AddRelationshipAsync(string twinId, string id, Relationship relationship);
        Task DeleteRelationshipAsync(string twinId, string id);
        Task DeleteTwinAndRelationshipsAsync(string id);
        Task DeleteTwinsAndRelationshipsAsync(Guid siteId, IEnumerable<string> ids);

        Task<IReadOnlyCollection<Twin>> GetTwinsAsync(Guid siteId);
        Task<TwinWithRelationships> GetTwinByIdAsync(string id, bool loadRelationships = true);
        Task<TwinWithRelationships> GetTwinByIdUncachedAsync(string id);
        Task<TwinWithRelationships> GetTwinByUniqueIdAsync(Guid id, bool hasAccessToSharedTwin = false);
        Task<Twin> GetTwinFloor(string twinId);
        Task<TwinWithRelationships> GetTwinByExternalIdAsync(string externalId);
        Task<TwinWithRelationships> GetTwinByTrendIdAsync(Guid uniqueId);
        Task<IDigitalTwinModelParser> GetModelParserAsync();
        Task<TwinWithRelationships> GetTwinByForgeViewerIdAsync(string id);
        Task<TwinWithRelationships> AddOrUpdateTwinAsync(Twin entity, bool isSyncRequired = true, string userId = null);
        Task DeleteTwinAsync(string id);
        Task<TwinWithRelationships> PatchTwin(string id, JsonPatchDocument patch, Azure.ETag? ifMatch, string userId);
        Task<Page<TwinWithRelationships>> GetSiteTwinsAsync(string continuationToken = null);
        Task<Page<TwinWithRelationships>> GetFloorTwinsAsync(Guid floorId, string continuationToken = null);
        Task<List<TwinWithRelationships>> GetTwinsByQueryAsync(string query, string alias = null);
        Task AppendRelationshipsAsync(TwinWithRelationships twinWithRelationships);

        SiteAdtSettings SiteAdtSettings { get; set; }
        Task StartReloadFromAdtAsync();

        Task Load(SiteAdtSettings settings, IMemoryCache cache);
        Task<AdtSiteStatsDto> GenerateADTInstanceStats();

        TwinWithRelationships[] FollowAllRelsToTargetModel(TwinWithRelationships twin, string[] relNames, string toModel);

        Task<Dictionary<string, BasicModelDto>> GetModelProps(string id);

        Task<List<TwinRelationship>> GetTwinRelationshipsByQuery(string twinId, string[] relNames, string[] targetModels, int hops, string sourceDirection, string targetDirection);
        
        Task<IEnumerable<Twin>> GetRelatedTwinsByHops(string twinId, int hops);
        Task<List<TwinIdDto>> GetTwinIdsByUniqueIdsAsync(List<Guid> uniqueIds);

        Task<IEnumerable<NestedTwin>> GetTreeAsync(IEnumerable<string> models, IEnumerable<string> outgoingRelationships, IEnumerable<string> incomingRelationships, bool exactModelMatch = false);

        Task<IEnumerable<Twin>> GetSitesByScopeAsync(string twinId);

        Task<IEnumerable<TwinMatchDto>> FindClosestWithCustomProperty(ClosestWithCustomPropertyQuery query);

        Task<Page<TwinWithRelationships>> GetTwinsByModuleTypeNameAsync(List<string> requestModuleTypeNamePaths,
            string continuationToken=null);

        Task<List<BuildingsTwinDto>> GetBuildingTwinsByExternalIds(List<string> externalIdValues, string externalIdName);


    }

    public class DigitalTwinService : IDigitalTwinService
    {
        private const string SiteIdProperty = Properties.SiteId;
        private const string UniqueIdProperty = Properties.UniqueId;
        private IConfiguration _config;

        protected readonly IAdtApiService _adtApiService;
        private readonly TimedLock _modelParserLockGuard = new TimedLock();

        protected IDigitalTwinCache _digitalTwinCache;
        private IDigitalTwinModelParser _digitalTwinModelParser;
        private ILogger<DigitalTwinService> _logger;

        public DigitalTwinService(IAdtApiService adtApiService, IConfiguration config = null, ILogger<DigitalTwinService> logger = null)
        {
            _adtApiService = adtApiService;
            _config = config;
            _logger = logger;
        }

        public SiteAdtSettings SiteAdtSettings { get; set; }

        private AzureDigitalTwinsSettings InstanceSettings => SiteAdtSettings.InstanceSettings;


        public async Task StartReloadFromAdtAsync()
        {
            if (_digitalTwinCache.IsLoading)
            {
                return;
            }

            // Return right away -- the call will time out otherwise 
            Task.Run(async () =>
            {
                await _digitalTwinCache.Reload();
            });
        }

        public async Task<Model> AddModel(string modelJson)
        {
            EnsureLoaded();

            try
            {
                using var jsonDocument = JsonDocument.Parse(modelJson);
                var modelId = jsonDocument.RootElement.GetProperty("@id").GetString();
                EnsureModelAccess(modelId, true);
            }
            catch (DigitalTwinCoreException)
            {
                throw;
            }
            catch (Exception)
            {
                throw new BadRequestException("Error parsing model json");
            }

            var dto = await _adtApiService.CreateModel(InstanceSettings, modelJson);
            _digitalTwinModelParser = null;
            var model = Model.MapFrom(dto);
            model.ModelDefinition = modelJson;
            return _digitalTwinCache.UpdateCachedModel(model);
        }

        public async Task Load(SiteAdtSettings settings, IMemoryCache memoryCache)
        {
            if (SiteAdtSettings != null)
            {
                throw new DigitalTwinCoreException(settings.SiteId, "DigitalTwinService.Load() called twice. Instance can only be set up once.");
            }
            SiteAdtSettings = settings;

            var instance = SiteAdtSettings.InstanceSettings.InstanceUri;
            // Create a DigitalTwinCache per ADT instance, not per site, for cases where we share instances
            _digitalTwinCache = await memoryCache.GetOrCreateAsync($"DigitalTwinCache_{instance}", async (c) =>
            {
                c.SetPriority(CacheItemPriority.NeverRemove);
                c.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(365);
                return new DigitalTwinCache(_adtApiService, settings.InstanceSettings, _config, _logger);
            });
        }

        public async Task<TwinWithRelationships> PatchTwin(string id, JsonPatchDocument patch, Azure.ETag? ifMatch, string userId)
        {
            EnsureLoaded();
            var twin = await _digitalTwinCache.GetTwinByIdAsync(id);
            if (twin == null)
            {
                throw new ResourceNotFoundException("twin", id);
            }

            EnsureModelAccess(twin.Metadata.ModelId, true);
            EnsureTwinAccess(twin);

            await _adtApiService.PatchTwin(InstanceSettings, id, patch, ifMatch);

            // We are currently modifying the cached twin in-place 
            // await _digitalTwinCache.UpdateCachedTwinAsync(Twin.MapFrom(updatedTwin));
            try
            {
                // TODO: We need to add code to verify that we don't try and patch indexed properties that should
                //  never change, such as ExternalId, TrendId, etc. 
                // If we want to support this, we'd need to call UpdateCachedTwin and make sure the code
                //   handles the changing of previously indexed IDs
                twin.ApplyCachedTwinPatch(patch);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error applying patch to local cached twin {id} : {patch}", id, patch);
                throw;
            }

            return twin;
        }

        public async Task<TwinWithRelationships> AddOrUpdateTwinAsync(Twin entity, bool isSyncRequired = true, string userId = null)
        {
            EnsureLoaded();
            EnsureModelAccess(entity.Metadata.ModelId, true);

            var parser = await GetModelParserAsync();
            var interfaceInfo = parser.GetInterface(entity.Metadata.ModelId);

            if (interfaceInfo.Contents.ContainsKey(SiteIdProperty) && !entity.CustomProperties.ContainsKey(SiteIdProperty))
            {
                entity.CustomProperties.Add(SiteIdProperty, SiteAdtSettings.SiteId);
            }

            // Note that currently all top-level models have this property, so the test below is always true
            // TODO: Handle all immutable properties such as uniqueID in a consistent way, or investigate as ADT feature
            if (interfaceInfo.Contents.ContainsKey(UniqueIdProperty))
            {
                var entityUniqId = entity.UniqueIdFromProperties;
                var existingWithTwinId = await _digitalTwinCache.GetTwinByIdAsync(entity.Id);
                if (existingWithTwinId != null && entityUniqId != null && entityUniqId != existingWithTwinId.UniqueId)
                {
                    throw new DigitalTwinCoreException(SiteAdtSettings.SiteId,
                        $"Can't update twin {entity.Id} with uniqueId: {entityUniqId} with the new uniqueID {existingWithTwinId.UniqueId}");
                }
                else if (existingWithTwinId == null && entityUniqId != null)
                {
                    var existingWithUniqId = await _digitalTwinCache.GetTwinByUniqueIdAsync(entityUniqId.Value);
                    if (existingWithUniqId != null && entity.Id != existingWithUniqId.Id)
                    {
                        throw new DigitalTwinCoreException(SiteAdtSettings.SiteId,
                            $"Can't create twin {entity.Id} with uniqueID {entityUniqId}- the twin {existingWithUniqId.Id} with the same uniqueID already exists");
                    }
                }

                if (entityUniqId == null)
                {
                    // Add a uniqueId if none has been provided -- make sure to re-use any existing uniqueId
                    entity.CustomProperties.Add(UniqueIdProperty, existingWithTwinId?.UniqueId ?? Guid.NewGuid());
                }
            }

            var providedSiteId = entity.GetStringProperty(SiteIdProperty);
            if (providedSiteId != null && providedSiteId != SiteAdtSettings.SiteId.ToString())
            {
                throw new DigitalTwinCoreException(SiteAdtSettings.SiteId, "Failed to add twin. Site Id in twin JSON must match the site id in the request URI.");
            }

            EnsureTwinAccess(entity);

            var dto = entity.MapToDto();

            var dtoResponse = await _adtApiService.AddOrUpdateTwin(InstanceSettings, entity.Id, dto);
            return await _digitalTwinCache.UpdateCachedTwinAsync(Twin.MapFrom(dtoResponse));
        }

        public async Task<TwinRelationship> AddRelationshipAsync(string twinId, string id, Relationship relationship)
        {
            EnsureLoaded();

            var twin = await GetTwinByIdAsync(twinId);
            if (twin == null)
            {
                throw new ResourceNotFoundException("Twin", twinId);
            }
            EnsureModelAccess(twin.Metadata.ModelId, true);
            EnsureTwinAccess(twin);

            var dto = relationship.MapToDto();
            var dtoResponse = await _adtApiService.AddRelationship(InstanceSettings, twinId, id, dto);
            return await _digitalTwinCache.AddCachedRelationship(Relationship.MapFrom(dtoResponse));
        }

        public async Task DeleteModel(string id)
        {
            EnsureLoaded();
            EnsureModelAccess(id, true);

            await _adtApiService.DeleteModel(InstanceSettings, id);
            _digitalTwinModelParser = null;
            _digitalTwinCache.RemoveCachedModel(id);
        }

        public async Task DeleteRelationshipAsync(string twinId, string id)
        {
            EnsureLoaded();

            var twin = await GetTwinByIdAsync(twinId);
            if (twin == null)
            {
                throw new ResourceNotFoundException("Twin", twinId);
            }
            EnsureModelAccess(twin.Metadata.ModelId, true);
            EnsureTwinAccess(twin);

            await _adtApiService.DeleteRelationship(InstanceSettings, twinId, id);
            await _digitalTwinCache.RemoveCachedRelationshipAsync(twinId, id);
        }

        public async Task DeleteTwinAsync(string id)
        {
            EnsureLoaded();
            await EnsureTwinAccessAsync(id);
            await _adtApiService.DeleteTwin(InstanceSettings, id);
            await _digitalTwinCache.RemoveCachedTwinAsync(id);
        }

        public async Task DeleteTwinsAndRelationshipsAsync(Guid siteId, IEnumerable<string> ids)
        {
            EnsureLoaded();

            var allTwins = ids.Select(id => _digitalTwinCache.GetTwinByIdAsync(id).Result).ToList();
            var nRels = allTwins.Sum(t => t.Relationships.Count);
            var iRel = 0;
            var deleteRelTasks = allTwins.Select(async twin =>
            {
                foreach (var rel in twin.Relationships)
                {
                    Debug.WriteLine($"Deleting relationship {++iRel} of {nRels}");
                    await DeleteRelationshipAsync(twin.Id, rel.Id);
                }
            });

            _logger?.LogInformation("Starting to delete {ntwins} twins with a total of {nrels} relationships",
                                                        allTwins.Count, nRels);

            await Task.WhenAll(deleteRelTasks);

            _logger?.LogInformation("Done deleting relationships");

            var deleteTwinTasks = allTwins.Select(async (twin, i) =>
            {
                Debug.WriteLine($"Deleting twin {i + 1} of {allTwins.Count}");
                await this.DeleteTwinAsync(twin.Id);
            });
            await Task.WhenAll(deleteTwinTasks);

            _logger?.LogInformation("Done deleting twins");
        }

        public async Task DeleteTwinAndRelationshipsAsync(string id)
        {
            var twin = await _digitalTwinCache.GetTwinByIdAsync(id);

            foreach (var rel in twin.Relationships)
            {
                await DeleteRelationshipAsync(twin.Id, rel.Id);
            }
            await DeleteTwinAsync(twin.Id);
        }

        public async Task<List<TwinRelationship>> GetIncomingRelationshipsAsync(string twinId)
        {
            EnsureLoaded();

            var twin = await GetTwinByIdAsync(twinId);
            if (twin == null)
            {
                throw new ResourceNotFoundException("Twin", twinId);
            }
            EnsureModelAccess(twin.Metadata.ModelId, true);
            EnsureTwinAccess(twin);

            return await _digitalTwinCache.GetIncomingRelationshipsAsync(twinId);
        }

        public Model GetModel(string id)
        {
            EnsureLoaded();
            EnsureModelAccess(id, true);

            return _digitalTwinCache.GetModel(id);
        }

        public IList<Model> GetModels()
        {
            EnsureLoaded();

            return _digitalTwinCache.GetModels().Where(m => IsSiteSpecificOrSharedModel(m.Id)).ToList();
        }

        public IEnumerable<string> GetModelIdsByQuery(string query)
        {
            EnsureLoaded();
            return _digitalTwinCache.GetModels()
                                    .Where(m => IsSiteSpecificOrSharedModel(m.Id) && m.Id.Contains(query, StringComparison.InvariantCultureIgnoreCase))
                                    .Select(m => m.Id);
        }

        public async Task<TwinRelationship> GetRelationshipAsync(string twinId, string id)
        {
            EnsureLoaded();
            var twin = await _digitalTwinCache.GetTwinByIdAsync(twinId);
            if (twin == null)
            {
                return null;
            }
            EnsureModelAccess(twin.Metadata.ModelId, true);
            EnsureTwinAccess(twin);

            return twin?.Relationships.SingleOrDefault(r => r.Id == id);
        }

        public async Task<TwinRelationship> GetRelationshipUncachedAsync(string twinId, string id)
        {
            EnsureLoaded();
            var twin = await _digitalTwinCache.GetTwinByIdAsync(twinId);
            if (twin == null)
            {
                return null;
            }
            EnsureModelAccess(twin.Metadata.ModelId, true);
            EnsureTwinAccess(twin);

            try
            {
                var relationshipDto = await _adtApiService.GetRelationship(SiteAdtSettings.InstanceSettings, twinId, id);

                if (relationshipDto != null)
                {
                    return new TwinRelationship
                    {
                        Id = relationshipDto.Id,
                        Name = relationshipDto.Name,
                        Source = twin,
                        Target = await _digitalTwinCache.GetTwinByIdAsync(relationshipDto.TargetId),
                        CustomProperties = relationshipDto.Properties
                    };
                }
            }
            catch (AdtApiException)
            { }
            return null;
        }

        public async Task<TwinWithRelationships> GetTwinByIdAsync(string id, bool loadRelationships = true)
        {
            EnsureLoaded();
            var twin = await _digitalTwinCache.GetTwinByIdAsync(id);
            if (twin == null)
            {
                return null;
            }
            EnsureModelAccess(twin.Metadata.ModelId, true);
            EnsureTwinAccess(twin);
            return twin;
        }

        public async Task<TwinWithRelationships> GetTwinByIdUncachedAsync(string id)
        {
            EnsureLoaded();

            BasicDigitalTwin twinDto;
            try
            {
                twinDto = await _adtApiService.GetTwin(SiteAdtSettings.InstanceSettings, id);
                if (twinDto != null)
                {
                    var twin = new TwinWithRelationships
                    {
                        Id = twinDto.Id,
                        CustomProperties = Twin.MapCustomProperties(twinDto.Contents),
                        Metadata = TwinMetadata.MapFrom(twinDto.Metadata)
                    };

                    EnsureTwinAccess(twin);

                    await Task.CompletedTask;
                    return twin;
                }
            }
            catch (AdtApiException)
            { }
            return null;
        }

        public async Task<TwinWithRelationships> GetTwinByUniqueIdAsync(Guid id, bool hasAccessToSharedTwin = false)
        {
            EnsureLoaded();
            var twin = await _digitalTwinCache.GetTwinByUniqueIdAsync(id);
            if (twin == null)
            {
                return null;
            }
            EnsureModelAccess(twin.Metadata.ModelId, true);
            if (!hasAccessToSharedTwin)
                EnsureTwinAccess(twin);
            return twin;
        }

        public async Task<List<TwinIdDto>> GetTwinIdsByUniqueIdsAsync(List<Guid> uniqueIds)
        {
	        EnsureLoaded();
			var result = new List<TwinIdDto>();
			foreach (var id in uniqueIds)
	        {
		        var twin = await _digitalTwinCache.GetTwinByUniqueIdAsync(id);
		        if (twin == null)
		        {
			        continue;
		        }
		        result.Add(TwinIdDto.MapFrom(twin));
	        }

	        return result;
        }
		public async Task<TwinWithRelationships> GetTwinByExternalIdAsync(string externalId)
        {
            EnsureLoaded();
            var twin = await _digitalTwinCache.GetTwinByExternalIdAsync(externalId);
            if (twin == null)
            {
                return null;
            }
            EnsureModelAccess(twin.Metadata.ModelId, true);
            EnsureTwinAccess(twin);
            return twin;
        }

        public async Task<TwinWithRelationships> GetTwinByTrendIdAsync(Guid trendId)
        {
            EnsureLoaded();
            var twin = await _digitalTwinCache.GetTwinByTrendIdAsync(trendId);
            if (twin == null)
            {
                return null;
            }
            EnsureModelAccess(twin.Metadata.ModelId, true);
            EnsureTwinAccess(twin);
            return twin;
        }

        public async Task<TwinWithRelationships> GetTwinByForgeViewerIdAsync(string id)
        {
            EnsureLoaded();
            var twin = await _digitalTwinCache.GetTwinByForgeViewerIdAsync(id);
            if (twin == null)
            {
                return null;
            }
            EnsureModelAccess(twin.Metadata.ModelId, true);
            EnsureTwinAccess(twin);
            return twin;
        }

        public async Task<IReadOnlyCollection<Twin>> GetTwinsAsync(Guid siteId)
        {
            EnsureLoaded();
            var twins = await _digitalTwinCache.GetTwinsAsync();
            // Site-specific models not supported -- can return shared list
            // return twins.Where(t => IsSiteSpecificOrSharedModel(t.Metadata.ModelId) && IsTwinForSite(t)).ToList();
            return twins;
        }

        // Gets twins from ADT based on an ADT Sql query.
        // Note that this returns only as much of the twin as the query specifies - such as only the $diId/twin-name.
        // This call does not convert the returned twins to fully-loaded twinsWithRels stored in the internal cache.
        public async Task<List<TwinWithRelationships>> GetTwinsByQueryAsync(string query, string alias = null)
        {
            EnsureLoaded();
            var dtos = await _adtApiService.GetTwins(InstanceSettings, query);

            var twinsWithRelationships = dtos.Select(d =>
                    new TwinWithRelationships
                    {
                        Id = d.Id,
                        CustomProperties = Twin.MapCustomProperties(d.Contents),
                        Metadata = TwinMetadata.MapFrom(d.Metadata)
                    })
                .ToList();
            return twinsWithRelationships;
        }

        public async Task<TwinRelationship> UpdateRelationshipAsync(string twinId, string id, JsonPatchDocument value)
        {
            EnsureLoaded();
            var twin = await _digitalTwinCache.GetTwinByIdAsync(id);
            if (twin == null)
            {
                return null;
            }
            EnsureModelAccess(twin.Metadata.ModelId, true);
            EnsureTwinAccess(twin);

            var dto = await _adtApiService.UpdateRelationship(InstanceSettings, twinId, id, value);
            return await _digitalTwinCache.UpdateCachedRelationship(Relationship.MapFrom(dto));
        }

        // Do depth-first search to find all twins that inherit from a specific model along a given set of relationships
        public TwinWithRelationships[] FollowAllRelsToTargetModel(TwinWithRelationships twin, string[] relNames, string toModel)
        {
            var frontier = new Stack<TwinWithRelationships>();
            var seen = new HashSet<string>();  // avoid graph loops
            var results = new List<TwinWithRelationships>();
            var parser = GetModelParserAsync().Result;

            toModel = DigitalTwinModelParser.QualifyModelName(toModel);

            var fwdRels = relNames.Where(r => !r.StartsWith('-')).ToList();
            var revRels = relNames.Where(r => r.StartsWith('-')).Select(s => s.Substring(1)).ToList();

            frontier.Push(twin);
            while (frontier.Count > 0)
            {
                if (frontier.Count > 1000)
                {
                    // This should never happen with the "seen" list, but better safe than sorry
                    throw new InvalidOperationException("Depth-limit exceeded for relationship query");
                }
                var candidate = frontier.Pop();
                seen.Add(candidate.Id);
                var twinModel = candidate.Metadata.ModelId;
                if (parser.IsDescendantOf(toModel, twinModel))
                {
                    results.Add(candidate);
                    continue;
                }

                var found = new List<TwinWithRelationships>();
                found.AddRange(candidate.Relationships.AsEnumerable()
                    .Where(t => fwdRels.Contains(t.Name))
                    .Select(r => r.Target));

                found.AddRange(_digitalTwinCache.GetIncomingRelationshipsAsync(candidate.Id).Result
                    .Where(t => revRels.Contains(t.Name))
                    .Select(r => r.Source));

                foreach (var t in found)
                    if (!seen.Contains(t.Id))
                        frontier.Push(t);
            }

            return results.ToArray();
        }

        public async Task<IDigitalTwinModelParser> GetModelParserAsync()
        {
            EnsureLoaded();
            if (_digitalTwinModelParser == null)
            {
                try
                {
                    using (await _modelParserLockGuard.Lock())
                    {
                        if (_digitalTwinModelParser == null)
                        {
                            _digitalTwinModelParser = await DigitalTwinModelParser.CreateAsync(GetModels(), _logger);
                        }
                    }
                }
                catch (ParsingException ex)
                {
                    throw new DigitalTwinCoreException(SiteAdtSettings.SiteId, "Error parsing models", ex);
                }
            }

            return _digitalTwinModelParser;
        }


        private void EnsureModelAccess(string modelId, bool isReadOnly = false)
        {
            var siteCode = GetSiteCodeFromModelId(modelId);
            bool hasAccess = SiteAdtSettings.SiteCodeForModelId == null ? siteCode == null : (siteCode == SiteAdtSettings.SiteCodeForModelId || (isReadOnly && siteCode == null));

            if (!hasAccess)
            {
                throw new DigitalTwinCoreException(SiteAdtSettings.SiteId, "Access to model denied. Invalid namespace.");
            }
        }

        private async Task EnsureTwinAccessAsync(string twinId)
        {
            var twin = await GetTwinByIdAsync(twinId);
            EnsureTwinAccess(twin);
        }

        private void EnsureTwinAccess(Twin twin)
        {
            if (twin != null)
            {
                var twinSiteId = twin.GetSiteId(SiteAdtSettings.SiteModelIds);
                // TODO: Doesn't this allow a client to access another client's twin
                //   if there is no site associated with it?
                if (twinSiteId != null && twinSiteId != SiteAdtSettings.SiteId)
                {
                    throw new DigitalTwinCoreException(SiteAdtSettings.SiteId, "Access to twin denied. Invalid site.");
                }
            }
        }

        private void EnsureLoaded()
        {
            if (InstanceSettings == null)
            {
                throw new DigitalTwinCoreException(Guid.Empty, "DigitalTwinService.Load() must be called to set up service before use");
            }
        }

        private bool IsSiteSpecificOrSharedModel(string modelId)
        {
            var siteCode = GetSiteCodeFromModelId(modelId);
            return (siteCode == null || siteCode == SiteAdtSettings.SiteCodeForModelId);
        }

        public static string GetSiteCodeFromModelId(string modelId)
        {
#if true
            // There are no longer any old-style models, and we do not support site-specific models at the moment.
            // If we decide to support SSM's, we need to parse once and store this info so we don't create string 
            //   garbage for every twin in the ADT instance as we scan them each time.
            return null;
#else
            var modelIdParts = modelId.Split(':');

            if (modelIdParts[1] == "willow")
            {
                //Old style model
                if (modelIdParts.Length > 3)
                {
                    return modelIdParts[^2];
                }
                else
                {
                    return null;
                }
            }
            else if (modelIdParts[1] == "com" && modelIdParts[2] == "willowinc")
            {
                //New style model (site code is penultimate)
                if (modelIdParts.Length > 4)
                {
                    return modelIdParts[^2];
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
#endif
        }

        private bool IsTwinForSite(TwinWithRelationships t)
        {
            var twinSiteId = t.GetSiteId(SiteAdtSettings.SiteModelIds);

            return (twinSiteId == null || twinSiteId == SiteAdtSettings.SiteId);
        }

        public Task<AdtSiteStatsDto> GenerateADTInstanceStats()
        {
            return _digitalTwinCache.GenerateADTInstanceStats();
        }

        public async Task<Page<Twin>> GetTwinsAsync(IEnumerable<string> twinIds = null, string continuationToken = null)
        {
            var twins = await _digitalTwinCache.GetTwinsAsync();

            if (twinIds != null && twinIds.Any())
                twins = twins.Where(x => twinIds.Contains(x.Id)).ToList();

            return new Page<Twin> { Content = twins };
        }

        public async Task<Page<TwinWithRelationships>> GetTwinsWithRelationshipsAsync(string continuationToken, IEnumerable<string> twinIds = null)
        {
            var twins = await _digitalTwinCache.GetTwinsAsync();

            if (twinIds != null && twinIds.Any())
                twins = twins.Where(x => twinIds.Contains(x.Id)).ToList();

            return new Page<TwinWithRelationships> { Content = twins };
        }

        public async Task<IEnumerable<TwinWithRelationships>> GetTwinsWithRelationshipsAsync(Guid siteId, IEnumerable<string> twinIds = null)
        {
            var twins = await _digitalTwinCache.GetTwinsAsync();

            if (twinIds != null && twinIds.Any())
                twins = twins.Where(x => twinIds.Contains(x.Id)).ToList();

            return twins;
        }

        public Task<List<TwinRelationship>> GetRelationships(string id)
        {
            throw new NotImplementedException();
        }

        public Task<Page<Twin>> GetTwinsByModelsAsync(Guid siteId, IEnumerable<string> models, bool restrictToSite, string continuationToken = null)
        {
            throw new NotImplementedException();
        }

        public Task<Twin> GetTwinFloor(string twinId)
        {
            throw new NotImplementedException();
        }

        public Task AppendRelationshipsAsync(TwinWithRelationships twinWithRelationships)
        {
            throw new NotImplementedException();
        }

        public Task<Page<TwinWithRelationships>> GetSiteTwinsAsync(string continuationToken = null)
        {
            throw new NotImplementedException();
        }

        public Task<Page<TwinWithRelationships>> GetFloorTwinsAsync(Guid floorId, string continuationToken = null)
        {
            throw new NotImplementedException();
        }

        public Task<Page<TwinWithRelationships>> GetTwinsByModuleTypeNameAsync(List<string> requestModuleTypeNamePaths,
            string continuationToken=null)
        {
            throw new NotImplementedException();
        }
        public Task<IEnumerable<TwinIncomingRelationship>> GetBasicIncomingRelationshipsAsync(string twinId)
        {
            throw new NotImplementedException();
        }

        public Task<List<TwinRelationship>> GetRelationships(IEnumerable<BasicRelationship> relationships)
        {
            throw new NotImplementedException();
        }

        public Task<Guid?> GetRelatedSiteId(string twinId)
        {
            throw new NotImplementedException();
        }

        public Task<Dictionary<string, BasicModelDto>> GetModelProps(string id)
        {
            throw new NotImplementedException();
        }

        public Task<List<TwinRelationship>> GetTwinRelationshipsByQuery(string twinId, string[] relNames, string[] targetModels, int hops, string sourceDirection, string targetDirection)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Twin>> GetRelatedTwinsByHops(string twinId, int hops)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<NestedTwin>> GetTreeAsync(IEnumerable<string> models, IEnumerable<string> outgoingRelationships, IEnumerable<string> incomingRelationships, bool exactModelMatch = false)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Twin>> GetSitesByScopeAsync(string twinId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TwinMatchDto>> FindClosestWithCustomProperty(ClosestWithCustomPropertyQuery query)
        {
            throw new NotImplementedException();
        }

        public Task<List<BuildingsTwinDto>> GetBuildingTwinsByExternalIds(List<string> externalIdValues, string externalIdName)
        {
            throw new NotImplementedException();
        }
    }
}
