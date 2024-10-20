using System;
using Willow.Rules.Model;
using System.Collections.Generic;
using Abodit.Mutable;
using System.Linq;

namespace Willow.Rules.Services;

/// <summary>
/// TwinDto together the graph context around it and a flattened hierarchy, feeds and isFedBy
/// </summary>
public class TwinDataContext
{
	private TwinDataContext(BasicDigitalTwinPoco twin, Graph<BasicDigitalTwinPoco, WillowRelation> graph)
	{
		Twin = twin ?? throw new ArgumentNullException(nameof(twin));
		Graph = graph ?? throw new ArgumentNullException(nameof(graph));
	}

	public string? TimeZone { get; private set; } = null;

	public int CapabilityCount { get; private set; }

	public List<string> FeedIds { get; private set; } = new List<string>();

	public List<string> FedByIds { get; private set; } = new List<string>();

	/// <summary>
	/// Is this Twin an excluded building?
	/// </summary>
	public bool IsExcluded { get; private set; }

	/// <summary>
	/// Twin is considered Commissioned based on either TrendID or Capabilities with TrendIDs
	/// </summary>
	public bool IsCommissioned { get; private set; }

	public BasicDigitalTwinPoco Twin { get; private set; }

	/// <summary>
	/// System graph centered on the primary model Id
	/// </summary>
	public Graph<BasicDigitalTwinPoco, WillowRelation> Graph { get; }

	/// <summary>
	/// The twins starting node in the graph
	/// </summary>
	public BasicDigitalTwinPoco? StartNode { get; private set; }

	/// <summary>
	/// Creates a new TwinDataContext flattening the system graph into feeds and isFedBy and location ancestors
	/// </summary>
	public static TwinDataContext Create(
		BasicDigitalTwinPoco twin,
		Graph<BasicDigitalTwinPoco, WillowRelation> graph)
	{
		var context = new TwinDataContext(twin, graph);

		// Find the start node in the system graph around it
		context.StartNode = graph.Nodes.FirstOrDefault(x => x.Id == context.Twin.Id);

		if (context.StartNode != null)
		{
			var locationFilter = (BasicDigitalTwinPoco s, WillowRelation p, BasicDigitalTwinPoco e) => (p.Name == "isPartOf" || p.Name == "locatedIn");

			var locations = graph
				.Successors<BasicDigitalTwinPoco>(context.StartNode!, locationFilter);

			// First (any) twin up the graph with a timezone on it
			context.TimeZone = locations.Nodes
				.Select(x => x.TimeZone?.Name).FirstOrDefault(x => !string.IsNullOrEmpty(x));

			//try to find any timezone
			if (string.IsNullOrEmpty(context.TimeZone))
			{
				context.TimeZone = graph.Nodes.FirstOrDefault(v => !string.IsNullOrEmpty(v.TimeZone?.Name))?.TimeZone?.Name;
			}

			var feeds = graph
				.Successors<BasicDigitalTwinPoco>(context.StartNode!, (s, p, e) => (p.Name == "feeds"));

			context.FeedIds = feeds.Select(v => v.Id).ToList();

			var fedBy = graph
				.Predecessors<BasicDigitalTwinPoco>(context.StartNode!, (s, p, e) => (p.Name == "feeds"));

			context.FedByIds = fedBy.Select(v => v.Id).ToList();

			context.CapabilityCount = graph
				.Predecessors<BasicDigitalTwinPoco>(context.StartNode!, (s, p, e) => (p.Name == "isCapabilityOf"))
				.Count();
		}

		context.IsExcluded = false;
		context.IsCommissioned = true;   // we used to attempt to calculate this but not working

		return context;
	}
}
