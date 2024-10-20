using Abodit.Graph;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Willow.Rules.Model;

/// <summary>
/// Relation used in graphs (name and substance) - usually identity mapped
/// </summary>
public class WillowRelation : IEquatable<WillowRelation>, IRelation
{
	// Could be called from two aspnetcore threads at the same time
	// Note: thread-safe initialization of the various static values
	private static readonly ConcurrentDictionary<(string name, string substance), WillowRelation> identityMap = new();

	/// <summary>
	/// isPartOf
	/// </summary>
	public static WillowRelation isPartOf => isPartOfLazy.Value;

	private static readonly Lazy<WillowRelation> isPartOfLazy = new(() => WillowRelation.Get("isPartOf", ""));

	/// <summary>
	/// isContainedIn
	/// </summary>
	public static WillowRelation isContainedIn => isContainedInLazy.Value;

	private static readonly Lazy<WillowRelation> isContainedInLazy = new(() => WillowRelation.Get("isContainedIn", ""));

	/// <summary>
	/// isCapabilityOf
	/// </summary>
	public static WillowRelation isCapabilityOf => isCapabilityOfLazy.Value;

	private static readonly Lazy<WillowRelation> isCapabilityOfLazy = new(() => WillowRelation.Get("isCapabilityOf", ""));

	/// <summary>
	/// isPartOf and isContainedIn represent the spatial hierarchy
	/// </summary>
	public static readonly WillowRelation[] spatialAncestor = new[] { WillowRelation.isPartOf, WillowRelation.isContainedIn };

	/// <summary>
	/// Name
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	/// Substance
	/// </summary>
	public string Substance { get; set; }

	public bool IsReflexive => false;

	// Force use of factory method
	private WillowRelation() { Name = "NOT IN USE"; Substance = ""; }

	/// <summary>
	/// Gets an identity-mapped <see cref="WillowRelation"/>
	/// </summary>
	public static WillowRelation Get(string name, string substance)
	{
		if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

		var result = identityMap.GetOrAdd((name, substance), (props) =>
		{
			return new WillowRelation { Name = props.name, Substance = props.substance };
		});

		return result;
	}

	/// <summary>
	/// Same Name and if substance is set on either, same substance
	/// </summary>
	public bool Equals(WillowRelation? other) =>
		(this.Name, this.Substance) == (other?.Name, other?.Substance)
		|| (this.Name == other?.Name &&
			(string.IsNullOrEmpty(this.Substance) || string.IsNullOrEmpty(other?.Substance)));

	/// <summary>
	/// Get all matching relations by name (i.e. any substance)
	/// </summary>
	internal static WillowRelation[] GetAll(string name)
	{
		return identityMap.Values.Where(v => v.Name.Equals(name)).ToArray();
	}

	/// <summary>
	/// Equals
	/// </summary>
	public override bool Equals(object? other)
	{
		return other is WillowRelation t && Equals(t);
	}

	/// <summary>
	/// GetHashCode
	/// </summary>
	public override int GetHashCode()
	{
		return HashCode.Combine(Name, Substance);
	}
}

