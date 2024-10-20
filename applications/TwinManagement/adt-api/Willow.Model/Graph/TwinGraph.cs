using Abodit.Mutable;
using System.Collections.Concurrent;
using System.Globalization;
using Willow.Model.Adt;

namespace Willow.Model.Graph;

/// <summary>
/// A graph of nodes and edges containing twins
/// </summary>
public struct TwinGraph
{
    public TwinNode[] Nodes { get; set; }
    public TwinRelationship[] Edges { get; set; }

    public static TwinGraph From(Graph<EquatableBasicDigitalTwin, TwinRelation> graph)
    {
        ConcurrentDictionary<string, int> allocatorSet = new();

        int index = 1;

        // maps keys to values
        int allocator(string id) => allocatorSet.AddOrUpdate(id, x => index++, (x, i) => i);

        var nodes = new List<TwinNode>();
        // calculate secondary grouping key - by alpha into five groups

        var primaryGroups = graph.Nodes.GroupBy(n => SuggestGroupKey(n, graph));
        foreach (var primaryGroup in primaryGroups)
        {
            int count = primaryGroup.Count();
            if (count < 10)
            {
                foreach (var twin in primaryGroup)
                {
                    nodes.Add(dtoFactory(twin, allocator(twin.Id),
                        primaryGroup.Key.gpName, primaryGroup.Key.extra));
                }
            }
            else
            {
                // Large groups need secondary grouping
                // First pass assign each word to a group of even size
                int divisor = (int)Math.Round(Math.Sqrt(count));  // 49 twins = 7 groups of 7
                var alphaGroups = primaryGroup
                    .OrderBy(x => x.GetProperty("name")?.ToUpperInvariant())
                    .Select((x, i) => (twin: x, key: i * divisor / count));

                // (TODO) Optionally shift 1/10th of any group up or down into next group
                // to maximize letter separation between groups - maybe not worth doing
                // names will be really similar
                foreach (var alphaGroup in alphaGroups.GroupBy(g => g.key))
                {
                    var groupName = alphaGroup.First().twin.GetProperty("name") + "-" + alphaGroup.Last().twin.GetProperty("name");
                    foreach (var ag in alphaGroup.ToList())
                    {
                        var twin = ag.twin;
                        nodes.Add(dtoFactory(twin, allocator(twin.Id),
                            primaryGroup.Key.gpName, groupName));
                    }
                }
            }
        }

        var relationships = graph.Edges.Select(e => CreateRelationshipDto(e.Start.Id, e.End.Id, e.Predicate.Name, allocator));

        return new TwinGraph
        {
            Nodes = nodes.ToArray(),
            Edges = relationships.ToArray()
        };
    }

    private static TwinNode dtoFactory(EquatableBasicDigitalTwin relatedTwin, int localId, string groupKey, string groupKey2)
    {
        var name = relatedTwin.GetProperty("name");
        return new TwinNode
        {
            Id = localId,
            TwinId = relatedTwin.Id,
            Name = !string.IsNullOrEmpty(name) ? name : relatedTwin.Id,
            ModelId = relatedTwin.Metadata.ModelId,
            GroupKey = groupKey,
            GroupKey2 = groupKey2
        };
    }

    /// <summary>
    /// Gets a suggested group key for collapsing nodes in the UI
    /// </summary>
    private static (string gpName, string extra) SuggestGroupKey(EquatableBasicDigitalTwin twin, Graph<EquatableBasicDigitalTwin, TwinRelation> graph)
    {
        // Group must match all the ins and outs, e.g. one entity
        // with multiple capabilities
        string inouts(EquatableBasicDigitalTwin n) =>
            string.Join("_",
                graph.Follow(n).Select(e => e.End)
                .Concat(graph.Back(n).Select(e => e.Start))
                .Select(x => x.GetProperty("name"))).GetHashCode().ToString(CultureInfo.InvariantCulture);

        if (!twin.Contents.TryGetValue("name", out var nameContents))
        {
            return ("", "");
        }

        var name = nameContents?.ToString();

        if (name == null)
        {
            return ("", "");
        }

        if (name.ToString().Contains("Supply", StringComparison.OrdinalIgnoreCase))
        {
            return ("supply", "");
        }

        if (name.ToString().Contains("Outside", StringComparison.OrdinalIgnoreCase))
        {
            return ("outside", "");
        }

        if (name.ToString().Contains("Cooling", StringComparison.OrdinalIgnoreCase))
        {
            return ("cooling", "");
        }

        if (name.ToString().Contains("Return", StringComparison.OrdinalIgnoreCase))
        {
            return ("return", "");
        }

        if (name.ToString().Contains("Exhaust", StringComparison.OrdinalIgnoreCase))
        {
            return ("exhaust", "");
        }

        if (name.ToString().Contains("Discharge", StringComparison.OrdinalIgnoreCase))
        {
            // TODO: Ask Rick if supply and discharge can be combined
            return ("discharge", "");
        }

        // otherwise potentially group by the model id
        return (twin.Metadata.ModelId, inouts(twin));
    }

    private static TwinRelationship CreateRelationshipDto(string sourceId, string targetId, string name, Func<string, int> allocator)
    {
        return new TwinRelationship
        {
            StartId = allocator(sourceId),
            EndId = allocator(targetId),
            StartTwinId = sourceId,
            EndTwinId = targetId,
            Name = name
        };
    }
}
