// POCO class
#nullable disable

using System.Collections.Generic;
using Willow.Rules.Model;

namespace RulesEngine.UtilsApi.DTO;

/// <summary>
/// Related entity
/// </summary>
public struct RelatedEntityDto
{
	/// <summary>
	/// Id of the twin
	/// </summary>
	public string id { get; set; }

	/// <summary>
	/// Name
	/// </summary>
	public string name { get; set; }

	/// <summary>
	/// Model id
	/// </summary>
	public string modelId { get; set; }

	/// <summary>
	/// Relationship name
	/// </summary>
	public string relationship { get; set; }

	/// <summary>
	/// Substance
	/// </summary>
	public string substance { get; set; }

	/// <summary>
	/// Unit of measure
	/// </summary>
	public string unit { get; set; }
}
