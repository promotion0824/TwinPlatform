using Abodit.Graph;
using Abodit.Mutable;
using System;
using System.Collections.Generic;
using System.Linq;
using Willow.Rules.Model;

namespace WillowRules.Extensions;

public static class GraphExtensions
{
	/// <summary>
	/// Finds a particular forward relationship for a given set of models
	/// </summary>
	public static IEnumerable<MetaGraphNode> GetForwardRelationshipsForModels(this Graph<MetaGraphNode, MetaGraphRelation> graph,
		IEnumerable<ModelData> nodes,
		string relationship)
	{
		var result = new List<MetaGraphNode>();

		foreach (var childModel in nodes)
		{
			var metaNode = graph.Nodes.FirstOrDefault(v => v.ModelId == childModel.Id);

			if (metaNode is not null)
			{
				foreach (var fedByNode in graph.GetForwardNodesForRelationship(metaNode, relationship))
				{
					result.Add(fedByNode);
				}
			}
		}

		return result.Distinct();
	}

	/// <summary>
	/// Indicates whether one modelid is an ancestor or decendant of another
	/// </summary>
	public static IEnumerable<MetaGraphNode> GetForwardNodesForRelationship(this Graph<MetaGraphNode, MetaGraphRelation> graph, MetaGraphNode startNode, string relationship)
	{
		var nodes = new List<MetaGraphNode>();

		Queue<MetaGraphNode> queue = new();

		queue.Enqueue(startNode);

		while (queue.Count > 0)
		{
			var node = queue.Dequeue();

			var forward = graph.Follow(node);

			foreach (var edge in forward.Where(v => v.Predicate.Relation == relationship))
			{
				if (!nodes.Contains(edge.End))
				{
					nodes.Add(edge.End);
					queue.Enqueue(edge.End);
				}
			}
		}

		return nodes;
	}

	/// <summary>
	/// Indicates whether one modelid succeeds another
	/// </summary>
	public static bool IsAncestorOrDescendantOrEqual(this Graph<ModelData, Relation> ontology, string nodeId, string endId)
	{
		return ontology.IsAncestorOrEqualTo(nodeId, endId) || ontology.IsDescendantOrEqualTo(nodeId, endId);
	}

	/// <summary>
	/// Indicates whether one modelid is an ancestor of another
	/// </summary>
	public static bool IsAncestorOrEqualTo(this Graph<ModelData, Relation> ontology, string nodeId, string ancestorId)
	{
		if (nodeId == ancestorId)
		{
			return true;
		}

		var startNode = ontology.Nodes.FirstOrDefault(v => v.Id == nodeId);

		if (startNode is not null)
		{
			var successors = ontology.Successors<ModelData>(startNode, Relation.RDFSType);
			return successors.Any(v => v.Id == ancestorId);
		}

		return false;
	}

	/// <summary>
	/// Indicates whether one modelid is a descendant of another
	/// </summary>
	public static bool IsDescendantOrEqualTo(this Graph<ModelData, Relation> ontology, string nodeId, string descendantId)
	{
		if (nodeId == descendantId)
		{
			return true;
		}

		var startNode = ontology.Nodes.FirstOrDefault(v => v.Id == nodeId);

		if (startNode is not null)
		{
			var predecessors = ontology.Predecessors<ModelData>(startNode, Relation.RDFSType);
			return predecessors.Any(v => v.Id == descendantId);
		}

		return false;
	}
}
