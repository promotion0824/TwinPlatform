using System;

// Poco classes
#nullable disable

namespace Willow.Rules.Model;

/// <summary>
/// MiniTwin is used in the twin graph to avoid an issue with string and FromBson
/// </summary>
public class MiniTwinDto : IEquatable<MiniTwinDto>
{
	public string Id { get; set; }

	/// <summary>
	/// IEquatable Equals implementation
	/// </summary>
	public bool Equals(MiniTwinDto other) => this.Id.Equals(other.Id);

	public override bool Equals(object other) => other is MiniTwinDto oth && this.Id.Equals(oth.Id);
	public override int GetHashCode() => this.Id.GetHashCode();

	public static bool operator ==(MiniTwinDto left, MiniTwinDto right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(MiniTwinDto left, MiniTwinDto right)
	{
		return !left.Equals(right);
	}
}

