using Azure;
using Azure.DigitalTwins.Core;
using Newtonsoft.Json;
using RulesEngine.UtilsApi.DTO;
using System.Text.Json;
using Willow.Rules.Cache;
using Willow.Rules.Model;
using Willow.Rules.Services;
using Willow.Rules.Sources;
using WillowRules.DTO;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace RulesEngineUtils.Api.Mocks;

public class TwinServiceMock : ITwinService
{
	public Dictionary<string, object> ConvertFromSystemTextJsonToRealObject(Dictionary<string, object> contents)
	{
		throw new NotImplementedException();
	}

	public IAsyncEnumerable<BasicDigitalTwinPoco> GetAllCached()
	{
		throw new NotImplementedException();
	}

	public Task<int> CountCached()
	{
		throw new NotImplementedException();
	}

	public virtual Task<List<Edge>> GetCachedBackwardRelatedTwins(string id)
	{
		throw new NotImplementedException();
	}

	public virtual Task<List<Edge>> GetCachedForwardRelatedTwins(string id)
	{
		throw new NotImplementedException();
	}

	public virtual Task<BasicDigitalTwinPoco?> GetCachedTwin(string id)
	{
		throw new NotImplementedException();
	}

	public virtual Task<BasicDigitalTwinPoco?> GetUncachedTwin(string id)
	{
		throw new NotImplementedException();
	}

	public Task<(BasicDigitalTwinPoco? twin, IEnumerable<Edge> forward, IEnumerable<Edge> reverse)> GetDigitalTwinWithRelationshipsAsync(string dtid)
	{
		throw new NotImplementedException();
	}

	public Task<List<BasicDigitalTwinPoco>> GetTopLevelEntities()
	{
		throw new NotImplementedException();
	}

	public Task<List<BasicDigitalTwinPoco>> GetTwinsByModelWithInheritance(string modelId)
	{
		throw new NotImplementedException();
	}

	public AsyncPageable<ExtendedRelationship> QueryAllRelationships(DigitalTwinsClient digitalTwinsClient)
	{
		throw new NotImplementedException();
	}

	public bool TryGetObjectFromJElement(JsonElement r, out object? obj)
	{
		throw new NotImplementedException();
	}

	public void ValidateTwin(BasicDigitalTwinPoco twin)
	{
		throw new NotImplementedException();
	}

	public Task<bool> IsRelationshipAllowed(BasicRelationship rel)
	{
		throw new NotImplementedException();
	}

	public Task<List<BasicDigitalTwinPoco>> Query(string query, string? twinField = null)
	{
		throw new NotImplementedException();
	}
}

public class TwinServiceWithHttpClient : TwinServiceMock
{
	private Dictionary<string, BasicDigitalTwinPoco> data = new();
	private Dictionary<string, ModelData> modelData = new();
	private readonly HttpClient client;
	private readonly string baseUrl;
	private readonly IDataCacheFactory dataCacheFactory;
	private readonly WillowEnvironment willowEnvironment;

	public TwinServiceWithHttpClient(
		WillowEnvironment willowEnvironment,
		IDataCacheFactory dataCacheFactory,
		HttpClient client,
		string baseUrl)
	{
		if (string.IsNullOrEmpty(baseUrl))
		{
			throw new ArgumentException($"'{nameof(baseUrl)}' cannot be null or empty.", nameof(baseUrl));
		}

		this.baseUrl = baseUrl;
		this.client = client ?? throw new ArgumentNullException(nameof(client));
		this.dataCacheFactory = dataCacheFactory ?? throw new ArgumentNullException(nameof(dataCacheFactory));
		this.willowEnvironment = willowEnvironment ?? throw new ArgumentNullException(nameof(willowEnvironment));
	}

	public override async Task<List<Edge>> GetCachedBackwardRelatedTwins(string id)
	{
		await GetOrAddTwin(id);

		(_, var result) = await dataCacheFactory.BackEdgeCache.TryGetValue(willowEnvironment.Id, id);

		return result?.Items ?? new List<Edge>();
	}

	public override async Task<List<Edge>> GetCachedForwardRelatedTwins(string id)
	{
		await GetOrAddTwin(id);

		(_, var result) = await dataCacheFactory.ForwardEdgeCache.TryGetValue(willowEnvironment.Id, id);

		return result?.Items ?? new List<Edge>();
	}

	public override Task<BasicDigitalTwinPoco?> GetCachedTwin(string id)
	{
		return GetOrAddTwin(id);
	}

	public override Task<BasicDigitalTwinPoco?> GetUncachedTwin(string id)
	{
		return GetOrAddTwin(id);
	}

	private async Task<BasicDigitalTwinPoco?> GetOrAddTwin(string twinId)
	{
		if (data.TryGetValue(twinId, out var existingTwin))
		{
			return existingTwin;
		}

		var twins = new Dictionary<string, BasicDigitalTwinPoco>();

		(var equipment, var twin) = await GetEquipment(twinId);

		var graph = new SerializableGraph<BasicDigitalTwinPoco, WillowRelation>()
		{
			Nodes = new List<BasicDigitalTwinPoco>(),
			Edges = new List<SerializableEdge<WillowRelation>>()
		};

		graph.Nodes.Add(twin);

		var backEdges = new List<Edge>();
		var fwdEdges = new List<Edge>();

		foreach (var entity in equipment
			.InverseRelatedEntities
			.Where(v => v.id != twin.Id))
		{
			(_, var relatedTwin) = await GetEquipment(entity.id);

			twins[entity.id] = relatedTwin;

			graph.Nodes.Add(twins[entity.id]);

			graph.Edges.Add(new SerializableEdge<WillowRelation>()
			{
				StartId = entity.id,
				EndId = twin.Id,
				Edge = WillowRelation.Get(entity.relationship, entity.substance)
			});

			backEdges.Add(new Edge()
			{
				RelationshipType = entity.relationship,
				Destination = twins[entity.id]
			});

			await AddModelId(relatedTwin);
		}

		foreach (var entity in equipment
			.RelatedEntities
			.Where(v => v.id != twin.Id))
		{
			(_, var relatedTwin) = await GetEquipment(entity.id);

			twins[entity.id] = relatedTwin;

			graph.Nodes.Add(twins[entity.id]);

			graph.Edges.Add(new SerializableEdge<WillowRelation>()
			{
				StartId = twin.Id,
				EndId = entity.id,
				Edge = WillowRelation.Get(entity.relationship, entity.substance)
			});

			fwdEdges.Add(new Edge()
			{
				RelationshipType = entity.relationship,
				Destination = twins[entity.id]
			});

			await AddModelId(relatedTwin);
		}

		await AddModelId(twin);

		await dataCacheFactory.TwinCache.AddOrUpdate(willowEnvironment.Id, twin.Id, twin);
		await dataCacheFactory.TwinSystemGraphCache.AddOrUpdate(willowEnvironment.Id, twin.Id, graph);
		await dataCacheFactory.BackEdgeCache.AddOrUpdate(willowEnvironment.Id, twin.Id, new CollectionWrapper<Edge>(backEdges.ToList()));
		await dataCacheFactory.ForwardEdgeCache.AddOrUpdate(willowEnvironment.Id, twin.Id, new CollectionWrapper<Edge>(fwdEdges.ToList()));

		data[twin.Id] = twin;

		return twin;
	}

	private async Task AddModelId(BasicDigitalTwinPoco twin)
	{
		if (!modelData.ContainsKey(twin.Metadata.ModelId))
		{
			var models = (await dataCacheFactory.AllModelsCache.GetAll(willowEnvironment.Id).ToListAsync()).FirstOrDefault()?.Items ?? new List<ModelData>();

			var model = await GetModel(twin.Metadata.ModelId);

			if (model is not null)
			{
				modelData[twin.Metadata.ModelId] = model;

				models.Add(model);

				if (model.DtdlModel.extends is not null)
				{
					foreach (var modelId in model.DtdlModel.extends)
					{
						if (!modelData.ContainsKey(modelId))
						{
							model = await GetModel(modelId);
							modelData[modelId] = model;
							models.Add(model);
						}
					}
				}

				if (model.DtdlModel.contents is not null)
				{
					foreach (var content in model.DtdlModel.contents)
					{
						if (!string.IsNullOrEmpty(content.target) && !modelData.ContainsKey(content.target))
						{
							model = await GetModel(content.target);
							modelData[content.target] = model;
							models.Add(model);
						}
					}
				}

				await dataCacheFactory.AllModelsCache.AddOrUpdate(willowEnvironment.Id, "allmodels4", new CollectionWrapper<ModelData>(models));
			}
		}
	}

	private async Task<(EquipmentDto equipment, BasicDigitalTwinPoco twin)> GetEquipment(string id)
	{
		if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));

		var equipmentJson = await client.GetStringAsync($"{baseUrl}/api/Twin/EquipmentWithRelationships?equipmentId={id}");

		var equipment = JsonConvert.DeserializeObject<EquipmentDto>(equipmentJson)!;

		var twin = new BasicDigitalTwinPoco(id)
		{
			name = equipment.Name,
			trendID = equipment.TrendId,
			connectorID = equipment.ConnectorId,
			externalID = equipment.ExternalId,
			unit = equipment.Unit,
			Contents = equipment.Contents,
			Metadata = new DigitalTwinMetadataPoco()
			{
				ModelId = equipment.ModelId
			}
		};

		return (equipment, twin);
	}

	private async Task<ModelData> GetModel(string modelId)
	{
		var modelJson = await client.GetStringAsync($"{baseUrl}/api/model/model?modelId={modelId}");

		return JsonConvert.DeserializeObject<ModelData>(modelJson)!;
	}
}
