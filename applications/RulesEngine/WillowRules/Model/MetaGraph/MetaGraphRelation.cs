using Abodit.Graph;
using System;
using System.Collections.Generic;

namespace Willow.Rules.Model;

public class MetaGraphRelation : IRelation, IEquatable<MetaGraphRelation>, IDotGraphEdge,
	IDotGraphEdgeThickness, IDotGraphEdgeColor
{

	/// <summary>
	/// Constructor for de-serialization only
	/// </summary>
	public MetaGraphRelation()
	{
		this.Relation = "";
		this.Substance = "";
	}

	/// <summary>
	/// Creates a new <see cref="MetaGraphRelation"/>
	/// </summary>
	public MetaGraphRelation(int startId, int endId, string relation, string substance)
	{
		this.StartId = startId;
		this.EndId = endId;
		this.Relation = relation;
		this.Substance = substance;
		this.Count = 1;
	}

	/// <summary>
	/// Is this reflexive
	/// </summary>
	public bool IsReflexive => false;

	public int StartId { get; set; }

	public int EndId { get; set; }

	/// <summary>
	/// Name of relation
	/// </summary>
	public string Relation { get; set; }

	/// <summary>
	/// Air, Water etc.
	/// </summary>
	public string Substance { get; set; }

	/// <summary>
	/// Count of relation
	/// </summary>
	public int Count { get; set; }

	public string DotLabel => $"{this.Relation} {this.Substance} ({this.Count})";

	public int Thickness => 1 + (int)(Math.Log2(this.Count));

	private static Dictionary<string, string> colors = new Dictionary<string, string>
	{
		["isCapabilityOf"] = "orange",
		["includedIn"] = "turquoise",
		["locatedIn"] = "purple",
		["manufacturedBy"] = "yellow",
		["isFedBy"] = "blue",
		["isPartOf"] = "green",
		["comprises"] = "blue",
		["feeds"] = "grey",
		["feedsACElec"] = "orange",
		["feedsWater"] = "blue",
		["feedsSprinklerWater"] = "turquoise",
		["feedsCondenserWater"] = "blue",
		["feedsCondensate"] = "blue",
		["feedsMakeupWater"] = "blue",
		["feedsSteam"] = "gray",
		["feedsIrrigationWater"] = "blue",
		["feedsStormDrainage"] = "blue",
		["feedsChilledWater"] = "blue",
		["feedsRefrig"] = "blue",
		["feedsColdDomesticWater"] = "blue",
		["feedsHotWater"] = "red",
		["feedsSupplyAir"] = "cyan",
		["feedsReturnAir"] = "cyan",
		["feedsOutsideAir"] = "cyan",
		["feedsAir"] = "cyan",
		["feedsMech"] = "brown",
		["feedsDriveElec"] = "brown",
		["feedsGas"] = "yellow",
		["feedsFuelOil"] = "yellow",
		["servedBy"] = "turquoise",
	};

	public string Color =>
		colors.TryGetValue(this.Relation + this.Substance, out string? col) ? col : "grey";

	/// <inheritdoc/>
	public bool Equals(MetaGraphRelation? other)
	{
		return other is MetaGraphRelation m && (m.StartId, m.EndId, m.Relation, m.Substance) ==
			(this.StartId, this.EndId, this.Relation, this.Substance);
	}
}
