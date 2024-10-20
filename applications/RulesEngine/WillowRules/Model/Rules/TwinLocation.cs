using Newtonsoft.Json;
using System;
using System.Diagnostics;
using WillowRules.RepositoryConfiguration;

namespace Willow.Rules.Model;

/// <summary>
/// Twin location details
/// </summary>
[DebuggerDisplay("{Name} {Id} {ModelId}")]
public class TwinLocation : IEquatable<TwinLocation>
{
	/// <summary>
	/// Twin Id
	/// </summary>
	public string Id { get; set; } = string.Empty;

	/// <summary>
	/// Twin Name
	/// </summary>
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// Model id
	/// </summary>
	public string ModelId { get; set; } = string.Empty;

	internal TwinLocation()
	{
		
	}

	public TwinLocation(string id, string name, string modelId)
	{
		Id = id;
		Name = name ?? id;
		ModelId = modelId;
	}

	public bool Equals(TwinLocation? other)
	{
		return other is TwinLocation p && p.Id == other.Id && p.Name == other.Name;
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as TwinLocation);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(this.Id, this.Name);
	}

	public override string ToString()
	{
		return $"{this.Name} -> {this.Id}";
	}
}
