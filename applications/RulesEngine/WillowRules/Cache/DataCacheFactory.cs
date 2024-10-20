using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Willow.Rules.Configuration;
using Willow.Rules.Model;
using WillowRules.DTO;

namespace Willow.Rules.Cache;

/// <summary>
/// Disk cache factory handles by-name resolution of the various disk caches
/// </summary>
public interface IDataCacheFactory
{
	/// <summary>
	/// A cache of model data
	/// </summary>
	IDataCache<ModelData> Models { get; }

	/// <summary>
	/// A cache of model system graphs
	/// </summary>
	IDataCache<SerializableGraph<MetaGraphNode, MetaGraphRelation>> MetaSystemGraphs { get; }

	/// <summary>
	/// A cache of the whole meta graph
	/// </summary>
	IDataCache<SerializableGraph<MetaGraphNode, MetaGraphRelation>> MetaModelGraph { get; }

	/// <summary>
	/// A cache for ontology graphs
	/// </summary>
	IDataCache<ModelSimpleGraphDto> OntologyCache { get; }

	/// <summary>
	/// A cache of all models as a single object
	/// </summary>
	IDataCache<CollectionWrapper<ModelData>> AllModelsCache { get; }

	/// <summary>
	/// A cache of BasicDigitalTwinPoco fetched from ADT
	/// </summary>
	IDataCache<BasicDigitalTwinPoco> TwinCache { get; }

	/// <summary>
	/// A cache of all relationships
	/// </summary>
	IDataCache<ExtendedRelationship> ExtendedRelationships { get; }

	/// <summary>
	/// A cache of edges outbound from a digital twin
	/// </summary>
	IDataCache<CollectionWrapper<Edge>> ForwardEdgeCache { get; }

	/// <summary>
	/// A cache of edges inbound from a digital twin
	/// </summary>
	IDataCache<CollectionWrapper<Edge>> BackEdgeCache { get; }

	/// <summary>
	/// A cache of twin system graphs (feeds, fedby, located in, ...)
	/// </summary>
	IDataCache<SerializableGraph<BasicDigitalTwinPoco, WillowRelation>> TwinSystemGraphCache { get; }

	/// <summary>
	/// A cache of models with inheritance
	/// </summary>
	IDataCache<CollectionWrapper<BasicDigitalTwinPoco>> DiskCacheTwinsByModelWithInheritance { get; }

	/// <summary>
	/// A cache of models with inheritance
	/// </summary>
	IDataCache<CollectionWrapper<BasicDigitalTwinPoco>> AdtQueryResult { get; }

	/// <summary>
	/// Creates a new named DiskCache
	/// </summary>
	/// <remarks>
	/// We need a named cache to distinguish between two caches of the same type
	/// </remarks>
	IDataCache<T> Get<T>(string name) where T : notnull;

	/// <summary>
	/// Creates a new named DiskCache with a maxage
	/// </summary>
	/// <remarks>
	/// We need a named cache to distinguish between two caches of the same type
	/// </remarks>
	IDataCache<T> Get<T>(string name, TimeSpan maxAge, CachePolicy diskCachePolicy, MemoryCachePolicy memoryCachePolicy) where T : notnull;

	/// <summary>
	/// Returns the total rows in the cache table
	/// </summary>
	/// <returns></returns>
	Task<int> GetTotalCacheCount();

	/// <summary>
	/// Clears any caches that was not updated after the provided update date
	/// </summary>
	Task ClearCacheBefore(DateTimeOffset lastUpdated);
}

/// <summary>
/// Disk cache factory handles by-name resolution of the various disk caches
/// </summary>
/// <remarks>
/// dotnet core dependency injection doesn't handle named dependencies unlike Autofac
/// </remarks>
public class DataCacheFactory : IDataCacheFactory
{
	/// <summary>
	/// Max Age objects hang around until the cache is reloaded, these are the base cached layer
	/// </summary>
	private readonly TimeSpan maxAge;

	/// <summary>
	/// Short renewal objects get reloaded often but may rely on longer-lived objects
	/// </summary>
	private readonly TimeSpan shortRenewal = TimeSpan.FromHours(1);
	private readonly IMemoryCache memoryCache;
	private readonly IRulesDistributedCache rulesDistributedCache;
	private readonly ILoggerFactory loggerFactory;

	public DataCacheFactory(
		IMemoryCache memoryCache,
		IRulesDistributedCache rulesDistributedCache,
		ILoggerFactory loggerFactory)
	{
		this.memoryCache = memoryCache ?? throw new System.ArgumentNullException(nameof(memoryCache));
		this.loggerFactory = loggerFactory ?? throw new System.ArgumentNullException(nameof(loggerFactory));
		this.rulesDistributedCache = rulesDistributedCache ?? throw new System.ArgumentNullException(nameof(rulesDistributedCache));
		this.maxAge = TimeSpan.MaxValue;//no need to expire. Auto cache refresh will now occur
	}

	/// <summary>
	/// Creates a new named DiskCache (signature  used only for ActorState)
	/// </summary>
	public IDataCache<T> Get<T>(string name) where T : notnull
	{
		return new DataCache<T>(name, TimeSpan.MaxValue, CachePolicy.EagerReload, MemoryCachePolicy.NoMemoryCache, memoryCache, rulesDistributedCache, loggerFactory);
	}

	/// <summary>
	/// Creates a new named DiskCache with a cache expiration
	/// </summary>
	public IDataCache<T> Get<T>(string name, TimeSpan maxAge, CachePolicy diskCachePolicy, MemoryCachePolicy memoryCachePolicy) where T : notnull
	{
		return new DataCache<T>(name, maxAge, diskCachePolicy, memoryCachePolicy, memoryCache, rulesDistributedCache, loggerFactory);
	}

	public Task<int> GetTotalCacheCount()
	{
		return rulesDistributedCache.CountAsync("");
	}

	public Task ClearCacheBefore(DateTimeOffset lastUpdated)
	{
		return rulesDistributedCache.RemoveAsync("", lastUpdated);
	}

	public IDataCache<ModelData> Models =>
		this.Get<ModelData>("models2", maxAge, CachePolicy.LazyReload, MemoryCachePolicy.WithMemoryCache);

	public IDataCache<BasicDigitalTwinPoco> TwinCache =>
		this.Get<BasicDigitalTwinPoco>("twins2", maxAge, CachePolicy.LazyReload, MemoryCachePolicy.WithMemoryCache);

	public IDataCache<ExtendedRelationship> ExtendedRelationships =>
		this.Get<ExtendedRelationship>("relationships2", maxAge, CachePolicy.LazyReload, MemoryCachePolicy.NoMemoryCache);

	public IDataCache<CollectionWrapper<ModelData>> AllModelsCache =>
		this.Get<CollectionWrapper<ModelData>>("models2", maxAge, CachePolicy.LazyReload, MemoryCachePolicy.NoMemoryCache);

	public IDataCache<ModelSimpleGraphDto> OntologyCache =>
		this.Get<ModelSimpleGraphDto>("ontology", maxAge, CachePolicy.LazyReload, MemoryCachePolicy.NoMemoryCache);

	public IDataCache<CollectionWrapper<BasicDigitalTwinPoco>> DiskCacheTwinsByModelWithInheritance =>
		this.Get<CollectionWrapper<BasicDigitalTwinPoco>>("inheritedtwinsbymodel", maxAge, CachePolicy.LazyReload, MemoryCachePolicy.NoMemoryCache);

	public IDataCache<CollectionWrapper<BasicDigitalTwinPoco>> AdtQueryResult =>
		this.Get<CollectionWrapper<BasicDigitalTwinPoco>>("adtqueryresult", maxAge, CachePolicy.LazyReload, MemoryCachePolicy.WithMemoryCache);

	public IDataCache<CollectionWrapper<BasicDigitalTwinPoco>> DiskCacheTwinsByModelConcrete =>
		this.Get<CollectionWrapper<BasicDigitalTwinPoco>>("twinsbymodelconcrete", maxAge, CachePolicy.LazyReload, MemoryCachePolicy.NoMemoryCache);

	public IDataCache<SerializableGraph<MetaGraphNode, MetaGraphRelation>> MetaModelGraph =>
		this.Get<SerializableGraph<MetaGraphNode, MetaGraphRelation>>("metamodelgraph2", maxAge, CachePolicy.LazyReload, MemoryCachePolicy.NoMemoryCache);

	public IDataCache<SerializableGraph<MetaGraphNode, MetaGraphRelation>> MetaSystemGraphs =>
		this.Get<SerializableGraph<MetaGraphNode, MetaGraphRelation>>("systems2", maxAge, CachePolicy.LazyReload, MemoryCachePolicy.NoMemoryCache);

	public IDataCache<CollectionWrapper<Edge>> ForwardEdgeCache =>
		this.Get<CollectionWrapper<Edge>>("edgeforward2", maxAge, CachePolicy.LazyReload, MemoryCachePolicy.WithMemoryCache);

	public IDataCache<CollectionWrapper<Edge>> BackEdgeCache =>
		this.Get<CollectionWrapper<Edge>>("edgebackward2", maxAge, CachePolicy.LazyReload, MemoryCachePolicy.WithMemoryCache);

	public IDataCache<SerializableGraph<MiniTwinDto, WillowRelation>> TwinIdsGraphCache =>
		this.Get<SerializableGraph<MiniTwinDto, WillowRelation>>("twinIdsGraphMini", maxAge, CachePolicy.LazyReload, MemoryCachePolicy.NoMemoryCache);

	public IDataCache<SerializableGraph<BasicDigitalTwinPoco, WillowRelation>> TwinSystemGraphCache =>
		this.Get<SerializableGraph<BasicDigitalTwinPoco, WillowRelation>>("twinsystemgraph4",
			maxAge,
			CachePolicy.LazyReload, MemoryCachePolicy.NoMemoryCache);//add memory cache here, it helps re-using these graphs during twin binding visitor
}
