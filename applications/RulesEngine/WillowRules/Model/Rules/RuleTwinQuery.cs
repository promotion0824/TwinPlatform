// POCO class
#nullable disable

using System;

namespace Willow.Rules.Model;

/// <summary>
/// A query for a subgraph of the Twin graph
/// </summary>
/// <remarks>
/// Initially the only kind of query we support is a single DTMID, e.g. ...AirHandler;1
/// Next we want to find X where X has a capability Y
/// </remarks>
public class RuleTwinQuery
{
	/// <summary>
	/// Id for relational database
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	/// Etag for regeneration
	/// </summary>
	public string ETag { get; set; }

	/// <summary>
	/// Gets the twin query
	/// </summary>
	/// <remarks>
	/// This may need to change to either a method that makes multiple calls to get models and relationships
	/// or some new query language as the MSFT cypher-like query is incapable of what we need.
	/// </remarks>
	public string ADTTwinQuery { get; set; }

	/// <summary>
	/// Serialize to string for debugging
	/// </summary>
	public string Serialize()
	{
		return ADTTwinQuery;
	}

	/// <summary>
	/// Model Ids involved in Query, primary model Id at index []zero
	/// </summary>
	/// <remarks>
	/// First version supports just one model id
	/// </remarks>
	public string ModelIds { get; init; }

	/// <summary>
	/// Creates a new <see cref="RuleTwinQuery" />
	/// </summary>
	protected RuleTwinQuery()
	{
		this.Id = Guid.NewGuid().ToString();
	}

	/// <summary>
	/// Query for single modelId
	/// </summary>
	public static RuleTwinQuery ByModelId(string modelId) => new RuleTwinQuery
	{
		ADTTwinQuery = $"SELECT * FROM DIGITALTWINS DT WHERE IS_OF_MODEL(DT, '{modelId}')",
		ETag = modelId,
		ModelIds = modelId
	};
}
