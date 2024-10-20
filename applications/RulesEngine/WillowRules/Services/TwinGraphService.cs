using Abodit.Mutable;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Willow.Rules.Cache;
using Willow.Rules.Logging;
using Willow.Rules.Model;
using Willow.Rules.Sources;

namespace Willow.Rules.Services;


/// <summary>
/// Service for loading a graph of all twins
/// </summary>
/// <remarks>
/// This takes hours (!) until the cache is loaded
/// </remarks>
public interface ITwinGraphService
{
	/// <summary>
	/// Get the [very large] graph of all twins ids (cached)
	/// </summary>
	Task<Graph<MiniTwinDto, WillowRelation>> GetGraphCachedAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Get the [very large] graph of all twin ids (uncached) and writes a new one to disk
	/// </summary>
	Task<Graph<MiniTwinDto, WillowRelation>> GetGraphUncachedAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Service for loading a graph of all twins
/// </summary>
public partial class TwinGraphService : ITwinGraphService
{
	private readonly IDataCacheFactory diskCacheFactory;
	private readonly WillowEnvironment willowEnvironment;
	private readonly ILogger<TwinGraphService> logger;

	/// <summary>
	/// Creates a new <see cref="TwinGraphService"/> for loading twin data from ADT
	/// </summary>
	public TwinGraphService(
		IDataCacheFactory diskCacheFactory,
		WillowEnvironment willowEnvironment,
		ILogger<TwinGraphService> logger)
	{
		this.diskCacheFactory = diskCacheFactory ?? throw new ArgumentNullException(nameof(diskCacheFactory));
		this.willowEnvironment = willowEnvironment;
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	/// <summary>
	/// Gets graph of twins
	/// </summary>
	public async Task<Graph<MiniTwinDto, WillowRelation>> GetGraphCachedAsync(
		CancellationToken cancellationToken = default)
	{
		using var timedLogger = logger.TimeOperation("GetGraphCachedAsync");

		//caching this object creates too big a serialized object in db and memory and reading from it is just as slow.
		var twinGraph = await GetGraphUncachedAsync(cancellationToken);

		logger.LogInformation("Got full graph {nodes} nodes, {edges} edges", twinGraph.Nodes.Count(), twinGraph.Edges.Count());

		return twinGraph;
	}

	/// <summary>
	/// Gets graph of twins uncached and writes a new one to disk
	/// </summary>
	/// <returns></returns>
	public async Task<Graph<MiniTwinDto, WillowRelation>> GetGraphUncachedAsync(
		CancellationToken cancellationToken = default)
	{
		using var timedLogger = logger.TimeOperation("GetGraphUncachedAsync");
		using var scopedLogger = logger.BeginScope("GetGraphUncachedAsync");

		var throttledLogger = logger.Throttle(TimeSpan.FromSeconds(15));

		var twinCache = this.diskCacheFactory.TwinCache;
		var relationshipCache = this.diskCacheFactory.ExtendedRelationships;

		var twins = twinCache.GetAll(willowEnvironment.Id);
		var relationships = relationshipCache.GetAll(willowEnvironment.Id);

		Graph<MiniTwinDto, WillowRelation> idGraph = new();

		if (!await twinCache.Any(willowEnvironment.Id))
		{
			logger.LogWarning("Why has processor not filled the cache? Disconnected environment?");
			return idGraph;
		}

		Dictionary<string, MiniTwinDto> dict = new();

		int c = 0;
		int r = 0;

		await foreach (BasicDigitalTwinPoco twin in twins)
		{
			c++;
			dict.Add(twin.Id, new MiniTwinDto() { Id = twin.Id });
			if ((c % 1000) == 0) throttledLogger.LogInformation("{twins} twins loaded", c);
		};

		logger.LogInformation("Counted {twins} twins, {dict} in dictionary", c, dict.Count);

		logger.LogInformation("Building in memory graph of all twins");

		await foreach (ExtendedRelationship rel in relationships)
		{
			r++;
			var relation = WillowRelation.Get(rel.Name, rel.substance);
			bool sourceOk = dict.TryGetValue(rel.SourceId, out var source);
			bool targetOk = dict.TryGetValue(rel.TargetId, out var target);

			if (sourceOk && targetOk)
			{
				idGraph.AddStatement(source!, relation, target!);
			}

			if ((r % 1000) == 0) throttledLogger.LogInformation("{rel} relationships added", r);

			// // And the inverse
			// string inverseName = InverseRelationships.GetInverse(rel.Name);
			// var inverseRelation = WillowRelation.Get(inverseName, rel.substance);
			// idGraph.AddStatement(rel.TargetId, inverseRelation, rel.SourceId);
		}

		// TODO: Are there other inferred relationships we want to add to the graph
		// before returning it, like feeding through an HVAC Zone to a room?

		// Note that a twin is not added to the graph if it has no relationships

		logger.LogInformation("Counted {c:N0} twins", c);
		logger.LogInformation("Counted {r:N0} relationships", r);
		logger.LogInformation("{dict.Count:N0} twins in dictionary", dict.Count);

		int missingRelationships = 0;
		var idGraphIds = idGraph.Nodes.Select(x => x.Id);

		foreach (var node in dict.Keys.Except(idGraphIds))
		{
			missingRelationships++;
			throttledLogger.LogWarning("Node {node} has no relationships ({count})", node, missingRelationships);
		}

		if (missingRelationships > 0)
		{
			logger.LogWarning("Found {missingRelationships} nodes with no relationships", missingRelationships);
		}

		return idGraph;
	}
}
