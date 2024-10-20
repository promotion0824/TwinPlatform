using Abodit.Mutable;
using System;
using System.Collections.Generic;
using System.Linq;
using Abodit.Graph;

namespace WillowRules.DTO;

/// <summary>
/// A serializable graph for persisting to the cache
/// </summary>
public struct SerializableGraph<T, E>
	where E : notnull, IEquatable<E>, IRelation
	where T : IEquatable<T>
{
	/// <summary>
	/// Get or set the serializable nodes
	/// </summary>
	public IList<T> Nodes { get; set; }

	/// <summary>
	/// Get or set the serializable edges
	/// </summary>
	public IList<SerializableEdge<E>> Edges { get; set; }

	/// <summary>
	/// Gets the fully formed graph
	/// </summary>
	public Graph<T, E> GetGraph(Func<T, string> idGetter)
	{
		Graph<T, E> graph = new();
		var nodeDict = Nodes.ToDictionary(x => idGetter(x), x => x);
		foreach (var edge in Edges)
		{
			if (nodeDict.TryGetValue(edge.StartId, out var start) &&
				nodeDict.TryGetValue(edge.EndId, out var end))
			{
				graph.AddStatement(start, edge.Edge, end);
			}
		}
		return graph;
	}

	/// <summary>
	/// Gets a serialized graph from a graph with projection of the nodes
	/// </summary>
	public static SerializableGraph<T, E> FromGraphWithProjection<TSource>(Graph<TSource, E> graph,
		Func<TSource, string> idGetter,
		Func<TSource, T> nodeGetter) where TSource : IEquatable<TSource>
	{
		var result = new SerializableGraph<T, E>
		{
			Nodes = graph.Nodes.Select(nodeGetter).ToList(),
			Edges = graph.Edges.Select(e => new SerializableEdge<E>
			{
				StartId = idGetter(e.Start),
				EndId = idGetter(e.End),
				Edge = e.Predicate
			}).ToList()

		};
		return result;
	}

	/// <summary>
	/// Gets a serialized graph from a graph
	/// </summary>
	public static SerializableGraph<T, E> FromGraph(Graph<T, E> graph, Func<T, string> idGetter)
	{
		var result = new SerializableGraph<T, E>
		{
			Nodes = graph.Nodes.ToList(),
			Edges = graph.Edges.Select(e => new SerializableEdge<E>
			{
				StartId = idGetter(e.Start),
				EndId = idGetter(e.End),
				Edge = e.Predicate
			}).ToList()
		};
		return result;
	}

}
