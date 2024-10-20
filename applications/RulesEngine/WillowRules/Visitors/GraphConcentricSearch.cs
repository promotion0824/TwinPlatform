using System;
using System.Collections.Generic;
using System.Linq;
using Abodit.Graph.Base;

namespace WillowRules.Visitors;

/// <summary>
/// Extension methods for graph traversal
/// </summary
public static class GraphConcentricSearch
{
	/// <summary>
	/// Get all the nodes that can be reached by following relationships of a given type
	/// returns them in sorted order according to how close they are
	/// so shortest path is returned first.  Tuple includes the distance.
	/// </summary>
	/// <remarks>
	/// Handles circular graphs too ...
	/// </remarks>
	public static IEnumerable<(TNode node, int distance)> DistanceToEverywhere<TNode, TRelation>(this GraphBase<TNode, TRelation> graph, TNode subject,
		bool includeStartNode = false)
		where TNode : IEquatable<TNode>
		where TRelation : IEquatable<TRelation>
	{
		HashSet<TNode> visited = new HashSet<TNode>();
		Dictionary<TNode, int> heap = new Dictionary<TNode, int>();

		// Breadth first search, ensures we search in distance order
		heap.Add(subject, 0);

		while (heap.Any())
		{
			var first = heap.OrderBy(h => h.Value).First();     // Next closest, TODO: an ordered heap would help

			var node = first.Key;
			var distance = first.Value;

			heap.Remove(node);
			visited.Add(node);

			// Where can we reach from this node? - add them all to the heap according to distance
			foreach (var item in graph.Follow(node))
			{
				var end = item.End;

				// If not already visited, go visit it ...
				if (!visited.Contains(end))
				{
					if (heap.ContainsKey(end)) heap[end] = Math.Min(heap[end], distance + 1);
					else heap[end] = distance + 1;
				}
			}

			foreach (var item in graph.Back(node))
			{
				var start = item.Start;

				// If not already visited, go visit it ...
				if (!visited.Contains(start))
				{
					if (heap.ContainsKey(start)) heap[start] = Math.Min(heap[start], distance + 1);
					else heap[start] = distance + 1;
				}
			}

			if (includeStartNode || !node.Equals(subject))
				yield return (node, distance);
		}
	}
}
