using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Abodit.Mutable;

namespace Willow.Rules.Model;

/// <summary>
/// A graph of nodes and edges containing twins
/// </summary>
public struct TwinGraphDto
{
	public TwinNodeDto[] Nodes { get; set; }
	public TwinRelationshipDto[] Edges { get; set; }

	public static TwinGraphDto From(Graph<BasicDigitalTwinPoco, WillowRelation> graph, IList<string> selectedNodeIds)
	{
		ConcurrentDictionary<string, int> allocatorSet = new();

		int index = 1;

		// maps keys to values
		int allocator(string id) => allocatorSet.AddOrUpdate(id, x => index++, (x, i) => i);

		//var nodes = graph.Nodes.Select(n => dtoFactory(n, selectedNodeIds.Contains(n.Id), allocator(n.Id)));

		List<TwinNodeDto> nodes = new List<TwinNodeDto>();
		// calculate secondary grouping key - by alpha into five groups

		//order nodes first to ensure consistency in case the incoming graph order changed (this can happen with serialization/deserialization from cache)
		var primaryGroups = graph.Nodes.OrderBy(v => v.Id).GroupBy(n => SuggestGroupKey(n, graph));
		foreach (var primaryGroup in primaryGroups)
		{
			int count = primaryGroup.Count();
			if (count < 5)
			{
				foreach (var twin in primaryGroup)
				{
					nodes.Add(dtoFactory(twin, selectedNodeIds.Contains(twin.Id), allocator(twin.Id),
						primaryGroup.Key.gpName, primaryGroup.Key.extra));
				}
			}
			else
			{
				// Large groups need secondary grouping
				// First pass assign each word to a group of even size
				int divisor = (int)Math.Round(Math.Sqrt(count));  // 49 twins = 7 groups of 7
				var alphaGroups = primaryGroup
					.OrderBy(x => (x.name?.ToUpperInvariant() ?? ""))
					.Select((x, i) => (twin: x, key: i * divisor / count));

				// (TODO) Optionally shift 1/10th of any group up or down into next group
				// to maximize letter separation between groups - maybe not worth doing
				// names will be really similar
				foreach (var alphaGroup in alphaGroups.GroupBy(g => g.key))
				{
					var groupName = alphaGroup.First().twin.name + "-" + alphaGroup.Last().twin.name;
					foreach (var ag in alphaGroup.ToList())
					{
						var twin = ag.twin;
						nodes.Add(dtoFactory(twin, selectedNodeIds.Contains(twin.Id), allocator(twin.Id),
							primaryGroup.Key.gpName, groupName));
					}
				}
			}
		}

		var relationships = graph.Edges.OrderBy(v => v.Start.Id).Select(e => CreateRelationshipDto(e.Start.Id, e.End.Id,
			e.Predicate.Name, e.Predicate.Substance, allocator));

		return new TwinGraphDto
		{
			Nodes = nodes.ToArray(),
			Edges = relationships.ToArray()
		};
	}

	private static TwinNodeDto dtoFactory(BasicDigitalTwinPoco relatedTwin, bool isSelected, int localId,
	string groupKey, string groupKey2) =>
	new TwinNodeDto
	{
		IsCollapsed = false,
		IsExpanded = false,
		IsSelected = isSelected,
		Id = localId,
		TwinId = relatedTwin.Id,
		Name = string.IsNullOrWhiteSpace(relatedTwin.name) ? relatedTwin.Id : relatedTwin.name,
		Description = relatedTwin.description ?? "",
		ModelId = relatedTwin.Metadata.ModelId,
		TrendInterval = relatedTwin.trendInterval,
		Position = relatedTwin.position,
		ValueExpression = relatedTwin.ValueExpression,
		Unit = relatedTwin.unit,
		GroupKey = groupKey, // SuggestGroupKey(relatedTwin)
		GroupKey2 = groupKey2,
		Contents = relatedTwin.Contents
		// GroupKey2 is set later
	};

	/// <summary>
	/// Gets a suggested group key for collapsing nodes in the UI
	/// </summary>
	private static (string gpName, string extra) SuggestGroupKey(BasicDigitalTwinPoco twin,
		Graph<BasicDigitalTwinPoco, WillowRelation> graph)
	{
		// Group must match all the ins and outs, e.g. one entity
		// with multiple capabilities
		string inouts(BasicDigitalTwinPoco n) =>
			string.Join("_",
				graph.Follow(n).Select(e => e.End)
				.Concat(graph.Back(n).Select(e => e.Start))
				.Select(x => x.name)).GetHashCode().ToString();

		if (string.IsNullOrEmpty(twin.name)) return ("", "");

		if (twin.name.Contains("Zone Air", StringComparison.OrdinalIgnoreCase) || twin.Metadata.ModelId.Contains("ZoneAir"))
		{
			return ("zone air related", "");
		}

		if (twin.name.Contains("Supply", StringComparison.OrdinalIgnoreCase) || twin.Metadata.ModelId.Contains("Supply"))
		{
			return ("supply related", "");
		}

		if (twin.name.Contains("Outside", StringComparison.OrdinalIgnoreCase) || twin.Metadata.ModelId.Contains("Outside"))
		{
			return ("outside related", "");
		}

		if (twin.name.Contains("Cooling", StringComparison.OrdinalIgnoreCase) || twin.Metadata.ModelId.Contains("Cooling"))
		{
			return ("cooling related", "");
		}

		if (twin.name.Contains("Return", StringComparison.OrdinalIgnoreCase) || twin.Metadata.ModelId.Contains("Return"))
		{
			return ("return related", "");
		}

		if (twin.name.Contains("Exhaust", StringComparison.OrdinalIgnoreCase) || twin.Metadata.ModelId.Contains("Exhaust"))
		{
			return ("exhaust related", "");
		}

		if (twin.name.Contains("Discharge", StringComparison.OrdinalIgnoreCase) || twin.Metadata.ModelId.Contains("Discharge"))
		{
			// TODO: Ask Rick if supply and discharge can be combined
			return ("discharge related", "");
		}

		if (twin.name.Contains("Case", StringComparison.OrdinalIgnoreCase) || twin.Metadata.ModelId.Contains("Case"))
		{
			return ("cases", "");
		}

		if (twin.name.Contains("Temperature", StringComparison.OrdinalIgnoreCase) || twin.Metadata.ModelId.Contains("Temperature"))
		{
			return ("temperature related", "");
		}

		if (twin.name.Contains("Pressure", StringComparison.OrdinalIgnoreCase) || twin.Metadata.ModelId.Contains("Pressure"))
		{
			return ("pressure related", "");
		}

		if (twin.unit?.Equals("Terminal Units") ?? false)  // HBC and CBC counts
		{
			return ("counters", "");
		}

		// TODO: Use model inheritance not ID names

		if (twin.Metadata.ModelId.Contains("Actuator"))
		{
			return ("actuators", twin.Metadata.ModelId);
		}

		if (twin.Metadata.ModelId.Contains("Setpoint"))
		{
			return ("setpoints", twin.Metadata.ModelId);
		}

		if (twin.Metadata.ModelId.Contains("EnergySensor"))
		{
			return ("energy", twin.Metadata.ModelId);
		}

		if (twin.Metadata.ModelId.Contains("CurrentSensor") || twin.Metadata.ModelId.Contains("CurrentMagnitudeSensor"))
		{
			return ("current", twin.Metadata.ModelId);
		}

		if (twin.Metadata.ModelId.Contains("PowerSensor"))
		{
			return ("power", twin.Metadata.ModelId);
		}

		if (twin.Metadata.ModelId.Contains("VoltageSensor") || twin.Metadata.ModelId.Contains("VoltageMagnitude"))
		{
			return ("voltage", twin.Metadata.ModelId);
		}

		// otherwise potentially group by the model id
		return (twin.Metadata.ModelId, inouts(twin));
	}

	private static TwinRelationshipDto CreateRelationshipDto(
		string sourceId, string targetId, string name, string substance, Func<string, int> allocator)
	{
		return new TwinRelationshipDto
		{
			StartId = allocator(sourceId),
			EndId = allocator(targetId),
			StartTwinId = sourceId,
			EndTwinId = targetId,
			Name = name,
			Substance = substance
		};
	}
}
