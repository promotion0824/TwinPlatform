using System;
using System.Collections.Generic;

#pragma warning disable CS8618 // Nullable fields in DTO

namespace WillowRules.DTO;

/// <summary>
/// A simplified ModelDto for autocomplete
/// </summary>
public class ModelSimpleDto : IEquatable<ModelSimpleDto>
{
	/// <summary>
	/// A numeric index to make graphs easier
	/// </summary>
	public int Id { get; init; }

	/// <summary>
	/// A model id for searching
	/// </summary>
	public string ModelId { get; init; }

	/// <summary>
	/// A label for the drop down
	/// </summary>
	/// <remarks>
	/// Currently just the first language string, prefering 'en'
	/// When UI is multilingual stop using this field
	/// </remarks>
	public string Label { get; init; }

	///<summary>
	///     A language dictionary that contains the localized display names as specified
	///     in the model definition.
	///</summary>
	public IReadOnlyDictionary<string, string> LanguageDisplayNames { get; init; }

	///<summary>
	///     A language dictionary that contains the localized descriptions as specified in
	///     the model definition.
	///</summary>
	public IReadOnlyDictionary<string, string> LanguageDescriptions { get; init; }

	/// <summary>
	/// Gets or sets the aggregated set of units in use for this model type
	/// </summary>
	public IList<string> Units { get; set; } = new List<string>();

	/// <summary>
	/// Has the model been decommissioned
	/// </summary>
	public bool Decommissioned { get; init; }

	/// <summary>
	/// How many of them there are in the building as this direct type
	/// </summary>
	public int Count { get; init; }

	/// <summary>
	/// How many of them there are in the building that are inherited from model but not model itself
	/// </summary>
	/// <remarks>
	/// Set post-construction
	/// </remarks>
	public int CountInherited { get; set; }

	/// <summary>
	/// The sum of Count and CountInherited
	/// </summary>
	public int Total => Count + CountInherited;

	/// <summary>
	/// Is this model an ancestor, decendant of or equal to the capability model
	/// </summary>
	public bool IsCapability { get; init; }

	/// <summary>
	/// Equatable compares only by Id assumes you have identity mapped on that
	/// </summary>
	public bool Equals(ModelSimpleDto? other)
	{
		return this.Id.Equals(other?.Id ?? 0);
	}
}

#pragma warning restore CS8618 // Nullable fields in DTO
