using Azure;
using Azure.DigitalTwins.Core;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Polly;
using Polly.RateLimit;
using Polly.Wrap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Willow.Rules.Cache;
using Willow.Rules.Logging;
using Willow.Rules.Model;
using Willow.Rules.Sources;
using WillowRules.Extensions;

namespace Willow.Rules.Services;

/// <summary>
/// Methods for getting twins
/// </summary>
public interface ITwinService
{
	/// <summary>
	/// Get a single digital twin by id with forward and reverse relationships
	/// </summary>
	Task<(BasicDigitalTwinPoco? twin, IEnumerable<Edge> forward, IEnumerable<Edge> reverse)> GetDigitalTwinWithRelationshipsAsync(string dtid);

	/// <summary>
	/// Get a single digitial twin, raw data
	/// </summary>
	Task<BasicDigitalTwinPoco?> GetCachedTwin(string id);

	/// <summary>
	/// Get a single digitial twin, raw data without using the cache
	/// </summary>
	/// <remarks>
	/// Used by calculated points to ensure latest data is used
	/// </remarks>
	Task<BasicDigitalTwinPoco?> GetUncachedTwin(string id);

	/// <summary>
	/// Count aall digital twins in cache
	/// </summary>
	Task<int> CountCached();

	/// <summary>
	/// Get all digital twins from cache
	/// </summary>
	IAsyncEnumerable<BasicDigitalTwinPoco> GetAllCached();

	/// <summary>
	/// Query all relationships
	/// </summary>
	AsyncPageable<ExtendedRelationship> QueryAllRelationships(DigitalTwinsClient digitalTwinsClient);

	/// <summary>
	/// Get Twins by model
	/// </summary>
	Task<List<BasicDigitalTwinPoco>> GetTwinsByModelWithInheritance(string modelId);

	/// <summary>
	/// Get forward related twins
	/// </summary>
	Task<List<Edge>> GetCachedForwardRelatedTwins(string id);

	/// <summary>
	/// Get backward related twins
	/// </summary>
	Task<List<Edge>> GetCachedBackwardRelatedTwins(string id);

	/// <summary>
	/// Get the top-level entities from the Twin Graph
	/// Buildings, Systems, ... whatever it finds first
	/// </summary>
	Task<List<BasicDigitalTwinPoco>> GetTopLevelEntities();

	/// <summary>
	/// Converts the contents of the twin into a useable form
	/// </summary>
	Dictionary<string, object> ConvertFromSystemTextJsonToRealObject(Dictionary<string, object> contents);

	/// <summary>
	/// Try to get a clean dotnet object from a System.Text.Json element
	/// </summary>
	bool TryGetObjectFromJElement(JsonElement r, out object? obj);

	/// <summary>
	/// Validates twin properties and logs warnings for issues
	/// </summary>
	void ValidateTwin(BasicDigitalTwinPoco twin);

	/// <summary>
	/// Checks whether the relationship is allowed
	/// </summary>
	Task<bool> IsRelationshipAllowed(BasicRelationship rel);

	/// <summary>
	/// Adhoc query for twins
	/// </summary>
	Task<List<BasicDigitalTwinPoco>> Query(string query, string? twinOutputField = null);
}

/// <summary>
/// Loads and caches twins from ADT
/// </summary>
public class TwinService : ITwinService
{
	private readonly WillowEnvironment willowEnvironment;
	private readonly ADTInstance[] adtInstances;
	private readonly IRetryPolicies retryPolicies;
	private readonly IDataCacheFactory dataCacheFactory;
	private readonly ILogger<TwinService> logger;
	private readonly ILogger throttledLogger;
	private readonly ILogger rateLimitLogger;
	private readonly PolicyWrap adtRateLimitedPolicy;
	private static int currentADTCalls = 0;

	/// <summary>
	/// Creates a new <see cref="TwinService"/> for loading twin data from ADT
	/// </summary>
	public TwinService(
		IADTService adtService,
		WillowEnvironment willowEnvironment,
		IRetryPolicies retryPolicies,
		IDataCacheFactory diskCacheFactory,
		ILogger<TwinService> logger)
	{
		this.willowEnvironment = willowEnvironment ?? throw new ArgumentNullException(nameof(willowEnvironment));
		this.retryPolicies = retryPolicies ?? throw new ArgumentNullException(nameof(retryPolicies));
		this.dataCacheFactory = diskCacheFactory ?? throw new ArgumentNullException(nameof(diskCacheFactory));
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		this.throttledLogger = logger.Throttle(TimeSpan.FromSeconds(15));
		this.rateLimitLogger = logger.Throttle(TimeSpan.FromSeconds(15));

		this.adtInstances = adtService.AdtInstances;

		//testing in brk, this combo gives about 10-15 at a time
		var rateLimit = Policy.RateLimit(50, TimeSpan.FromSeconds(1), 15);
		adtRateLimitedPolicy = Policy.Handle<RateLimitRejectedException>()
			.WaitAndRetry(100, (retryAttempt) => TimeSpan.FromSeconds(15),//large, but aggressive rate limit retry
			(Exception e, TimeSpan t, int c, Context con) =>
			{
				rateLimitLogger.LogWarning("Retrying ADT query. Retry count {c}", c);
			})
			.Wrap(rateLimit);
	}

	public async Task<(BasicDigitalTwinPoco? twin, IEnumerable<Edge> forward, IEnumerable<Edge> reverse)> GetDigitalTwinWithRelationshipsAsync(string dtid)
	{
		var twin = await this.GetCachedTwin(dtid);
		if (twin is null)
		{
			logger.LogWarning("Did not find {dtid}", dtid);
			return (null!, Enumerable.Empty<Edge>(), Enumerable.Empty<Edge>());
		}

		var forwardRelationships = await GetCachedForwardRelatedTwins(dtid);
		var reverseRelationships = await GetCachedBackwardRelatedTwins(dtid);

		return (twin, forwardRelationships, reverseRelationships);
	}

	public async IAsyncEnumerable<BasicDigitalTwinPoco> GetAllCached()
	{
		await foreach (var dtpoco in dataCacheFactory.TwinCache.GetAll(willowEnvironment.Id))
		{
			yield return dtpoco;
		}
	}

	public async Task<int> CountCached()
	{
		return await dataCacheFactory.TwinCache.Count(willowEnvironment.Id);
	}

	public AsyncPageable<ExtendedRelationship> QueryAllRelationships(DigitalTwinsClient digitalTwinsClient)
	{
		var all = digitalTwinsClient.QueryAsync<ExtendedRelationship>("select * from relationships");
		return all;
	}

	public async Task<BasicDigitalTwinPoco?> GetCachedTwin(string id)
	{
		if (id.Equals("all")) return null!;
		if (id.Equals("OCCUPANCYSENSOR")) return null!;
		if (id.Equals("-SP-DEVIATION")) return null!;
		if (id.Equals("zone_temp_sp_deviation")) return null!;

		var twin = await dataCacheFactory.TwinCache.GetOrCreateAsync(willowEnvironment.Id, id, async () =>
		{
			return await GetUncachedTwinInternal(id);
		});

		return twin;
	}

	public async Task<BasicDigitalTwinPoco?> GetUncachedTwin(string id)
	{
		var twin = await GetUncachedTwinInternal(id);
		if (twin is null) return null!;
		return await dataCacheFactory.TwinCache.AddOrUpdate(willowEnvironment.Id, id, twin);
	}

	private async Task<BasicDigitalTwinPoco?> GetUncachedTwinInternal(string id)
	{
		foreach (var adtInstance in adtInstances)
		{
			try
			{
				var twin = await adtInstance.ADTClient.GetDigitalTwinAsync<BasicDigitalTwinPoco>(id);

				if (twin.Value is BasicDigitalTwinPoco tw)
				{
					tw.Contents = ConvertFromSystemTextJsonToRealObject(tw.Contents);
					return tw;
				}
			}
			catch (Azure.RequestFailedException ex)
			{
				if (ex.Status == 404)
				{
					logger.LogWarning("GetUncachedTwin did not find twin id {id}", id);
				}
				else
				{
					logger.LogError(ex, "GetUncachedTwin failed for id {id}", id);
				}

			}
		}

		return null;
	}

	public async Task<List<Edge>> GetCachedForwardRelatedTwins(string id)
	{
		var edges = await this.dataCacheFactory.ForwardEdgeCache.GetOrCreateAsync(willowEnvironment.Id, id, async () =>
		{
			var result = await GetForwardRelatedTwins(id);
			return new CollectionWrapper<Edge>(result);
		});

		return edges!.Items;
	}

	public async Task<List<Edge>> GetCachedBackwardRelatedTwins(string id)
	{
		var edges = await this.dataCacheFactory.BackEdgeCache.GetOrCreateAsync(willowEnvironment.Id, id, async () =>
		{
			var result = await GetBackwardRelatedTwins(id);
			return new CollectionWrapper<Edge>(result);
		});

		return edges!.Items;
	}

	/// <summary>
	/// Escape single quote characters in query expressions
	/// </summary>
	/// <remarks>
	/// This appears to be the only sanitisation we need to do for ADT queries
	/// </remarks>
	private static string safeId(string id)
	{
		return id.Replace("'", "\\'");
	}

	private async Task<List<Edge>> GetForwardRelatedTwins(string id)
	{
		var edges = new List<Edge>();

		foreach (var adtInstance in adtInstances)
		{
			var adtClient = adtInstance.ADTClient;

			// Forward query
			string queryForward = $"SELECT twin,rel from digitaltwins MATCH (equipment_twin)-[rel]->(twin) WHERE equipment_twin.$dtId='{safeId(id)}'";

			var resultForward = await retryPolicies.ADTRetryPolicy.ExecuteAsync(async () =>
			{
				var pageable = adtClient.QueryAsync<Dictionary<string, System.Text.Json.JsonDocument>>(queryForward);
				List<Dictionary<string, System.Text.Json.JsonDocument>> results = new();
				await foreach (var item in pageable) { results.Add(item); }
				return results;
			});

			foreach (var twinrel in resultForward)
			{
				var twin = twinrel["twin"];
				var rel = twinrel["rel"];

				var twinDto = System.Text.Json.JsonSerializer.Deserialize<BasicDigitalTwinPoco>(twin)!;
				if (twinDto is BasicDigitalTwinPoco tw)
				{
					tw.Contents = ConvertFromSystemTextJsonToRealObject(tw.Contents);
				}

				var relExtended = System.Text.Json.JsonSerializer.Deserialize<ExtendedRelationship>(rel)!;

				// // TODO: Instead of getting all and removing ones we don't want, use query to filter
				if (relExtended.Name == "hasDocument") continue;        // ignore non-physical relationship
																		// KEEP FWD if (relExtended.Name == "isCapabilityOf") continue;     // ignore non-physical relationship
				if (relExtended.Name == "installedBy") continue;        // ignore non-physical relationship
				if (relExtended.Name == "manufacturedBy") continue;        // ignore non-physical relationship

				if (string.IsNullOrEmpty(twinDto.name))
				{
					twinDto.name = twinDto.Id;
					logger.LogWarning("Getforward: {twinId} has no name", twinDto.Id);
				}

				edges.Add(new Edge
				{
					Destination = twinDto,
					RelationshipType = relExtended.Name,
					Substance = relExtended.substance
				});
			}
		}

		return edges;
	}

	// TODO Make this AsyncEnumerable
	private async Task<List<Edge>> GetBackwardRelatedTwins(string id)
	{
		var edges = new List<Edge>();

		foreach (var adtInstance in adtInstances)
		{
			var adtClient = adtInstance.ADTClient;

			// Backward query
			string backQuery = $"SELECT twin,rel from digitaltwins MATCH (equipment_twin)<-[rel]-(twin) WHERE equipment_twin.$dtId='{safeId(id)}'";

			var resultBack = await retryPolicies.ADTRetryPolicy.ExecuteAsync(async () =>
			{
				var pageable = adtClient.QueryAsync<Dictionary<string, System.Text.Json.JsonDocument>>(backQuery);
				List<Dictionary<string, System.Text.Json.JsonDocument>> results = new();
				await foreach (var item in pageable) { results.Add(item); }
				return results;
			});


			foreach (var twinrel in resultBack)
			{
				var twin = twinrel["twin"];
				var rel = twinrel["rel"];

				var twinDto = System.Text.Json.JsonSerializer.Deserialize<BasicDigitalTwinPoco>(twin)!;
				if (twinDto is BasicDigitalTwinPoco tw)
				{
					tw.Contents = ConvertFromSystemTextJsonToRealObject(tw.Contents);
				}

				var relExtended = System.Text.Json.JsonSerializer.Deserialize<ExtendedRelationship>(rel)!;

				if (!(await IsRelationshipAllowed(relExtended)))
				{
					continue;
				}

				if (string.IsNullOrEmpty(twinDto.name))
				{
					twinDto.name = twinDto.Id;
					logger.LogWarning("Get backward: Twin {twinId} has no name", twinDto.Id);
				}

				edges.Add(new Edge
				{
					Destination = twinDto,
					RelationshipType = relExtended.Name,
					Substance = relExtended.substance
				});
			}
		}
		return edges;
	}

	/// <summary>
	/// Adhoc query for twins
	/// </summary>
	public async Task<List<BasicDigitalTwinPoco>> Query(string query, string? twinOutputField = null)
	{
		foreach (var adtInstance in adtInstances)
		{
			var client = adtInstance.ADTClient;

			(bool ok, var wrappedResult) = await dataCacheFactory.AdtQueryResult.TryGetValue(willowEnvironment.Id, query);

			if(ok)
			{
				return wrappedResult!.Items;
			}

			var twins = OnceOnly<List<BasicDigitalTwinPoco>>.Execute(query,() =>
			{
				return adtRateLimitedPolicy.Execute(() =>
				{
					Interlocked.Increment(ref currentADTCalls);

					throttledLogger.LogInformation("Concurrent ADT Queries {count}. Incoming query {query}", currentADTCalls, query);

					var data = new List<BasicDigitalTwinPoco>();

					using (logger.TimeOperationOver(TimeSpan.FromSeconds(10), "Running adhoc ADT query {query}", query))

						try
						{
							if (!string.IsNullOrEmpty(twinOutputField))
							{
								var twins = client.Query<JsonElement>(query);

								data.AddRange(twins.Select(v => v.GetProperty(twinOutputField).Deserialize<BasicDigitalTwinPoco>()!));
							}
							else
							{

								data.AddRange(client.Query<BasicDigitalTwinPoco>(query));
							}

							foreach (var twin in data)
							{
								twin.Contents = ConvertFromSystemTextJsonToRealObject(twin.Contents);
							}
						}
						finally
						{
							Interlocked.Decrement(ref currentADTCalls);
						}

					return data;
				});
			});

			await dataCacheFactory.AdtQueryResult.AddOrUpdate(willowEnvironment.Id, query, new CollectionWrapper<BasicDigitalTwinPoco>(twins));

			return twins;
		}

		return new List<BasicDigitalTwinPoco>(0);
	}

	/// <summary>
	/// Get Twins by model
	/// </summary>
	public async Task<List<BasicDigitalTwinPoco>> GetTwinsByModelWithInheritance(string modelId)
	{
		var result = await dataCacheFactory.DiskCacheTwinsByModelWithInheritance.GetOrCreateAsync(willowEnvironment.Id, modelId, async () =>
		{
			using (var timed = logger.TimeOperation("Calling ADT to get twins my model for '{modelId}'", modelId))
			{
				List<BasicDigitalTwinPoco> result = new();
				foreach (var adtInstance in adtInstances)
				{
					var client = adtInstance.ADTClient;
					var twins = client.QueryAsync<BasicDigitalTwinPoco>($"SELECT * FROM DIGITALTWINS DT WHERE IS_OF_MODEL(DT, '{safeId(modelId)}')");
					await foreach (var twin in twins)
					{
						if (twin is BasicDigitalTwinPoco tw)
						{
							tw.Contents = ConvertFromSystemTextJsonToRealObject(tw.Contents);

							(_, var existingTwin) = await dataCacheFactory.TwinCache.TryGetValue(willowEnvironment.Id, tw.Id);

							tw.Locations = existingTwin?.Locations ?? [];
						}

						result.Add(twin);
						//logger.LogInformation(JsonConvert.SerializeObject(twin));
					}
				}
				logger.LogDebug("ADT returned {count} twins for '{modelId}'", result.Count, modelId);
				return new CollectionWrapper<BasicDigitalTwinPoco>(result);
			}
		});

		if (result is null)
		{
			logger.LogWarning("Null result from get twins my model for `{modelId}`", modelId);
			return new();
		}

		logger.LogInformation("GetTwinsByModelWithInheritance: Got {count} twins for model {model}", result.Items.Count, modelId);

		return result.Items;
	}

	private static string[] topLevelModels = new string[] {
		"dtmi:com:willowinc:Portfolio;1",
		"dtmi:com:willowinc:Building;1",
		"dtmi:com:willowinc:Land;1", "dtmi:com:willowinc:Floor;1",
		"dtmi:com:willowinc:mining:System;1",
		"dtmi:com:willowinc:Equipment;1"
	};

	/// <inheritdoc />
	public async Task<List<BasicDigitalTwinPoco>> GetTopLevelEntities()
	{
		var result = await dataCacheFactory.DiskCacheTwinsByModelWithInheritance.GetOrCreateAsync(willowEnvironment.Id, "toplevel", async () =>
		{
			List<BasicDigitalTwinPoco> result = new();
			foreach (var adtInstance in adtInstances)
			{
				foreach (var modelId in topLevelModels)
				{
					var client = adtInstance.ADTClient;
					var twins = client.QueryAsync<BasicDigitalTwinPoco>($"SELECT * FROM DIGITALTWINS DT WHERE IS_OF_MODEL(DT, '{safeId(modelId)}')");
					await foreach (var twin in twins)
					{
						if (twin is BasicDigitalTwinPoco tw)
						{
							tw.Contents = ConvertFromSystemTextJsonToRealObject(tw.Contents);
						}
						result.Add(twin);
					}
					// Keep going one below Portfolio as some instances don't have links yet between Portfolio and Buildings
					if (!modelId.Contains("Portfolio") && result.Any()) break;
				}
			}

			// Log the timezone for each building
			foreach (var twin in result)
			{
				logger.LogInformation("Twin {twin}, Timezone {tz}", twin.Id, twin.TimeZone?.Name);
			}

			return new CollectionWrapper<BasicDigitalTwinPoco>(result);
		});

		return result!.Items;
	}

	/// <summary>
	/// Fixes the System.Text.Json object structure back to something that works
	/// </summary>
	public Dictionary<string, object> ConvertFromSystemTextJsonToRealObject(Dictionary<string, object>? contents)
	{
		contents ??= new Dictionary<string, object>();
		if (contents.Count == 0) return contents;
		Dictionary<string, object> replacementDictionary = new();
		foreach (var pair in contents)
		{
			// We don't need the metadata like lastUpdateTime
			if (pair.Key.Equals("$metadata")) continue;

			// We already have the haystack tags in the tags field
			if (pair.Key.Equals("TagString")) continue;

			// Because we used NewtonSoft to parse the BasicDigitalTwin the contents field contains these
			if (pair.Value is JToken jtoken)
			{
				var convertedNewtonsoft = ConvertJTokenToDictionary(jtoken);
				if (convertedNewtonsoft is not null)
				{
					replacementDictionary.Add(pair.Key, convertedNewtonsoft);
				}
				continue;
			}

			// Still some old-style stuff in there?
			if (pair.Value is System.Text.Json.JsonElement r)
			{
				switch (r.ValueKind)
				{
					case JsonValueKind.Object:
						{
							var convertedSystem = ConvertJsonToDictionary(r);
							if (convertedSystem is not null)
							{
								replacementDictionary.Add(pair.Key, convertedSystem);
							}
							break;
						}
					case JsonValueKind.Array:
						{
							var convertedSystem = ConvertJsonArrayToList(r.EnumerateArray());
							replacementDictionary.Add(pair.Key, convertedSystem);
							break;
						}
					default:
						{
							var convertedSystem = GetJsonElementValue(r);
							if (convertedSystem is not null)
							{
								replacementDictionary.Add(pair.Key, convertedSystem);
							}
							break;
						}
				}
				continue;
			}

			// Else, already converted just add it
			replacementDictionary.Add(pair.Key, pair.Value);
		}

		return replacementDictionary;
	}


	public static object? ConvertJTokenToDictionary(Newtonsoft.Json.Linq.JToken token)
	{
		if (token is null)
			return new Dictionary<string, object>();

		if (token.Type == Newtonsoft.Json.Linq.JTokenType.Object)
		{
			var dict = new Dictionary<string, object>();
			foreach (var prop in token.Children<Newtonsoft.Json.Linq.JProperty>())
			{
				// We don't need the metadata like lastUpdateTime
				if (prop.Name.Equals("$metadata")) continue;

				var res = ConvertJTokenToDictionary(prop.Value);
				if (res is not null)
				{
					dict[prop.Name] = res;
				}
			}
			return dict.Any() ? dict : null;
		}

		if (token.Type == Newtonsoft.Json.Linq.JTokenType.Array)
		{
			var list = new List<object>();
			foreach (var item in token.Children())
			{
				list.Add(ConvertJTokenToDictionary(item)!);
			}
			return list;
		}

		return ((Newtonsoft.Json.Linq.JValue)token).Value!;
	}
	public static Dictionary<string, object>? ConvertJsonToDictionary(JsonElement element)
	{
		Dictionary<string, object> result = new Dictionary<string, object>();

		foreach (var property in element.EnumerateObject())
		{
			// We don't need the metadata like lastUpdateTime
			if (property.Name.Equals("$metadata")) continue;

			switch (property.Value.ValueKind)
			{
				case JsonValueKind.Object:
					var res = ConvertJsonToDictionary(property.Value);
					if (res is not null)
					{
						result[property.Name] = res;
					}
					break;
				case JsonValueKind.Array:
					result[property.Name] = ConvertJsonArrayToList(property.Value.EnumerateArray());
					break;
				default:
					result[property.Name] = GetJsonElementValue(property.Value);
					break;
			}
		}

		if (!result.Any()) return null; else return result;
	}

	public static List<object> ConvertJsonArrayToList(JsonElement.ArrayEnumerator enumerator)
	{
		List<object> result = new List<object>();

		foreach (var element in enumerator)
		{
			switch (element.ValueKind)
			{
				case JsonValueKind.Object:
					result.Add(ConvertJsonToDictionary(element)!);
					break;
				case JsonValueKind.Array:
					result.Add(ConvertJsonArrayToList(element.EnumerateArray()));
					break;
				default:
					result.Add(GetJsonElementValue(element));
					break;
			}
		}

		return result;
	}

	public static object GetJsonElementValue(JsonElement element)
	{
		switch (element.ValueKind)
		{
			case JsonValueKind.Number:
				return element.GetDouble();
			case JsonValueKind.String:
				return element.GetString()!;
			case JsonValueKind.True:
				return true;
			case JsonValueKind.False:
				return false;
			case JsonValueKind.Null:
				return null!;
			default:
				throw new NotSupportedException($"Unsupported JSON value kind: {element.ValueKind}");
		}
	}




	/// <summary>
	/// Try to get a clean dotnet object from a System.Text.Json element
	/// </summary>
	public bool TryGetObjectFromJElement(JsonElement r, out object? obj)
	{
		switch (r.ValueKind)
		{
			case JsonValueKind.String:
				{
					var value = r.GetString();
					if (value is string)
					{
						obj = value;
						return true;
					}
					break;
				}
			case JsonValueKind.Number:
				{
					var value = r.GetDouble();
					obj = value;
					return true;
				}
			case JsonValueKind.True:
				{
					obj = true;
					return true;
				}
			case JsonValueKind.False:
				{
					obj = false;
					return true;
				}
			case JsonValueKind.Null:
				{
					// leave it out of dictionary
					obj = null!;
					return false;
				}
			case JsonValueKind.Object:
				{
					var dict = r.Deserialize<Dictionary<string, object>>();
					if (dict is not null)
					{
						var inner = ConvertFromSystemTextJsonToRealObject(dict);

						// Don't add empty objects, trying to keep this small in memory
						// Check this using http://localhost:5050/equipment/MS-PS-B122-VSVAV.L3.01
						if (inner is not null && inner.Any())
						{
							obj = dict;
							return true;
						}
					}
					obj = null!;
					return false;
				}
			case JsonValueKind.Array:
			default:
				{
					obj = null!;
					return false;
				}
		}
		obj = null!;
		return false;
	}

	public void ValidateTwin(BasicDigitalTwinPoco twin)
	{
		if (twin.Contents is not null)
		{
			ValidateContent(twin, twin.Contents);
		}
	}

	private void ValidateContent(BasicDigitalTwinPoco twin, Dictionary<string, object?> content)
	{
		foreach ((var key, var value) in content)
		{
			if (value is Dictionary<string, object?> c)
			{
				ValidateContent(twin, c);
			}
			else if (value is null)
			{
				throttledLogger.LogWarning("Null field {key} for twin {id}", key, twin.Id);
			}
			else if (!(value is IConvertible))
			{
				throttledLogger.LogWarning("Invalid field {key} of type {type} for twin {id}", key, value.GetType().FullName, twin.Id);
			}
		}
	}

	public async Task<bool> IsRelationshipAllowed(BasicRelationship rel)
	{
		// There are just too many of these to handle and in any case
		// rules engine doesn't care about them, so why read them at all?
		if (rel.Name == "hasDocument") // ignore non-physical relationship
		{
			return false;
		}

		//alot but only allow for certain scenarios
		if (rel.Name == "hostedBy")
		{
			(var targetok, var target) = await dataCacheFactory.TwinCache.TryGetValue(willowEnvironment.Id, rel.TargetId);

			//only allowed for Electrical meter
			if (targetok && target!.ModelId() != "dtmi:com:willowinc:ElectricalMeter;1")
			{
				return false;
			}
		}

		return true;
	}
}
