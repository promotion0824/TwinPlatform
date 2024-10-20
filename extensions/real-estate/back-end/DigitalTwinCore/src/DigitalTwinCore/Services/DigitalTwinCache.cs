
// Use a cache implementation that uses a primary ConcurrentDictionary indexed by
//  Id for quick twin lookups, and a shared lock-free list of the most recent twins
//   when all twins are needed.
#define CACHE_READ_REPLICA
#define USE_PARALLEL_PROCESSING

using DigitalTwinCore.Infrastructure;
using DigitalTwinCore.Models;
using DigitalTwinCore.Services.AdtApi;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using DigitalTwinCore.Constants;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Willow.Infrastructure.Exceptions;
using DigitalTwinCore.Extensions;
using System.Text.Json;
using DigitalTwinCore.Dto;
using Azure.DigitalTwins.Core;

namespace DigitalTwinCore.Services
{
    public interface IDigitalTwinCache
    {
        Task Reload();
        bool IsLoading { get; }
        List<Model> GetModels();
        Model GetModel(string id);
        Model UpdateCachedModel(Model model);
        void RemoveCachedModel(string id);

        Task<IReadOnlyCollection<TwinWithRelationships>> GetTwinsAsync();
        Task<TwinWithRelationships> GetTwinByIdAsync(string id);
        Task<TwinWithRelationships> GetTwinByUniqueIdAsync(Guid uniqueId);
        Task<TwinWithRelationships> GetTwinByExternalIdAsync(string externalId);
        Task<TwinWithRelationships> GetTwinByTrendIdAsync(Guid uniqueId);
        Task<TwinWithRelationships> GetTwinByForgeViewerIdAsync(string id);

        Task<List<TwinRelationship>> GetIncomingRelationshipsAsync(string twinId);
        
         // Note: We are mutating the twin in-place in the calls below
        Task<TwinWithRelationships> UpdateCachedTwinAsync(Twin twin);
        Task<TwinRelationship> AddCachedRelationship(Relationship relationship);
        Task<TwinRelationship> UpdateCachedRelationship(Relationship relationship);
        Task RemoveCachedTwinAsync(string id);
        Task RemoveCachedRelationshipAsync(string twinId, string relationshipId);

        Task<AdtSiteStatsDto> GenerateADTInstanceStats();
    }


    public class DigitalTwinCache : IDigitalTwinCache
    {
        private readonly IAdtApiService _adtApiService;

        private IReadOnlyCollection<TwinWithRelationships> _readTwins; // latest copy of twins 
        private ConcurrentDictionary<string,TwinWithRelationships> _twins; // transient copy of twins -- may be in the process of updating

        private ConcurrentDictionary<Guid,TwinWithRelationships> _twinsByUniqueId; // index by uniqueId
        private ConcurrentDictionary<string,Twin> _twinsByExternalId; // index by externalId - for live-data
        private ConcurrentDictionary<string,Twin> _twinsByTrendId; // index by trendId - for live-data

        private IReadOnlyCollection<TwinWithRelationships> ReadTwins => _readTwins ??= getReadTwins();
        private ConcurrentMultiDictionary<string, TwinRelationship> _incommingRels = new ConcurrentMultiDictionary<string, TwinRelationship>();

        private Dictionary<string, Model> _models;
        private readonly object _modelsLockGuard = new object();
        private readonly TimedLock _twinsLockGuard = new TimedLock();
        private readonly AzureDigitalTwinsSettings _instanceSettings;
        private readonly bool _enablePersistentCache = false;
        private readonly string _persistentCacheMode;
        private ILogger<DigitalTwinService> _logger;
        private ConcurrentDictionary<string, string> _errorTwins = new ConcurrentDictionary<string, string>();
        public bool IsLoading { get; private set; } = false;
#if DEBUG
        private int _tasksComplete = 0;
        private int _tasksTotal = 0;
#endif


        public DigitalTwinCache(IAdtApiService adtApiService, AzureDigitalTwinsSettings instanceSettings, IConfiguration config, ILogger<DigitalTwinService> logger = null)
        {
            _adtApiService = adtApiService;
            _instanceSettings = instanceSettings;
            _logger = logger;


            // When debugging locally to a remote ADT instance (potentially across the world)
            //   a local persistent cache can be enabled to restore the _twins to their last state.
            // NOTE: You'll probably want this to remain disabled while running unit tests, unless
            //   the goal is to regression test with the persistent cache itself.
            _enablePersistentCache = config?.GetValue<bool>("Caching:persistedTwinCacheEnabled") ?? false;
            // The "savdOnly" mode allows us to save a cache file after loading from ADT, 
            //   but never load from it - this allows dev to copy the cache file from the appService to use locally
            _persistentCacheMode = config?.GetValue<string>("Caching:persistedTwinCacheMode");
        }

        public async Task<IReadOnlyCollection<TwinWithRelationships>> GetTwinsAsync()
        {
            await EnsureTwinCacheLoaded();

            // Gets re-created on-demand if reset to null whenever twins are added or deleted
            return ReadTwins;
        }

        private IReadOnlyCollection<TwinWithRelationships> getReadTwins()
        {
            _logger?.LogInformation("Starting regenerating read-only twins list");
            return _twins.Values.ToList().AsReadOnly();
        }

        // _twins has been updated -- update read-only list, uniqueId index, and incomming rel index
        private void updateTwinsIndicies()
        {
            using (_logger?.BeginScope("Regenerating twin indicies for {ntwins} twins", _twins.Count))
            {
                // Atomically assign most recent twins list copy
                _readTwins = _twins.Values.ToList().AsReadOnly();
                _twinsByUniqueId = new ConcurrentDictionary<Guid, TwinWithRelationships>();
                _twinsByExternalId = new ConcurrentDictionary<string, Twin>();
                _twinsByTrendId = new ConcurrentDictionary<string, Twin>();
                // To deal with >1 twin for uniqueId issue until resolved, we will emit warnings at startup
                //   but here for the index we will just choose whichever happens to be entered latest 
                // _readTwins.Select(t => new KeyValuePair<string, TwinWithRelationships>(t.UniqueId.ToString(), t)));
                foreach (var t in _readTwins)
                {
                    try
                    {
                        // Note: .UniqueId property will throw if not set -- 
                        //   but this should never happen now as we create them when the twin is created
                        //   if one is not already provided
                        _twinsByUniqueId[t.UniqueId] = t;
                        if (t.ExternalId != null) _twinsByExternalId[t.ExternalId] = t;
                        if (t.TrendId != null) _twinsByTrendId[t.TrendId] = t;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogCritical("Error updating uniqueId index for twin {TwinId}: {Message}", t.Id, ex.Message);
                        continue;
                    }
                }
                foreach (var rel in _readTwins.SelectMany(t => t.Relationships))
                {
                    if (rel?.Target?.Id == null)
                    {
                        _logger.LogError("Invalid relationship detected and skipped: {Rel}",
                            rel == null ? "<null>" : JsonSerializer.Serialize(rel));
                        continue;
                    }
                    try
                    {
                        _incommingRels.Add(rel.Target.Id, rel);
                    }
                    catch (Exception ex)
                    {
                        // This should never happen
                        _logger.LogCritical("Error adding incoming rel to index for twin {TwinId}: {Message}", rel.Id, ex.Message);
                        continue;
                    }
                }
            }
        }

        public async Task EnsureTwinCacheLoaded(bool cacheAllowed = true)
        {
            if (_twins == null)
            {
                // TODO: should have a large timeout (20mins?) to abort - or better enable in healthcheck / services manager
                using (await _twinsLockGuard.Lock())
                {
                    try
                    {
                        if (IsLoading)
                        {
                            _logger?.LogError("Reentering load lock!");
                            return;
                        }
                        IsLoading = true;
                        await loadTwinsInLock(cacheAllowed);
                    }
                    finally
                    {
                        IsLoading = false;
                    }
                }
            }
        }

        public async Task Reload()
        {
            _logger?.LogInformation("Starting reload of DigitalTwinCore");
            _twins = null;
            await EnsureTwinCacheLoaded(false);
        }

        private async Task loadTwinsInLock(bool loadCacheAllowed)
        {
            _logger?.LogInformation("Loading twins in lock for {adtInstance}. Local file cache enabled:{LocalFileCacheEnabled}", 
                _instanceSettings.InstanceUri, _enablePersistentCache);


            if (_twins == null)
            {
                var loadedFromCache = false;
                if (_enablePersistentCache && loadCacheAllowed)
                {
                    var twinsList = persistentCacheLoad<IReadOnlyCollection<TwinWithRelationships>>(getCachePath("twins"));
                    if (twinsList != null)
                    {
                        _twins = new ConcurrentDictionary<string, TwinWithRelationships>(
                            twinsList.Select(t => new KeyValuePair<string, TwinWithRelationships>(t.Id, t)));
                    }
                    if (_twins != null)
                    {
                        loadedFromCache = true;
                        _logger?.LogInformation("{count} twins loaded from persisted cache", _twins.Count);
                        // Note that it's possible to have a hybrid mode where we use the 
                        //   saved file immediately for quick startup, then spawn loadTwins
                        //    as a task, and swap-in the updated info from ADT when it's ready
                    }
                }

                if (_twins == null)
                {
                    _logger?.LogInformation($"Loading Twins from ADT");
                    _twins = await loadTwinsFromADTInstance();
                }
                _logger?.LogInformation("Finished Loading {nTwins} Twins from {where}", 
                    _twins.Count, loadedFromCache ? "local file cache" : "ADT instance"); 

                try
                {
                    updateTwinsIndicies();
                }
                catch (Exception ex)
                {
                    // TODO: In cases like this, the server should become unavailable and health-checking should fail
                    _logger?.LogCritical(ex, "Exception caught during site indexing");
                    throw;
                }

                if (_enablePersistentCache && !loadedFromCache)
                {
                    // Note currently we never update the persistent cache after the initial load
                    //   from the ADT instance -- we just recreate this initial state for debugging purposes
                    // To save updates, we could trigger a save in a background thread on a debounced twins changed event
                    persistentCacheSave(_readTwins, getCachePath("twins"));
                }

                try
                {
                    GenerateADTInstanceStats();
                }
                catch (Exception ex)
                {
                    _logger?.LogCritical(ex, "Exception caught during site consistency checking and statistics generation");
                    throw;
                }
            }
        }




        // Site consistency checks and statistics generation
        // Note that when we move to an actual ADT SQL query-based system and we're not pre-loading
        //   from the instance, this functionality would have to be retained as a seperate module or tool,
        //   or we could see if most of these checks could be done with actual queries - however,
        //   some queries may not be possible or be more expensive that just loading the instance 
        //   to do the analysis in memory.

        public async Task<AdtSiteStatsDto> GenerateADTInstanceStats()
        {
            await EnsureTwinCacheLoaded();
            using (await _twinsLockGuard.Lock())
            {
                return _GenerateADTInstanceStats();
            }
        }

        private AdtSiteStatsDto _GenerateADTInstanceStats()
        {
            using (_logger?.BeginScope("generating site statistics for {ntwins} twins", _twins.Count))
            {
                var noSiteTwins = new List<string>();
                var noFloorTwins = new List<string>();
                var noInRelTwins = new HashSet<string>();
                var noOutRelTwins = new HashSet<string>();
                var noUniqueIdTwins = new List<string>();
                // TODO: Have dynamic list of other properties that need to be unique and check them:
                //    trendId, forgeViewerId, externalId, etc.
                var uniqIdCounts = new Dictionary<string, int>();
                foreach (var twin in _readTwins)
                {
                    var uid = twin.UniqueId.ToString();
                    if (!uniqIdCounts.ContainsKey(uid)) uniqIdCounts[uid] = 1; else uniqIdCounts[uid]++;
                    if (twin.UniqueIdFromProperties == null)
                    {
                        noUniqueIdTwins.Add(twin.Id);
                    }
                    // NOTE: Neither of the below are considered an error at the moment, but may be indicative of a data issue
                    if (twin.GetSiteId() == null)
                    {
                        noSiteTwins.Add(twin.Id);
                    }
                    else if (twin.GetSiteId() == Guid.Empty)
                    {
                        _logger.LogError("AdtSiteStat: Site id of all-zeros/default Guid detected for twin: {TwinId}", twin.Id);
                    }
                    if (twin.GetFloorId() == null)
                    {
                        noFloorTwins.Add(twin.Id);
                    }
                    else if (twin.GetFloorId() == Guid.Empty)
                    {
                        _logger.LogError("AdtSiteStat: Floor id of all-zeros/default Guid detected for twin: {TwinId}", twin.Id);
                    }
                    if (twin.Relationships.Count == 0)
                    {
                        noOutRelTwins.Add(twin.Id);
                    }
                    if (!_incommingRels.Get(twin.Id).Any())
                    {
                        noInRelTwins.Add(twin.Id);
                    }
                }

                _logger?.LogInformation("No outgoing rel count: {noOutRelCount}", noOutRelTwins.Count);
                _logger?.LogInformation("No incoming rel count: {noInRelCount}", noInRelTwins.Count);
                var noAnyRels = noInRelTwins.Intersect(noOutRelTwins).ToList();
                _logger?.LogInformation("Orphaned twin (no in or out rels) count: {noRelTwinCount}", noAnyRels.Count);
                if (noAnyRels.Count > 0)
                    _logger?.LogWarning("Orphaned (no in or our rels) twins: {noRelTwins}}", noAnyRels);

                var sites = _readTwins.Select(t => t.GetSiteIdString()).Distinct();
                var siteTwinCounts = sites.ToDictionary(
                    s => s ?? "no-site-found",
                    s => _readTwins.Where(t => t.GetSiteIdString() == s).Count());

                _logger?.LogInformation("Site counts for ADT instance '{inst}': {counts}",
                    _instanceSettings.InstanceUri.AbsoluteUri, JsonSerializer.Serialize(siteTwinCounts));

                var floors = _readTwins.Select(t => t.GetFloorIdString()).Distinct();
                var floorTwinCounts = floors.ToDictionary(
                    f => f ?? "no-floor-found",
                    f => _readTwins.Where(t => t.GetFloorIdString() == f).Count());

                _logger?.LogInformation("Floor counts for ADT instance '{inst}': {counts}",
                    _instanceSettings.InstanceUri.AbsoluteUri, JsonSerializer.Serialize(floorTwinCounts));

                var dupUniqIds = uniqIdCounts.Where(kvp => kvp.Value > 1).Select(kvp => kvp.Key).ToList();
                _logger?.LogInformation("Duplicated uniqueId count: {multUniqTwins}", dupUniqIds.Count);

                foreach (var uidDup in dupUniqIds)
                {
                    var utwins = _readTwins.Where(t => t.UniqueId.ToString() == uidDup).ToList();
                    _logger?.LogWarning("Multiple twins for uniqueID: {uniqueId} detected for twins: {duplicateTwinList}",
                        uidDup, utwins.Select(t => t.Id));
                    var usites = utwins.Select(t => t.GetSiteId()).ToHashSet();
                    if (usites.Count != 1)
                    {
                        _logger?.LogError("Multiple twins for uniqueID: {uid} for different sites: {@dups}",
                            uidDup, utwins.Select(t => new { t.Id, SiteId = t.GetSiteId() }));
                    }
                    utwins.ForEach(t => _errorTwins.AddOrUpdate(
                        t.Id,
                        "Twin does not have unique 'uniqueID'",
                        (k, v) => k + " | " + v));
                }

                var stats = new AdtSiteStatsDto
                {
                    TotalAdtInstanceTwinCount = _readTwins.Count,
                    TotalAdtInstanceRelCount = _readTwins.Sum(t => t.Relationships?.Count ?? 0),
                    NoOutgoingRelTwinCount = noOutRelTwins.Count,
                    NoIncomingRelTwinCount = noInRelTwins.Count,
                    NoIncomingOrOutgoingRelTwinCount = noAnyRels.Count,
                    NoIncomingOrOutgoingRelTwins = noAnyRels,
                    NoUniqueIdTwins = noUniqueIdTwins,
                    SharedUniqueIdTwinCount = dupUniqIds.Count,
                    SharedUniqueIdTwins = dupUniqIds,
                    TwinCountsBySite = siteTwinCounts,
                    TwinCountsByFloor = floorTwinCounts,
                };

                _logger?.LogInformation("DigitalTwinCore statistics for '{instance}': {stats}",
                    _instanceSettings.InstanceUri, JsonSerializer.Serialize(stats));

                return stats;
            }
        }

        private async Task<ConcurrentDictionary<string, TwinWithRelationships>> loadTwinsFromADTInstance()
        {
            _logger?.LogInformation($"Start loading twins from ADT");
            var dtos = await _adtApiService.GetTwins(_instanceSettings);
#if DEBUG
            _tasksTotal = dtos.Count;
#endif
            _logger?.LogInformation($"Loaded {dtos.Count} twins from ADT");

            var relationshipTasks = dtos.Select(GetTwinRelationshipsAsync).ToArray();

            _logger?.LogInformation($"Start loading relationships...");

            var taskResults = await Task.WhenAll(relationshipTasks);
            var relationships = taskResults.SelectMany(r => r).ToList();
            var relationshipsBySrc = relationships.ToLookup(r => r.SourceId);

            _logger?.LogInformation($"Loaded relationships. Start processing twins graph..."); 

            var twinsWithRelationshipsDict = dtos.Select(d =>
                    new TwinWithRelationships
                    {
                        Id = d.Id,
                        CustomProperties = Twin.MapCustomProperties(d.Contents),
                        Metadata = TwinMetadata.MapFrom(d.Metadata)
                    })
                .ToDictionary(t => t.Id);

            Parallel.ForEach(twinsWithRelationshipsDict, twinKV =>
            {
                var twinRelationships = relationshipsBySrc[twinKV.Key];
                twinKV.Value.Relationships = twinRelationships.Select(r =>
                    new TwinRelationship
                    {
                        Id = r.Id,
                        Name = r.Name,
                        Target = twinsWithRelationshipsDict[r.TargetId],
                        Source = twinKV.Value,
                        CustomProperties = Twin.MapCustomProperties(r.Properties)
                    }
               ).ToList().AsReadOnly();
            });

            var relCount = twinsWithRelationshipsDict.Values.Sum(twr => twr.Relationships.Count);
            _logger?.LogInformation($"Loaded {relCount} relationships");

            _logger?.LogInformation($"Done post-processing twins graph");
            return new ConcurrentDictionary<string, TwinWithRelationships>(twinsWithRelationshipsDict);
        }

        private Task<List<BasicRelationship>> GetTwinRelationshipsAsync(BasicDigitalTwin twin)
        {
            return Task.Run(async () =>
            {
                try
                {
                    // These are globals to avoid creating a separate closure for each task
#if DEBUG
                    Debug.WriteLine($"TwinRelLoad # {++_tasksComplete} of {_tasksTotal}");
#endif
                    return await _adtApiService.GetRelationships(_instanceSettings, twin.Id);
                }
                catch (Exception)
                {
                    return new List<BasicRelationship>();
                }
            });
        }

        // Note that Update is for base twin properties -- not for relationship changes
        public async Task<TwinWithRelationships> UpdateCachedTwinAsync(Twin twin)
        {
            await EnsureTwinCacheLoaded();
            var cachedTwin = _twins.GetOrAdd(twin.Id, id => 
            {
                // Create new twin, update index and reset _readTwins
                var newTwin = new TwinWithRelationships { Id = twin.Id};
                // This is always true, as we don't have a Guid?
                if (twin.UniqueId != null) 
                {
                    _twinsByUniqueId[twin.UniqueId] = newTwin;
                    if (twin.ExternalId != null) _twinsByExternalId[twin.ExternalId] = newTwin;
                    if (twin.TrendId != null) _twinsByTrendId[twin.TrendId] = newTwin;
                    _readTwins = null; 
                }
                return newTwin;
            });
            // Upsert twin data
            cachedTwin.Metadata = twin.Metadata;
            cachedTwin.CustomProperties = twin.CustomProperties;
            return cachedTwin;
        }

        private (TwinWithRelationships tSource, TwinWithRelationships tTarget) getRelationshipTwins( Relationship rel)
        {
            TwinWithRelationships sourceTwin, targetTwin;
            lock (_twins)
            {
                if (!_twins.TryGetValue(rel.SourceId, out sourceTwin))
                {
                    throw new ResourceNotFoundException("Twin", rel.SourceId);
                }
                if (!_twins.TryGetValue(rel.TargetId, out targetTwin))
                {
                    throw new ResourceNotFoundException("Twin", rel.TargetId);
                }
            }
            return (sourceTwin, targetTwin);
        }

        public async Task<TwinRelationship> AddCachedRelationship(Relationship relationship)
        {
            await EnsureTwinCacheLoaded();
            var (sourceTwin, targetTwin) = getRelationshipTwins(relationship);

            var twinRelationship = new TwinRelationship
            {
                Id = relationship.Id,
                CustomProperties = relationship.CustomProperties,
                Name = relationship.Name,
                Target = targetTwin,
                Source = sourceTwin
            };

            lock (sourceTwin.Relationships)
            {
                sourceTwin.Relationships = 
                    sourceTwin.Relationships.Append(twinRelationship).ToList().AsReadOnly();
            }
            _incommingRels.Add(twinRelationship.Target.Id, twinRelationship);

            return twinRelationship;
        }

        public async Task<TwinWithRelationships> GetTwinByIdAsync(string id)
        {
            await EnsureTwinCacheLoaded();
            return _twins.TryGetValue(id, out var twin) ? twin : null;
        }

        public async Task<TwinWithRelationships> GetTwinByUniqueIdAsync(Guid uniqueId)
        {
            await EnsureTwinCacheLoaded();
            _twinsByUniqueId.TryGetValue(uniqueId, out var twin);
            // We could just return the "winning" twin here, but we will retain the 
            //  semantics of .FirstOrDefault() and complain if we have a duplicate mapping 
            if (twin != null && _errorTwins.ContainsKey(twin.Id))
            {
                throw new BadRequestException( _errorTwins[twin.Id]);
            }
            return twin;
        }

        public async Task<TwinWithRelationships> GetTwinByExternalIdAsync(string externalId)
        {
            await EnsureTwinCacheLoaded();
            _twinsByExternalId.TryGetValue(externalId, out var twin);
            return (TwinWithRelationships) twin;
        }

        public async Task<TwinWithRelationships> GetTwinByTrendIdAsync(Guid trendId)
        {
            await EnsureTwinCacheLoaded();
            _twinsByTrendId.TryGetValue(trendId.ToString(), out var twin);
            return (TwinWithRelationships) twin;
        }

        public async Task<TwinWithRelationships> GetTwinByForgeViewerIdAsync(string id)
        {
            await EnsureTwinCacheLoaded();

            // TODO: Consider another secondary index if we do this often
            return ReadTwins.FirstOrDefault(t => id.Equals(t.GetStringProperty(Properties.GeometryViewerId), StringComparison.InvariantCultureIgnoreCase));
        }


        public async Task<List<TwinRelationship>> GetIncomingRelationshipsAsync(string twinId)
        {
            await EnsureTwinCacheLoaded();

            var found = _twins.TryGetValue(twinId, out var sourceTwin);
            if (!found)
            {
                return null;
            }
            return _incommingRels.Get(twinId).ToList();
        }

        public async Task<TwinRelationship> UpdateCachedRelationship(Relationship relationship)
        {
            await EnsureTwinCacheLoaded();
            var (sourceTwin, targetTwin) = getRelationshipTwins(relationship);

            var twinRelationship = sourceTwin.Relationships.SingleOrDefault(r => r.Id == relationship.Id);
            if (twinRelationship == null)
            {
                twinRelationship = new TwinRelationship
                {
                    Id = relationship.Id,
                    Name = relationship.Name,
                    Source = sourceTwin,
                    Target = targetTwin,
                    CustomProperties = relationship.CustomProperties
                };
                lock (sourceTwin.Relationships)
                {
                    sourceTwin.Relationships =
                        sourceTwin.Relationships.Append(twinRelationship).ToList().AsReadOnly();
                }
            }
            else
            {
                twinRelationship.Name = relationship.Name;
                // TODO: does this happen??
                if (twinRelationship.Target.Id != targetTwin.Id)
                {
                    _incommingRels.TryRemoveFirst(twinRelationship.Target.Id, tr => tr.Id == twinRelationship.Id);
                }
                twinRelationship.Target = targetTwin;
                twinRelationship.CustomProperties = relationship.CustomProperties;
            }
            _incommingRels.Add(twinRelationship.Target.Id, twinRelationship);
            return twinRelationship;
        }

        public async Task RemoveCachedTwinAsync(string id)
        {
            await EnsureTwinCacheLoaded();

            _twins.TryRemove(id, out var _removedTwin);
            if (_removedTwin != null)
            {
                _twinsByUniqueId.TryRemove(_removedTwin.UniqueId, out var _);
                if (_removedTwin.ExternalId !=  null) _twinsByExternalId.TryRemove(_removedTwin.ExternalId, out var _);
                if (_removedTwin.TrendId !=  null) _twinsByTrendId.TryRemove(_removedTwin.TrendId, out var _);
                _readTwins = null;
                foreach (var rel in _removedTwin.Relationships)
                {
                    _incommingRels.RemoveAll( rel.Target.Id);
                }
            }
        }

        public async Task RemoveCachedRelationshipAsync(string twinId, string relationshipId)
        {
            await EnsureTwinCacheLoaded();

            if (!_twins.TryGetValue(twinId, out var twin))
            {
                throw new ResourceNotFoundException("Twin", twinId);
            }

            // twin.Relationships is already thread-safe, but wrap find & set on the off chance we get another add/del at the same time
            lock (twin.Relationships)
            {
                var twinRelationship = twin.Relationships.SingleOrDefault(r => r.Id == relationshipId);
                if (twinRelationship != null)
                {
                    twin.Relationships =
                        twin.Relationships.Where(t => t != twinRelationship).ToList().AsReadOnly();
                    _incommingRels.TryRemoveFirst(twinRelationship.Target.Id, tr => tr.Id == twinRelationship.Id);
                }
            }
        }

        #region ========== Models ===========

        public List<Model> GetModels()
        {
            EnsureModelCacheLoaded();

            lock (_modelsLockGuard)
            {
                return _models.Values.ToList();
            }
        }

        public Model GetModel(string id)
        {
            EnsureModelCacheLoaded();

            lock (_modelsLockGuard)
            {
                return _models.ContainsKey(id) ? _models[id] : null;
            }
        }

        private void EnsureModelCacheLoaded()
        {
            if (_models == null)
            {
                lock (_modelsLockGuard)
                {
                    if (_models == null)
                    {
                        var dtos = _adtApiService.GetModels(_instanceSettings);
                        _models = Model.MapFrom(dtos).ToDictionary(m => m.Id);
                    }
                }
            }
        }

        public Model UpdateCachedModel(Model model)
        {
            EnsureModelCacheLoaded();

            lock (_modelsLockGuard)
            {
                _models[model.Id] = model;
                return model;
            }
        }

        public void RemoveCachedModel(string id)
        {
            EnsureModelCacheLoaded();

            lock (_modelsLockGuard)
            {
                if (_models.ContainsKey(id))
                {
                    _models.Remove(id);
                }
            }
        }

        #endregion Models

        #region ===========   Local persistent cache methods =============

        private string getCachePath(string name) 
        {
            var inst = _instanceSettings.InstanceUri.AbsoluteUri.Replace("https:", "").Replace("/", "");
            return Path.GetFullPath(Path.Combine(
                // %TEMP% will be set to local filesystem of AppService
                Environment.GetEnvironmentVariable("TEMP") ?? ".",
                $"dtcore_cache_{name}_{inst}"));
        }

        private void persistentCacheSave(object item, string path)
        {
            using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
            try
            {
                JsonSerializer.Serialize(stream, item);
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Error saving cache to {path}: {ex}");
                // Allow this to fail to ensure service start
            }

            _logger?.LogInformation($"Saved cache to {path}");
        }

        private T persistentCacheLoad<T>(string path) where T : class
        {
            if (string.Equals( _persistentCacheMode, "saveOnly", StringComparison.InvariantCultureIgnoreCase))
            {
                _logger?.LogWarning("Persistent cache in saveOnly mode -- not loading from cache");
                return null;
            }
            if (!File.Exists(path))
            {
                _logger?.LogInformation($"Local cache does not exist: {path}");
                return null;
            }

            _logger?.LogInformation($"Attempting to load cache from {path}");

            T item = null;
            try
            {
                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                item = (T) JsonSerializer.Deserialize<T>(stream);
            }
            catch (Exception ex)
            {
                _logger?.LogError($"   Error loading cache from {path}: {ex} -- attempting to delete file");
                try
                {
                    File.Delete(path);
                }
                catch (Exception exDel)
                {
                    _logger?.LogError($"   Error deleting bad cache file {path}: {exDel}");
                }
                return null;
            }

            _logger?.LogInformation($"{DateTime.Now}   Loaded cache from {path}");
            return item;
        }

#endregion

    }
}
