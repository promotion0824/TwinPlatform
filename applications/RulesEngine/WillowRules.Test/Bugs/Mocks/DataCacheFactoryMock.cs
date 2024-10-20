using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Willow.Rules.Cache;
using Willow.Rules.Model;
using WillowRules.DTO;

namespace WillowRules.Test.Bugs.Mocks;

public class DataCacheMock<T> : IDataCache<T>
{
	public ConcurrentDictionary<string, T> Data { get; init; } = new();

	public Task<T> AddOrUpdate(string willowEnvironment, string id, T serializableVersion)
	{
		Data[id] = serializableVersion;

		return Task.FromResult(serializableVersion);
	}

	public Task<bool> Any(string willowEnvironmentId)
	{
		return Task.FromResult(Data.Count > 0);
	}

	public IAsyncEnumerable<T> GetAll(string willowEnvironment, int maxParallelism = 40)
	{
		return Data.Values.ToAsyncEnumerable();
	}

	public Task<int> Count(string willowEnvironment, int maxParallelism = 40)
	{
		return Task.FromResult(Data.Values.Count);
	}

	public Task<T?> GetOrCreateAsync(string willowEnvironment, string id, Func<Task<T?>> create)
	{
		if (Data.TryGetValue(id, out var value))
		{
			return Task.FromResult<T?>(value);
		}

		value = create().Result;

		Data.TryAdd(id, value!);

		return Task.FromResult<T?>(value);
	}

	public Task RemoveItems(string willowEnvironmentId, DateTimeOffset lastUpdated)
	{
		return Task.CompletedTask;
	}

	public Task RemoveKey(string willowEnvironment, string id)
	{
		return Task.CompletedTask;
	}

	public Task<(bool ok, T? result)> TryGetValue(string willowEnvironment, string id)
	{
		if (Data.TryGetValue(id, out var value))
		{
			return Task.FromResult<(bool ok, T? result)>((true, value));
		}

		return Task.FromResult<(bool ok, T? result)>((false, default(T)));
	}
}

public class DataCacheFactoryMock : IDataCacheFactory
{
	public IDataCache<ModelData> Models { get; set; } = new DataCacheMock<ModelData>();

	public IDataCache<SerializableGraph<MetaGraphNode, MetaGraphRelation>> MetaSystemGraphs { get; set; } = new DataCacheMock<SerializableGraph<MetaGraphNode, MetaGraphRelation>>();

	public IDataCache<SerializableGraph<MetaGraphNode, MetaGraphRelation>> MetaModelGraph { get; set; } = new DataCacheMock<SerializableGraph<MetaGraphNode, MetaGraphRelation>>();

	public IDataCache<ModelSimpleGraphDto> OntologyCache { get; set; } = new DataCacheMock<ModelSimpleGraphDto>();

	public IDataCache<CollectionWrapper<ModelData>> AllModelsCache { get; set; } = new DataCacheMock<CollectionWrapper<ModelData>>();

	public IDataCache<BasicDigitalTwinPoco> TwinCache { get; set; } = new DataCacheMock<BasicDigitalTwinPoco>();

	public IDataCache<ExtendedRelationship> ExtendedRelationships { get; set; } = new DataCacheMock<ExtendedRelationship>();

	public IDataCache<CollectionWrapper<Edge>> ForwardEdgeCache { get; set; } = new DataCacheMock<CollectionWrapper<Edge>>();

	public IDataCache<CollectionWrapper<Edge>> BackEdgeCache { get; set; } = new DataCacheMock<CollectionWrapper<Edge>>();

	public IDataCache<SerializableGraph<BasicDigitalTwinPoco, WillowRelation>> TwinGraphCache { get; set; } = new DataCacheMock<SerializableGraph<BasicDigitalTwinPoco, WillowRelation>>();

	public IDataCache<SerializableGraph<MiniTwinDto, WillowRelation>> TwinIdsGraphCache =>
		new DataCacheMock<SerializableGraph<MiniTwinDto, WillowRelation>>();

	public IDataCache<SerializableGraph<BasicDigitalTwinPoco, WillowRelation>> TwinSystemGraphCache { get; set; } = new DataCacheMock<SerializableGraph<BasicDigitalTwinPoco, WillowRelation>>();

	public IDataCache<CollectionWrapper<BasicDigitalTwinPoco>> DiskCacheTwinsByModelWithInheritance { get; set; } = new DataCacheMock<CollectionWrapper<BasicDigitalTwinPoco>>();

	public IDataCache<ActorState> ActorStateCache { get; set; } = new DataCacheMock<ActorState>();

	public IDataCache<CollectionWrapper<BasicDigitalTwinPoco>> AdtQueryResult { get; set; } = new DataCacheMock<CollectionWrapper<BasicDigitalTwinPoco>>();

	public Task ClearCacheBefore(DateTimeOffset lastUpdated)
	{
		throw new NotImplementedException();
	}

	public IDataCache<T> Get<T>(string name) where T : notnull
	{
		throw new NotImplementedException();
	}

	public IDataCache<T> Get<T>(string name, TimeSpan maxAge, CachePolicy diskCachePolicy, MemoryCachePolicy memoryCachePolicy) where T : notnull
	{
		throw new NotImplementedException();
	}

	public Task<int> GetTotalCacheCount()
	{
		throw new NotImplementedException();
	}
}
