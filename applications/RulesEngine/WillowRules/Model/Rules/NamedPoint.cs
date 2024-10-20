using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Serialization;
using WillowRules.RepositoryConfiguration;

namespace Willow.Rules.Model;

/// <summary>
/// A named point mapping a rule variable name to a point entity Id
/// </summary>
[DebuggerDisplay("{VariableName} {Id} {Unit}")]
public class NamedPoint : IEquatable<NamedPoint>
{
	/// <summary>
	/// This is now the Twin Id
	/// </summary>
	public string Id { get; set; } = string.Empty;

	/// <summary>
	/// The name that is used grids and expression field
	/// </summary>
	[Required]
	public string VariableName { get; set; } = string.Empty;

	/// <summary>
	/// The units from the Twin
	/// </summary>
	public string Unit { get; set; } = string.Empty;

	/// <summary>
	/// The calculated unambiguous name that used in the rule expression
	/// </summary>
	public string FullName { get; set; } = string.Empty;

	/// <summary>
	/// Model id
	/// </summary>
	public string ModelId { get; set; } = string.Empty;

	/// <summary>
	/// Parent chain by locatedIn and isPartOf
	/// </summary>
	/// <remarks>
	/// This is a flattened list sorted in the ascending direction.
	/// </remarks>
	public IList<TwinLocation> Locations { get; set; } = new List<TwinLocation>(0);

	internal NamedPoint()
	{

	}

	public NamedPoint(string id, string variableName, string unit, string modelId, IList<TwinLocation> locations)
	{
		Id = id;
		VariableName = variableName;
		FullName = variableName; //Give FullName default value to start with before calculated value is set
		//standardize the value. For example sometimes the twin has "Bool" instead of "bool"
		Unit = !string.IsNullOrEmpty(unit) ? Willow.Expressions.Unit.Get(unit).Name : unit; 
		ModelId = modelId;
		Locations = locations;
	}

	/// <summary>
	/// Gets the most suitable unique id for a point. TrendId is preferred and fallback is ExernalId_ConnectorId (if they exist)
	/// </summary>
	public static bool TryParsePointId(string trendId, string externalId, string connectorId, out string pointId)
	{
		pointId = string.Empty;

		if (!string.IsNullOrWhiteSpace(trendId) && Guid.TryParse(trendId, out var result) && result != Guid.Empty)
		{
			pointId = trendId;
			return true;
		}
		else if (!string.IsNullOrWhiteSpace(externalId) && !string.IsNullOrWhiteSpace(connectorId))
		{
			pointId = $"{externalId}_{connectorId}";
			return true;
		}

		return false;
	}

	public bool Equals(NamedPoint? other)
	{
		return other is NamedPoint p && p.Id == other.Id && p.VariableName == other.VariableName;
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as NamedPoint);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(this.Id, this.VariableName);
	}

	public override string ToString()
	{
		return $"{this.VariableName} -> {this.Id}";
	}

	/// <summary>
	/// Creates a string value of the first letter of each word for the point's name
	/// </summary>
	/// <returns></returns>
	public string ShortName()
	{
		if (!string.IsNullOrWhiteSpace(VariableName))
		{
			return new string(VariableName.Trim().Split(' ')
				.Where(v => !string.IsNullOrEmpty(v))
				.Select(s => s[0])
				.ToArray());
		}

		return Id;
	}
}
