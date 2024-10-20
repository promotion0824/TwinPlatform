using Newtonsoft.Json;
using System;

#nullable disable  // just a poco

namespace Willow.Rules.Model;

/// <summary>
/// A <see cref="InsightDependency"/> signifies a relationship between two insights
/// </summary>
public class InsightDependency
{
	/// <summary>
	/// Creates a new <see cref="InsightDependency"/> (for deserialization)
	/// </summary>
	[JsonConstructor]
	private InsightDependency()
	{
	}

	/// <summary>
	/// Creates a new <see cref="InsightDependency"/>
	/// </summary>
	public InsightDependency(string relationship, string insightId)
	{
		Relationship = relationship ?? throw new ArgumentNullException(nameof(relationship));
		InsightId = insightId ?? throw new ArgumentNullException(nameof(insightId));
	}

	/// <summary>
	/// The relationship to the referenced insight
	/// </summary>
	public string Relationship { get; init; }

	/// <summary>
	/// The referenced insight
	/// </summary>
	public string InsightId { get; init; }

	/// <summary>
	/// The Id of the insight if it exists
	/// </summary>
	/// <remarks>
	/// Starts as Guid.Empty
	/// </remarks>
	public Guid CommandInsightId { get; set; }

}
