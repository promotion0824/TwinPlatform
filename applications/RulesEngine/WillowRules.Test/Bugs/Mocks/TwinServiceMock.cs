using Azure;
using Azure.DigitalTwins.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Willow.Rules.Model;
using Willow.Rules.Services;

namespace WillowRules.Test.Bugs.Mocks;

public class TwinServiceMock : ITwinService
{
	private readonly ITwinService twinService;
	public string LastADTQuery = "";

	public TwinServiceMock(ITwinService twinService)
	{
		this.twinService = twinService;
	}

	public Dictionary<string, object> ConvertFromSystemTextJsonToRealObject(Dictionary<string, object> contents)
	{
		throw new NotImplementedException();
	}

	public Task<int> CountCached()
	{
		throw new NotImplementedException();
	}

	public IAsyncEnumerable<BasicDigitalTwinPoco> GetAllCached()
	{
		throw new NotImplementedException();
	}

	public Task<List<Edge>> GetCachedBackwardRelatedTwins(string id)
	{
		return twinService.GetCachedBackwardRelatedTwins(id);
	}

	public Task<List<Edge>> GetCachedForwardRelatedTwins(string id)
	{
		return twinService.GetCachedForwardRelatedTwins(id);
	}

	public Task<BasicDigitalTwinPoco?> GetCachedTwin(string id)
	{
		return twinService.GetCachedTwin(id);
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
		return twinService.GetTwinsByModelWithInheritance(modelId);
	}

	public Task<BasicDigitalTwinPoco?> GetUncachedTwin(string id)
	{
		return twinService.GetCachedTwin(id);
	}

	public Task<bool> IsRelationshipAllowed(BasicRelationship rel)
	{
		throw new NotImplementedException();
	}

	public Task<List<BasicDigitalTwinPoco>> Query(string query, string? twinOutputField = null)
	{
		LastADTQuery = query;
		return twinService.GetAllCached().ToListAsync().AsTask();
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
}
